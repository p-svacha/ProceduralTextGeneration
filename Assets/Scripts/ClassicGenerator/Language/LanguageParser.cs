using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LanguageParser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<string> languages = new List<string>();

        // Read raw file
        string line;
        System.IO.StreamReader rawFile = new System.IO.StreamReader("Assets/Resources/RawInputData/LanguagesRaw.txt");
        line = rawFile.ReadLine(); // skip first line
        while ((line = rawFile.ReadLine()) != null)
        {
            string sep = "\t";
            string[] splits = line.Split(sep.ToCharArray());
            string language = splits[3];
            languages.Add(language);
        }
        rawFile.Close();

        // Write dataset
        string path = "Assets/Resources/InputData/Languages_Parsed.txt";
        TextWriter tw = new StreamWriter(path);
        foreach (string language in languages) tw.WriteLine(language);
        tw.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
