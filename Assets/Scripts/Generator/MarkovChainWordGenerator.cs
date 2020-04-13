﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarkovChainWordGenerator
{
    public static Dictionary<string, InputDataType> WordCategories = new Dictionary<string, InputDataType>{
        { "Planet", InputDataType.SingleLine},
        { "Country", InputDataType.SingleLine},
        { "Gemeinde", InputDataType.SingleLine},
        { "Mineral", InputDataType.SingleLine},
        { "FaberSongs", InputDataType.SingleLine},
        { "FaberSongText", InputDataType.MultiLine},
        { "Test", InputDataType.SingleLine},
    };

    private const char WordStartChar = '>';
    private const char WordEndChar = '<';
    private const string MultiLineEndInput = "---ENDINPUT---";

    public int MinNGramLength = 2;
    public int MaxNGramLength = 6;

    /// <summary>
    /// The initial key represents the word type (i.e. "Planet" or "Country")
    /// The key of the first dictionary is the nGram length.
    /// The final dictionary: key is the ngram string and the int the amount of occurences in the input data set
    /// </summary>
    private Dictionary<string, Dictionary<int, Dictionary<string, int>>> NGrams;

    /// <summary>
    /// Key is the word type  (i.e. "Planet" or "Country")
    /// Value is a list with all input words
    /// </summary>
    public Dictionary<string, List<string>> InputWords;

    public MarkovChainWordGenerator()
    {
        NGrams = new Dictionary<string, Dictionary<int, Dictionary<string, int>>>();
        InputWords = new Dictionary<string, List<string>>();

        foreach (KeyValuePair<string, InputDataType> kvp in WordCategories)
        {
            string category = kvp.Key;
            InputDataType inputDataType = kvp.Value;
            NGrams.Add(category, new Dictionary<int, Dictionary<string, int>>());

            switch (inputDataType)
            {
                case InputDataType.SingleLine:
                    ReadSingleLineInputs(category);
                    break;

                case InputDataType.MultiLine:
                    ReadMultiLineInputs(category);
                    break;
            }
        }
    }

    public string GenerateWord(string wordType, int nGramLength)
    {
        string word = WordStartChar + "";

        while (!word.EndsWith(WordEndChar + ""))
        {
            //Debug.Log(word);
            if (word.Length < (nGramLength - 1))
            {
                int startIndex = 0;
                int length = word.Length;
                string nGram = PickRandomNGramStartingWith(wordType, word.Substring(startIndex, length), nGramLength);
                word += nGram[word.Length];
            }
            else
            {
                int startIndex = word.Length - (nGramLength - 1);
                int length = nGramLength - 1;
                string nGram = PickRandomNGramStartingWith(wordType, word.Substring(startIndex, length), nGramLength);
                word += nGram[nGramLength - 1];
            }
        }

        return word.Substring(1, word.Length - 2); // Remove word start and end char
    }

    private void ReadSingleLineInputs(string category)
    {
        List<string> duplicates = new List<string>();
        InputWords.Add(category, new List<string>());

        string line;
        System.IO.StreamReader file = new System.IO.StreamReader("Assets/Resources/InputData/" + category + ".txt", System.Text.Encoding.GetEncoding("iso-8859-1"));
        while ((line = file.ReadLine()) != null)
        {
            if (!InputWords[category].Contains(line))
            {
                for (int i = MinNGramLength; i <= MaxNGramLength; i++)
                {
                    CreateNGramsFor(category, line, i);
                }
                InputWords[category].Add(line);
            }
            else duplicates.Add(line);
        }
        file.Close();

        if (duplicates.Count > 0)
        {
            string s = "";
            s += "Following entries or duplicate in input data " + category + ":\n";
            foreach (string d in duplicates) s += d + "\n";
            Debug.Log(s);
        }
    }

    private void ReadMultiLineInputs(string category)
    {
        InputWords.Add(category, new List<string>());
        string line;
        string currentInput = "";
        System.IO.StreamReader file = new System.IO.StreamReader("Assets/Resources/InputData/" + category + ".txt", System.Text.Encoding.GetEncoding("iso-8859-1"));
        while ((line = file.ReadLine()) != null)
        {
            if (line == MultiLineEndInput)
            {
                currentInput = currentInput.TrimEnd('\n');
                for (int i = MinNGramLength; i <= MaxNGramLength; i++)
                {
                    CreateNGramsFor(category, currentInput, i);
                }
                InputWords[category].Add(currentInput);
                currentInput = "";
            }
            else
            {
                currentInput += line + "\n";
            }
        }
        file.Close();
    }

    private string PickRandomNGramStartingWith(string wordType, string nGramStart, int nGramLength)
    {
        Dictionary<string, int> candidateNGrams = NGrams[wordType][nGramLength].Where(x => x.Key.StartsWith(nGramStart)).ToDictionary(x => x.Key, x => x.Value);
        int totalProbability = candidateNGrams.Sum(x => x.Value);

        // Create array where each ngram has as many occurences as it has in the original list
        string[] weightedArray = new string[totalProbability];
        int id = 0;
        foreach(KeyValuePair<string, int> kvp in candidateNGrams)
            for (int i = 0; i < kvp.Value; i++) weightedArray[id++] = kvp.Key;

        // Chose one random entry in the weighted array
        if (weightedArray.Length == 0) throw new Exception("No nGram found that starts with " + nGramStart);
        string chosenNgram = weightedArray[UnityEngine.Random.Range(0, weightedArray.Length)];
        //Debug.Log("Chosen nGram: " + chosenNgram + " (out of " + candidateNGrams.Count + " options)");
        return chosenNgram;
    }

    private void CreateNGramsFor(string wordType, string word, int nGramLength)
    {
        if (word.Length < (nGramLength - 1)) return; // Skip word if shorter than nGramLength - 1
        AddNGram(wordType, WordStartChar + word.Substring(0, nGramLength - 1), nGramLength);
        for(int i = 0; i < word.Length; i++)
        {
            if (i < word.Length - (nGramLength - 1)) AddNGram(wordType, word.Substring(i, nGramLength), nGramLength);
            else AddNGram(wordType, word.Substring(i, word.Length - i) + WordEndChar, nGramLength);
        }
    }

    private void AddNGram(string wordType, string ngram, int nGramLength)
    {
        if (!NGrams[wordType].ContainsKey(nGramLength)) NGrams[wordType].Add(nGramLength, new Dictionary<string, int>());
        if (NGrams[wordType][nGramLength].ContainsKey(ngram)) NGrams[wordType][nGramLength][ngram]++;
        else NGrams[wordType][nGramLength].Add(ngram, 1);
    }
}