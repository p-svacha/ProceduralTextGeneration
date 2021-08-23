using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class crawls throught the TMX API and adds tracks to the input data list. It goes from oldest tracks to newest tracks and saves the current page of the api as the first line in the input file.
/// </summary>
public class TMXCrawler
{
    // How to Use:
    // 1. Go to https://trackmania.exchange/mapsearch2/search?api=on&limit=100&priord=3&page={???}, where ??? is the first page number that hasn't been added yet
    // Added pages: 128
    // 2. Copy paste the content into Assets/Resources/RawInputData/tmxJson.txt
    // 3. Run the sample scene and click "Read tmx Json"
    private static string InputFilePath = Application.dataPath + "/Resources/InputData/TrackmaniaMapNames.txt";
    private static string RawInputFilePath = Application.dataPath + "/Resources/RawInputData/tmxJson.txt";

    public static void ReadJson()
    {
        // Load new names
        string json = File.ReadAllText(RawInputFilePath);

        TMXMapList maps = JsonUtility.FromJson<TMXMapList>(json);
        List<string> newNames = new List<string>();
        foreach (TMXMap map in maps.results) newNames.Add(map.Name.TrimStart(' ').TrimEnd(' '));

        // Read existing names
        List<string> existingNames = new List<string>();
        string line;

        StreamReader inputFile = new StreamReader(InputFilePath, System.Text.Encoding.UTF8);
        while ((line = inputFile.ReadLine()) != null)
        {
            existingNames.Add(line);
        }
        inputFile.Close();

        // Add new names
        List<string> namesToAdd = newNames.Where(x => !existingNames.Contains(x)).ToList();
        foreach (string newName in namesToAdd)
        {
            File.AppendAllText(InputFilePath, newName + Environment.NewLine);
        }
        Debug.Log("Added " + namesToAdd.Count + " names"); 
    }

}
