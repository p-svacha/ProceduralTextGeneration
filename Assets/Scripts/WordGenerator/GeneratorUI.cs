using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : UIElement
{
    [Header("Layout")]
    public RectTransform InputPanel;
    public RectTransform MidPanel;
    public RectTransform OutputPanel;

    [Header("General")]
    public Dropdown TypeDropdown;
    public InputField AmountInput;
    public InputField StartInput;
    public Toggle HideEqualOutputs;

    [Header("Markov")]
    public Slider NGramSlider;
    public Text NGramSliderValueText;
    public Button GenerateMarkovButton;

    [Header("CNN")]
    public Text CNN_TrainIterationsText;
    public Toggle CNN_TrainToggle;
    public Slider CNN_SkewSlider;
    public Text CNN_SkewValueText;
    public Button CNN_GenerateCurrentButton;
    public Button CNN_SaveCnnButton;
    public Button CNN_GenerateLoadedButton;
    public Button CNN_LoadCnnButton;
    public Text CNN_InfoText;

    [Header("TMX")]
    public Button TmxButton;
    
    private string CurrentCategory;

    MarkovChainWordGenerator MarkovWordGenerator;
    CNNTextGenerator CNNWordGenerator;

    protected override void OnStart()
    {
        InputDataReader.Init();

        MarkovWordGenerator = new MarkovChainWordGenerator();
        CNNWordGenerator = new CNNTextGenerator();
        GenerateMarkovButton.onClick.AddListener(GenerateMarkovButton_OnClick);
        TmxButton.onClick.AddListener(TMXCrawler.ReadJson);
        NGramSlider.onValueChanged.AddListener(NGramSlider_OnValueChanged);
        NGramSlider.minValue = MarkovWordGenerator.MinNGramLength;
        NGramSlider.maxValue = MarkovWordGenerator.MaxNGramLength;
        NGramSliderValueText.text = MarkovWordGenerator.MinNGramLength.ToString();

        CNN_SkewSlider.onValueChanged.AddListener(SkewFactorSlider_OnValueChanged);
        CNN_GenerateCurrentButton.onClick.AddListener(GenerateCurrentCnnButton_OnClick);
        CNN_SaveCnnButton.onClick.AddListener(SaveCnnButton_OnClick);
        CNN_GenerateLoadedButton.onClick.AddListener(GenerateLoadedCnnButton_OnClick);
        CNN_LoadCnnButton.onClick.AddListener(LoadCnnButton_OnClick);
        CNN_SkewSlider.value = 0.8f;

        foreach(string s in InputDataReader.WordCategories.Keys) TypeDropdown.options.Add(new Dropdown.OptionData(s));
        TypeDropdown.onValueChanged.AddListener(TypeDropdown_OnValueChanged);
        TypeDropdown.value = 1;
        TypeDropdown.value = 0;

        AmountInput.text = "10";
    }

    void Update()
    {
        if(CNN_TrainToggle.isOn)
        {
            CNNWordGenerator.TrainOnce(CurrentCategory);
            CNN_TrainIterationsText.text = CNNWordGenerator.TrainingIterations[CurrentCategory].ToString();
        }
    }

    public void UpdateUI(List<string> inputs, List<string> outputs)
    {
        Clear();
        UpdatePanel(inputs.Take(200).ToList(), InputPanel, 12, 40);
        UpdatePanel(outputs, OutputPanel, 20, 30);
    }

    private void UpdatePanel(List<string> inputs, RectTransform parent, int fontSize, int nRows)
    {
        float titleHeight = 0.05f;

        int row = 0;
        int col = 0;

        int nCols = inputs.Count% nRows == 0 ? inputs.Count / nRows : (inputs.Count / nRows) + 1;
        //if (nCols == 1 && inputs.Count < nRows) nRows = inputs.Count;

        float xMargin = 0.02f;
        float xStep = 1f / nCols;
        float yStep = (1 - titleHeight) / nRows;

        // Randomize list
        List<string> randomizedList = new List<string>();
        List<string> copy = new List<string>();
        foreach (string s in inputs) copy.Add(s);
        for(int i = 0; i < Mathf.Min(nRows * nCols, inputs.Count); i++)
        {
            string s = copy[Random.Range(0, copy.Count)];
            copy.Remove(s);
            randomizedList.Add(s);
        }

        foreach (string input in randomizedList)
        {
            AddText(input, fontSize, MarkovWordGenerator.InputWords[CurrentCategory].Contains(input) ? Color.black : new Color(0, 0.4f, 0), FontStyle.Normal, col * xStep + xMargin, titleHeight + (row * yStep), (col + 1) * xStep - xMargin, titleHeight + ((row + 1) * yStep), parent, TextAnchor.UpperLeft);
            row++;
            if(row == nRows)
            {
                row = 0;
                col++;
            }
        }
    }

    private void TypeDropdown_OnValueChanged(int value)
    {
        CurrentCategory = TypeDropdown.options[value].text;
        UpdateUI(CNNWordGenerator.InputWords[CurrentCategory], new List<string>());
    }

    private void SkewFactorSlider_OnValueChanged(float value)
    {
        CNN_SkewValueText.text = value.ToString();
    }

    private void GenerateMarkovButton_OnClick()
    {
        List<string> words = new List<string>();
        int iterationsWithoutNewWord = 0;
        while(words.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = MarkovWordGenerator.GenerateWord(CurrentCategory, (int)NGramSlider.value, StartInput.text);
            if (!words.Contains(word) && (!HideEqualOutputs.isOn || !MarkovWordGenerator.InputWords[CurrentCategory].Contains(word)))
            {
                words.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;
            
        }
        UpdateUI(MarkovWordGenerator.InputWords[CurrentCategory], words);
    }

    private void NGramSlider_OnValueChanged(float value)
    {
        NGramSliderValueText.text = value.ToString();
    }

    private void GenerateCurrentCnnButton_OnClick()
    {
        List<string> words = new List<string>();
        int iterationsWithoutNewWord = 0;
        while (words.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = CNNWordGenerator.GenerateWord(CurrentCategory, CNN_SkewSlider.value, StartInput.text);
            if (!words.Contains(word) && (!HideEqualOutputs.isOn || !CNNWordGenerator.InputWords[CurrentCategory].Contains(word)))
            {
                words.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;

        }
        UpdateUI(CNNWordGenerator.InputWords[CurrentCategory], words);
    }

    private void SaveCnnButton_OnClick()
    {
        //string path = "Assets/Resources/SavedNetworks/" + CurrentCategory + "_" + System.DateTime.Now.Year + "_" + System.DateTime.Now.Month + "_" + System.DateTime.Now.Day + "_" + System.DateTime.Now.Hour + "_" + System.DateTime.Now.Minute + "_" + System.DateTime.Now.Second + ".txt";
        string path = "Assets/Resources/SavedNetworks/" + CurrentCategory + ".txt";
        CNN_InfoText.text = CNNWordGenerator.SaveCnn(CurrentCategory, path);
    }

    private void GenerateLoadedCnnButton_OnClick()
    {
        if(CNNWordGenerator.LoadedNetwork == null)
        {
            CNN_InfoText.text = "No network loaded";
            return;
        }

        List<string> words = new List<string>();
        int iterationsWithoutNewWord = 0;
        while (words.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = CNNWordGenerator.GenerateLoadedNetworkWord(CNN_SkewSlider.value, StartInput.text);
            if (!words.Contains(word) && (!HideEqualOutputs.isOn || !CNNWordGenerator.InputWords[CurrentCategory].Contains(word)))
            {
                words.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;

        }
        UpdateUI(CNNWordGenerator.InputWords[CurrentCategory], words);
    }

    private void LoadCnnButton_OnClick()
    {
        CNN_InfoText.text = CNNWordGenerator.LoadCnn(CurrentCategory);
    }

}
