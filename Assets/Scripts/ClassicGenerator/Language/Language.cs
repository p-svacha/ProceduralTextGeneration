using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A language is used to influece word generation in the classic generator. It defines letters and letter clusters that appear more and some that never appear, thus simulating a language.
/// </summary>
public class Language
{
    public string Name;
    public List<string> MainLetters;
    public List<string> OmittedLetters;

    public Language(string name, List<string> mainLetters, List<string> omittedLetters)
    {
        Name = name;
        MainLetters = mainLetters;
        OmittedLetters = omittedLetters;
    }

    public string GetMainLetters()
    {
        string s = "";
        foreach (string l in MainLetters) s += l + ", ";
        return s.Substring(0, s.Length - 2);
    }

    public string GetOmittedLetters()
    {
        string s = "";
        foreach (string l in OmittedLetters) s += l + ", ";
        return s.Substring(0, s.Length - 2);
    }

    #region Language Generation

    private const int MIN_MAIN_1_LETTERS = 2;
    private const int MAX_MAIN_1_LETTERS = 5;

    private const int MIN_MAIN_2_LETTERS = 1;
    private const int MAX_MAIN_2_LETTERS = 3;

    private const int MIN_MAIN_3_LETTERS = 0;
    private const int MAX_MAIN_3_LETTERS = 1;

    private const int MIN_OMIT_1_LETTERS = 1;
    private const int MAX_OMIT_1_LETTERS = 3;

    public static Language GetRandomLanguage(MarkovChainWordGenerator gen)
    {
        List<char> usedChars = new List<char>();

        // Main letters
        List<string> mainLetters = new List<string>();
        int numMain1Letters = Random.Range(MIN_MAIN_1_LETTERS, MAX_MAIN_1_LETTERS + 1);
        int numMain2Letters = Random.Range(MIN_MAIN_2_LETTERS, MAX_MAIN_2_LETTERS + 1);
        int numMain3Letters = Random.Range(MIN_MAIN_3_LETTERS, MAX_MAIN_3_LETTERS + 1);
        for(int i = 0; i < numMain1Letters; i++)
        {
            string s = GetRandomLetters(1, usedChars);
            mainLetters.Add(s);
            foreach (char c in s) usedChars.Add(c);
        }
        for (int i = 0; i < numMain2Letters; i++)
        {
            string s = gen.GetRandomNgram("Province", 2, 5).ToLower();
            mainLetters.Add(s);
            foreach (char c in s) usedChars.Add(c);
        }
        for (int i = 0; i < numMain3Letters; i++)
        {
            string s = gen.GetRandomNgram("Province", 3, 3).ToLower();
            mainLetters.Add(s);
            foreach (char c in s) usedChars.Add(c);
        }

        // Omitted letters
        List<string> omittedLetters = new List<string>();
        int numOmit1Letters = Random.Range(MIN_OMIT_1_LETTERS, MAX_OMIT_1_LETTERS + 1);
        for (int i = 0; i < numOmit1Letters; i++)
        {
            string s = GetRandomLetters(1, usedChars);
            omittedLetters.Add(s);
            foreach (char c in s) usedChars.Add(c);
        }

        return new Language(gen.GenerateWord("Language", 5), mainLetters, omittedLetters);
    }

    private static string GetRandomLetters(int length, List<char> forbiddenChars)
    {
        string chars = "abcdefghijklmnopqrstuvwxyz";
        string s = new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Range(0, s.Length)]).ToArray());
        while(forbiddenChars.Any(x => s.Contains(x))) s = new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Range(0, s.Length)]).ToArray());
        return s;
    }


    #endregion
}
