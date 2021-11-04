using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileParser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<string> entries = new List<string>();

        // Read raw file
        string line;
        System.IO.StreamReader rawFile = new System.IO.StreamReader("Assets/Resources/RawInputData/cities15000.txt");
        line = rawFile.ReadLine(); // skip first line
        while ((line = rawFile.ReadLine()) != null)
        {
            string sep = "\t";
            string[] splits = line.Split(sep.ToCharArray());
            string language = splits[2];
            entries.Add(language);
        }
        rawFile.Close();

        WriteFile("Assets/Resources/InputData/Cities_Parsed.txt", entries);
    }

    public static void WriteFile(string path, List<string> lines) // path = "Assets/Resources/InputData/Cities_Parsed.txt"
    {
        // Write dataset
        TextWriter tw = new StreamWriter(path);
        foreach (string entry in lines) tw.WriteLine(entry);
        tw.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
