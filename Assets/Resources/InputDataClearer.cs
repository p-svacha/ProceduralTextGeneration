using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class InputDataClearer : MonoBehaviour
{
    void Start()
    {
        //TakeFirstTab("Assets/Resources/InputData/Gemeinde.txt");
        //ReplaceDoubleS("Assets/Resources/InputData/FaberSongText.txt");
        //ParseProvince("Assets/Resources/InputData/Province.txt");
    }


    private void ReplaceDoubleS(string path)
    {
        List<string> newLines = new List<string>();

        string text = File.ReadAllText(path, System.Text.Encoding.GetEncoding("iso-8859-1"));
        text = text.Replace("ß", "ss");
        File.WriteAllText(path, text, System.Text.Encoding.GetEncoding("iso-8859-1"));
    }

    private void TakeFirstTab(string path)
    {
        List<string> lines = new List<string>();

        string line;
        System.IO.StreamReader file = new System.IO.StreamReader(path, System.Text.Encoding.GetEncoding("iso-8859-1"));
        while ((line = file.ReadLine()) != null)
        {
            if (line.Trim() != "")
            {
                string gemeinde = line.Split('\t')[0];
                lines.Add(gemeinde);
            }
        }
        file.Close();

        using (System.IO.StreamWriter file2 =
            new System.IO.StreamWriter("Assets/Resources/InputData/tmp.txt"))
        {
            foreach (string line2 in lines)
            {
                file2.WriteLine(line2);
            }
        }
    }

    /// <summary>
    /// Replaces the given char with a new line
    /// </summary>
    private void ParseProvince(string path)
    {
        System.IO.StreamReader file = new System.IO.StreamReader(path, System.Text.Encoding.GetEncoding("iso-8859-1"));
        string text = file.ReadToEnd();
        file.Close();

        // Remove white space
        text = text.Trim();

        // Remove weird characters
        text = text.Replace("*", "");
        text = text.Replace(")", "");
        text = text.Replace("]", "");

        // Make new lines
        text = text.Replace(',', '\n');
        text = text.Replace(';', '\n');
        text = text.Replace('(', '\n');
        text = text.Replace('[', '\n');
        text = text.Replace('/', '\n');

        using (System.IO.StreamWriter file2 =
            new System.IO.StreamWriter("Assets/Resources/InputData/tmp.txt"))
        {
            file2.Write(text);
        }

        // Remove white space from each line
        List<string> lines = new List<string>();
        string line;
        System.IO.StreamReader file3 = new System.IO.StreamReader("Assets/Resources/InputData/tmp.txt", System.Text.Encoding.GetEncoding("iso-8859-1"));
        while ((line = file3.ReadLine()) != null)
        {
            if (line.Trim() != "")
            {
                lines.Add(line.Trim());
            }
        }
        file3.Close();

        using (System.IO.StreamWriter file4 = new System.IO.StreamWriter("Assets/Resources/InputData/tmp2.txt"))
        {
            foreach (string line2 in lines)
            {
                file4.WriteLine(line2);
            }
        }

    }
}
