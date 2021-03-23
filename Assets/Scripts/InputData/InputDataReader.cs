using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class InputDataReader
{
    public static Dictionary<string, InputDataType> WordCategories = new Dictionary<string, InputDataType>{
        { "Planet", InputDataType.SingleLine},
        { "Country", InputDataType.SingleLine},
        { "Province", InputDataType.SingleLine},
        { "Gemeinde", InputDataType.SingleLine},
        { "Mineral", InputDataType.SingleLine},
        { "TrackmaniaMapNames", InputDataType.SingleLine },
        { "FaberSongs", InputDataType.SingleLine},
        { "FaberSongText", InputDataType.MultiLine},
        { "Test", InputDataType.SingleLine},
       
    };
    private const string MultiLineEndInput = "---ENDINPUT---";

    private static Dictionary<string, List<string>> InputWords;

    /// <summary>
    /// Reads all files with input data seperated by category. Can be filtered by a string of accepted chars. Then only words than only contain chars within the acceptedChars string are returned.
    /// </summary>
    /// <param name="acceptedChars"></param>
    /// <returns></returns>
    public static Dictionary<string, List<string>> GetInputWords(string acceptedChars = "", bool debug = false)
    {
        InputWords = new Dictionary<string, List<string>>();

        foreach (KeyValuePair<string, InputDataType> kvp in WordCategories)
        {
            string category = kvp.Key;
            InputDataType inputDataType = kvp.Value;

            switch (inputDataType)
            {
                case InputDataType.SingleLine:
                    ReadSingleLineInputs(category, acceptedChars);
                    if(debug) Debug.Log("Added " + InputWords[category].Count + " words from the category " + category);
                    break;

                case InputDataType.MultiLine:
                    ReadMultiLineInputs(category, acceptedChars);
                    if(debug) Debug.Log("Added " + InputWords[category].Count + " words from the category " + category);
                    break;
            }
        }

        return InputWords;
    }

    private static void ReadSingleLineInputs(string category, string acceptedChars = "")
    {
        List<string> duplicates = new List<string>();
        List<string> invalidWords = new List<string>();
        InputWords.Add(category, new List<string>());

        string line;
        System.IO.StreamReader file = new System.IO.StreamReader("Assets/Resources/InputData/" + category + ".txt", Encoding.UTF8);
        while ((line = file.ReadLine()) != null)
        {
            if (InputWords[category].Contains(line)) duplicates.Add(line);
            else if (!IsValidWord(line, acceptedChars)) invalidWords.Add(line);
            else InputWords[category].Add(line);
        }
        file.Close();

        if (duplicates.Count > 0)
        {
            string s = "";
            s += "Following entries are duplicate in input data " + category + ":\n";
            foreach (string d in duplicates) s += d + "\n";
            Debug.Log(s);
        }
        if (invalidWords.Count > 0)
        {
            string s = "";
            s += "Following entries are invalid in input data " + category + " with the acceptedChars [" + acceptedChars + "]:\n";
            foreach (string d in invalidWords) s += d + "\n";
            Debug.Log(s);
        }
    }

    private static void ReadMultiLineInputs(string category, string acceptedChars = "")
    {
        InputWords.Add(category, new List<string>());
        string line;
        string currentInput = "";
        System.IO.StreamReader file = new System.IO.StreamReader("Assets/Resources/InputData/" + category + ".txt", Encoding.GetEncoding("iso-8859-1"));
        while ((line = file.ReadLine()) != null)
        {
            if (line == MultiLineEndInput)
            {
                currentInput = currentInput.TrimEnd('\n');
                if(IsValidWord(currentInput, acceptedChars)) InputWords[category].Add(currentInput);
                currentInput = "";
            }
            else
            {
                currentInput += line + "\n";
            }
        }
        file.Close();
    }

    /// <summary>
    /// Returns if a word is valid given the set of accepted characters
    /// </summary>
    private static bool IsValidWord(string word, string acceptedChars)
    {
        if (acceptedChars == "") return true;
        return word.All(x => acceptedChars.Contains(x));
    }
}
