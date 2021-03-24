using System.Collections;
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
    public Button CNN_GenerateButton;

    [Header("TMX")]
    public Button TmxButton;
    
    private string WordType;

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
        CNN_GenerateButton.onClick.AddListener(GenerateCNNButton_OnClick);
        CNN_SkewSlider.value = 0.8f;

        foreach(string s in InputDataReader.WordCategories.Keys) TypeDropdown.options.Add(new Dropdown.OptionData(s));
        TypeDropdown.onValueChanged.AddListener(TypeDropdown_OnValueChanged);
        TypeDropdown.value = 1;
        TypeDropdown.value = 0;

        AmountInput.text = "30";
    }

    void Update()
    {
        if(CNN_TrainToggle.isOn)
        {
            CNNWordGenerator.TrainOnce(WordType);
            CNN_TrainIterationsText.text = CNNWordGenerator.TrainingIterations[WordType].ToString();
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
            AddText(input, fontSize, MarkovWordGenerator.InputWords[WordType].Contains(input) ? Color.black : new Color(0, 0.4f, 0), FontStyle.Normal, col * xStep + xMargin, titleHeight + (row * yStep), (col + 1) * xStep - xMargin, titleHeight + ((row + 1) * yStep), parent, TextAnchor.UpperLeft);
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
        WordType = TypeDropdown.options[value].text;
        UpdateUI(CNNWordGenerator.InputWords[WordType], new List<string>());
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
            string word = MarkovWordGenerator.GenerateWord(WordType, (int)NGramSlider.value, StartInput.text);
            if (!words.Contains(word) && (!HideEqualOutputs.isOn || !MarkovWordGenerator.InputWords[WordType].Contains(word)))
            {
                words.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;
            
        }
        UpdateUI(MarkovWordGenerator.InputWords[WordType], words);
    }

    private void NGramSlider_OnValueChanged(float value)
    {
        NGramSliderValueText.text = value.ToString();
    }

    private void GenerateCNNButton_OnClick()
    {
        List<string> words = new List<string>();
        int iterationsWithoutNewWord = 0;
        while (words.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = CNNWordGenerator.GenerateWord(WordType, CNN_SkewSlider.value, StartInput.text);
            if (!words.Contains(word) && (!HideEqualOutputs.isOn || !CNNWordGenerator.InputWords[WordType].Contains(word)))
            {
                words.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;

        }
        UpdateUI(CNNWordGenerator.InputWords[WordType], words);
    }

}
