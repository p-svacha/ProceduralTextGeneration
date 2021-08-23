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
        //{ "TrackmaniaMapNames", InputDataType.SingleLine },
        //{ "FaberSongs", InputDataType.SingleLine},
        //{ "FaberSongText", InputDataType.MultiLine},
        { "Usernames", InputDataType.SingleLine},
        //{ "Test", InputDataType.SingleLine},
       
    };
    private const string MultiLineEndInput = "---ENDINPUT---";

    private static Dictionary<string, List<string>> InputWords;

    public static void Init()
    {
        InputWords = new Dictionary<string, List<string>>();

        foreach (KeyValuePair<string, InputDataType> kvp in WordCategories)
        {
            string category = kvp.Key;
            InputDataType inputDataType = kvp.Value;
            Debug.Log("############### READING INPUT DATA >>>" + category + "<<< ###############");

            switch (inputDataType)
            {
                case InputDataType.SingleLine:
                    ReadSingleLineInputs(category);
                    break;

                case InputDataType.MultiLine:
                    ReadMultiLineInputs(category);
                    break;
            }

            Debug.Log("Added " + InputWords[category].Count + " words from the category " + category);
        }
    }

    /// <summary>
    /// Reads all files with input data seperated by category. Can be filtered by a string of accepted chars. Then only words than only contain chars within the acceptedChars string are returned.
    /// </summary>
    /// <param name="acceptedChars"></param>
    /// <returns></returns>
    public static List<string> GetInputWords(string category, string acceptedChars = "")
    {
        List<string> words = new List<string>();
        List<string> invalidWords = new List<string>();

        foreach(string word in InputWords[category])
        {
            if (IsValidWord(word, acceptedChars)) words.Add(word);
            else invalidWords.Add(word);
        }

        if (invalidWords.Count > 0)
        {
            string s = "";
            s += category + ": " + invalidWords.Count + " entries are invalid with the acceptedChars [" + acceptedChars + "]:\n";
            foreach (string d in invalidWords) s += d + "\n";
            Debug.LogWarning(s);
        }

        return words;
    }

    private static void ReadSingleLineInputs(string category)
    {
        List<string> duplicates = new List<string>();
        Dictionary<string, int> charOccurences = new Dictionary<string, int>();
        InputWords.Add(category, new List<string>());

        string line;
        System.IO.StreamReader file = new System.IO.StreamReader("Assets/Resources/InputData/" + category + ".txt", Encoding.UTF8);
        while ((line = file.ReadLine()) != null)
        {
            foreach(char c in line)
            {
                string s = c.ToString();
                if (charOccurences.ContainsKey(s)) charOccurences[s]++;
                else charOccurences.Add(s, 1);
            }
            if (InputWords[category].Contains(line)) duplicates.Add(line);
            else InputWords[category].Add(line);
        }
        file.Close();

        if (duplicates.Count > 0)
        {
            string s = "";
            s += category + ": " + duplicates.Count + " entries are duplicate:\n";
            foreach (string d in duplicates) s += d + "\n";
            Debug.LogWarning(s);
        }
        string charString = category + ": Character occurences ordered from most occuring to least occuring:";
        foreach (KeyValuePair<string, int> kvp in charOccurences.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, y => y.Value))
        {
            charString += "\n" + kvp.Key + ": " + kvp.Value;
        }
        Debug.Log(charString);
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
