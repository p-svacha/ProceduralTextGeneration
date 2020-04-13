using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InputDataClearer : MonoBehaviour
{
    void Start()
    {
        //TakeFirstTab("Assets/Resources/InputData/Gemeinde.txt");
        //ReplaceDoubleS("Assets/Resources/InputData/FaberSongText.txt");
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
}
