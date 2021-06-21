using UnityEngine;
using System.Collections;
using GoogleSheetsToUnity;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// example script to show realtime updates of multiple items
/// </summary>
public class AnimalManager : MonoBehaviour
{
    public enum SheetStatus
    {
        PUBLIC,
        PRIVATE
    }
    public SheetStatus sheetStatus;

    [HideInInspector]
    public string associatedSheet = "1GVXeyWCz0tCjyqE1GWJoayj92rx4a_hu4nQbYmW_PkE";
    [HideInInspector]
    public string associatedWorksheet = "Stats";

    public List<AnimalObject> animalObjects = new List<AnimalObject>();
    public AnimalContainer container;
    

    public bool updateOnPlay;

    void Awake()
    {
        if(updateOnPlay)
        {
           UpdateStats();
        }
    }

    void UpdateStats()
    {
        if (sheetStatus == SheetStatus.PRIVATE)
        {
            SpreadsheetManager.Read(new GSTU_Search(associatedSheet, associatedWorksheet), UpdateAllAnimals);
        }
        else if(sheetStatus == SheetStatus.PUBLIC)
        {
            SpreadsheetManager.ReadPublicSpreadsheet(new GSTU_Search(associatedSheet, associatedWorksheet), UpdateAllAnimals);
        }
    }

    void UpdateAllAnimals(GstuSpreadSheet ss)
    {
        foreach (Animal animal in container.allAnimals)
        {
            animal.UpdateStats(ss);
        }

        foreach(AnimalObject animalObject in animalObjects)
        {
            animalObject.BuildAnimalInfo();
        }
    }

}
