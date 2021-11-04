using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GeneratorUI : UIElement
{
    [Header("Layout")]
    public Text InputTitle;
    public RectTransform InputPanel;
    public RectTransform MidPanel;
    public Text OutputTitle;
    public RectTransform OutputPanel;

    [Header("General")]
    public Dropdown TypeDropdown;
    public InputField AmountInput;
    public InputField StartInput;
    public Toggle HideEqualOutputs;

    [Header("Language")]
    private List<Language> Languages = new List<Language>();
    private Language ActiveLanguage;
    public Dropdown LanguageDropdown;
    public Button AddLanguageButton;
    public Text LangMainLettersText;
    public Text LangOmitLettersText;
    public Slider LangWeightSlider;
    public Text LangSliderValueText;

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
    public Button CNN_GenerateSavedButton;
    public Text CNN_InfoText;

    [Header("Save Output")]
    public Button SaveOutputButton;

    [Header("TMX")]
    public Button TmxButton;
    
    private string CurrentCategory;

    MarkovChainWordGenerator MarkovWordGenerator;
    CNNTextGenerator CNNWordGenerator;

    private List<string> CurrentOutput;

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
        CNN_GenerateSavedButton.onClick.AddListener(GenerateSavedCnnButton_OnClick);
        CNN_SkewSlider.value = 0.8f;

        LanguageDropdown.onValueChanged.AddListener(LanguageDropdown_OnValueChanged);
        LanguageDropdown.options.Add(new Dropdown.OptionData("Default"));
        LanguageDropdown.value = 1; LanguageDropdown.value = 0;
        AddLanguageButton.onClick.AddListener(AddLanguageButton_OnClick);
        LangWeightSlider.onValueChanged.AddListener(LanguageWeightSlider_OnValueChanged);
        LangWeightSlider.minValue = MarkovWordGenerator.MinLanguageWeight;
        LangWeightSlider.maxValue = MarkovWordGenerator.MaxLanguageWeight;
        LangWeightSlider.value = MarkovWordGenerator.MinLanguageWeight;
        LanguageWeightSlider_OnValueChanged(LangWeightSlider.value);

        foreach (string s in InputDataReader.WordCategories.Keys) TypeDropdown.options.Add(new Dropdown.OptionData(s));
        TypeDropdown.onValueChanged.AddListener(TypeDropdown_OnValueChanged);
        TypeDropdown.value = 1; TypeDropdown.value = 0;

        AmountInput.text = "10";

        SaveOutputButton.onClick.AddListener(SaveOutputButton_OnClick);
    }

    void Update()
    {
        if(CNN_TrainToggle.isOn)
        {
            CNNWordGenerator.TrainOnce(CurrentCategory);
            CNN_TrainIterationsText.text = CNNWordGenerator.TrainingIterations[CurrentCategory].ToString();
        }
    }

    // Used in editor by main menu button
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(sceneName: "MainMenu");
    }

    public void UpdateUI(List<string> inputs, List<string> outputs)
    {
        Clear();
        UpdatePanel(inputs.Take(200).ToList(), InputPanel, InputTitle, "Inputs (" + inputs.Count + ")", 12, 40);
        UpdatePanel(outputs, OutputPanel, OutputTitle, "Outputs (" + outputs.Count + ")", 20, 30);
    }

    private void UpdatePanel(List<string> inputs, RectTransform parent, Text title, string titleText, int fontSize, int nRows)
    {
        float titleHeight = 0.05f;
        title.text = titleText;

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
            AddText(input, fontSize, MarkovWordGenerator.InputWords[CurrentCategory].Contains(input) ? Color.white : new Color(0.6f, 1f, 0.6f), FontStyle.Normal, col * xStep + xMargin, titleHeight + (row * yStep), (col + 1) * xStep - xMargin, titleHeight + ((row + 1) * yStep), parent, TextAnchor.UpperLeft);
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

    private void LanguageDropdown_OnValueChanged(int value)
    {
        if(value == 0)
        {
            LangMainLettersText.text = "";
            LangOmitLettersText.text = "";
            ActiveLanguage = null;
        }
        else
        {
            ActiveLanguage = Languages[value - 1];
            LangMainLettersText.text = ActiveLanguage.GetMainLetters();
            LangOmitLettersText.text = ActiveLanguage.GetOmittedLetters();
        }
    }

    private void AddLanguageButton_OnClick()
    {
        Language newLanguage = Language.GetRandomLanguage(MarkovWordGenerator);
        Languages.Add(newLanguage);
        LanguageDropdown.options.Add(new Dropdown.OptionData(newLanguage.Name));
        LanguageDropdown.value = Languages.Count;
    }

    private void LanguageWeightSlider_OnValueChanged(float value)
    {
        LangSliderValueText.text = value.ToString();
    }

    private void SkewFactorSlider_OnValueChanged(float value)
    {
        CNN_SkewValueText.text = value.ToString();
    }

    private void GenerateMarkovButton_OnClick()
    {
        CurrentOutput = new List<string>();
        int iterationsWithoutNewWord = 0;
        while(CurrentOutput.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = MarkovWordGenerator.GenerateWord(CurrentCategory, (int)NGramSlider.value, StartInput.text, ActiveLanguage, (int)LangWeightSlider.value);
            if (!CurrentOutput.Contains(word) && (!HideEqualOutputs.isOn || !MarkovWordGenerator.InputWords[CurrentCategory].Contains(word)))
            {
                CurrentOutput.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;
            
        }
        UpdateUI(MarkovWordGenerator.InputWords[CurrentCategory], CurrentOutput);
    }

    private void NGramSlider_OnValueChanged(float value)
    {
        NGramSliderValueText.text = value.ToString();
    }

    private void GenerateCurrentCnnButton_OnClick()
    {
        CurrentOutput = new List<string>();
        int iterationsWithoutNewWord = 0;
        while (CurrentOutput.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = CNNWordGenerator.GenerateWord(CurrentCategory, CNN_SkewSlider.value, StartInput.text);
            if (!CurrentOutput.Contains(word) && (!HideEqualOutputs.isOn || !CNNWordGenerator.InputWords[CurrentCategory].Contains(word)))
            {
                CurrentOutput.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;

        }
        UpdateUI(CNNWordGenerator.InputWords[CurrentCategory], CurrentOutput);
    }

    private void SaveCnnButton_OnClick()
    {
        string path = "Assets/Resources/SavedNetworks/" + CurrentCategory + ".txt";
        CNN_InfoText.text = CNNWordGenerator.SaveCnn(CurrentCategory, path);
    }

    private void GenerateSavedCnnButton_OnClick()
    {
        CNN_InfoText.text = CNNWordGenerator.LoadCnn(CurrentCategory);

        if (CNNWordGenerator.LoadedNetwork == null) return;

        CurrentOutput = new List<string>();
        int iterationsWithoutNewWord = 0;
        while (CurrentOutput.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = CNNWordGenerator.GenerateLoadedNetworkWord(CNN_SkewSlider.value, StartInput.text);
            if (!CurrentOutput.Contains(word) && (!HideEqualOutputs.isOn || !CNNWordGenerator.InputWords[CurrentCategory].Contains(word)))
            {
                CurrentOutput.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;

        }
        UpdateUI(CNNWordGenerator.InputWords[CurrentCategory], CurrentOutput);
    }

    private void SaveOutputButton_OnClick()
    {
        if (CurrentOutput == null || CurrentOutput.Count == 0) return;
        string path = "Assets/Resources/output.txt";
        Debug.Log("Saving " + CurrentOutput.Count + " elements to output.txt");
        using (StringWriter writer = new StringWriter())
        {
            foreach (string s in CurrentOutput) writer.WriteLine(s);
            File.WriteAllText(path, writer.ToString());
        }
    }
}
