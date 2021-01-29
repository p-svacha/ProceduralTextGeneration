using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : UIElement
{
    public RectTransform InputPanel;
    public RectTransform OutputPanel;
    public Button GenerateButton;

    public Button TmxButton;

    public Slider NGramSlider;
    public Text NGramSliderValueText;

    public Toggle HideEqualOutputs;

    public InputField AmountInput;

    public Dropdown TypeDropdown;
    public string WordType;

    MarkovChainWordGenerator WordGenerator;

    protected override void OnStart()
    {
        WordGenerator = new MarkovChainWordGenerator();
        GenerateButton.onClick.AddListener(GenerateButton_OnClick);
        TmxButton.onClick.AddListener(TMXCrawler.ReadJson);
        NGramSlider.onValueChanged.AddListener(delegate { NGramSlider_OnValueChanged(); });
        NGramSlider.minValue = WordGenerator.MinNGramLength;
        NGramSlider.maxValue = WordGenerator.MaxNGramLength;
        NGramSliderValueText.text = WordGenerator.MinNGramLength.ToString();

        foreach(string s in MarkovChainWordGenerator.WordCategories.Keys) TypeDropdown.options.Add(new Dropdown.OptionData(s));
        TypeDropdown.value = 1;
        TypeDropdown.value = 0;

        AmountInput.text = "30";
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
            AddText(input, fontSize, WordGenerator.InputWords[WordType].Contains(input) ? Color.black : new Color(0, 0.4f, 0), FontStyle.Normal, col * xStep + xMargin, titleHeight + (row * yStep), (col + 1) * xStep - xMargin, titleHeight + ((row + 1) * yStep), parent, TextAnchor.UpperLeft);
            row++;
            if(row == nRows)
            {
                row = 0;
                col++;
            }
        }
    }

    private void GenerateButton_OnClick()
    {
        WordType = TypeDropdown.options[TypeDropdown.value].text;
        List<string> words = new List<string>();
        int iterationsWithoutNewWord = 0;
        while(words.Count < int.Parse(AmountInput.text) && iterationsWithoutNewWord < 100)
        {
            string word = WordGenerator.GenerateWord(WordType, (int)NGramSlider.value);
            if (!words.Contains(word) && (!HideEqualOutputs.isOn || !WordGenerator.InputWords[WordType].Contains(word)))
            {
                words.Add(word);
                iterationsWithoutNewWord = 0;
            }
            else iterationsWithoutNewWord++;
            
        }
        UpdateUI(WordGenerator.InputWords[WordType], words);
    }

    private void NGramSlider_OnValueChanged()
    {
        NGramSliderValueText.text = NGramSlider.value.ToString();
    }

}
