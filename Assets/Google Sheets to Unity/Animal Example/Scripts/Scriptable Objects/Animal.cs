using UnityEngine;
using System.Collections;
using GoogleSheetsToUnity;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using GoogleSheetsToUnity.ThirdPary;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Animal : ScriptableObject
{
    [HideInInspector]
    public string associatedSheet = "1GVXeyWCz0tCjyqE1GWJoayj92rx4a_hu4nQbYmW_PkE";
    [HideInInspector]
    public string associatedWorksheet = "Stats";

    public int health;
    public int attack;
    public int defence;
    public List<string> items = new List<string>();

    internal void UpdateStats(List<GSTU_Cell> list)
    {
        items.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            switch (list[i].columnId)
            {
                case "Health":
                    {
                        health = int.Parse(list[i].value);
                        break;
                    }
                case "Attack":
                    {
                        attack = int.Parse(list[i].value);
                        break;
                    }
                case "Defence":
                    {
                        defence = int.Parse(list[i].value);
                        break;
                    }
                case "Items":
                    {
                        items.Add(list[i].value.ToString());
                        break;
                    }
            }
        }
    }

    internal void UpdateStats(GstuSpreadSheet ss)
    {
        items.Clear();
        health = int.Parse(ss[name, "Health"].value);
        attack = int.Parse(ss[name, "Attack"].value);
        defence = int.Parse(ss[name, "Defence"].value);
        items.Add(ss[name, "Items"].value.ToString());
    }

    internal void UpdateStats(GstuSpreadSheet ss, bool mergedCells)
    {
        items.Clear();
        health = int.Parse(ss[name, "Health"].value);
        attack = int.Parse(ss[name, "Attack"].value);
        defence = int.Parse(ss[name, "Defence"].value);

        //I know that my items column may contain multiple values so we run a for loop to ensure they are all added
        foreach (var value in ss[name, "Items", true])
        {
            items.Add(value.value.ToString());
        }
    }
}


//Custom editior to provide additional features
#if UNITY_EDITOR
[CustomEditor(typeof(Animal))]
public class AnimalEditor : Editor
{
    Animal animal;

    void OnEnable()
    {
        animal = (Animal)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Read Data Examples");

        if (GUILayout.Button("Pull Data Method One"))
        {
            UpdateStats(UpdateMethodOne);
        }

        if (GUILayout.Button("Pull Data Method Two"))
        {
            UpdateStats(UpdateMethodTwo);
        }

        if (GUILayout.Button("Pull Data With merged Cells"))
        {
            UpdateStats(UpdateMethodMergedCells, true);
        }

        GUILayout.Label("Write Data Examples");
        GUILayout.Label("Update the existing data");
        if (GUILayout.Button("Update sheet information"))
        {
            UpdateAnimalInformationOnSheet();
        }

        if (GUILayout.Button("Update Only Health"))
        {
            UpdateAnimalHealth();
        }

        GUILayout.Label("Add New Data");
        if (GUILayout.Button("Add Via Append"))
        {
            AppendToSheet();
        }

        if (GUILayout.Button("Add Via Write"))
        {
            WriteToSheet();
        }
    }

    void UpdateStats(UnityAction<GstuSpreadSheet> callback, bool mergedCells = false)
    {
        SpreadsheetManager.Read(new GSTU_Search(animal.associatedSheet, animal.associatedWorksheet), callback, mergedCells);
    }

    void UpdateMethodOne(GstuSpreadSheet ss)
    {
        animal.UpdateStats(ss.rows[animal.name]);

        EditorUtility.SetDirty(target);
    }

    void UpdateMethodTwo(GstuSpreadSheet ss)
    {
        animal.UpdateStats(ss);

        EditorUtility.SetDirty(target);
    }

    void UpdateMethodMergedCells(GstuSpreadSheet ss)
    {
        animal.UpdateStats(ss, true);

        EditorUtility.SetDirty(target);
    }

    /// <summary>
    /// Appends the new animal to the spreadsheet online
    /// </summary>
    void AppendToSheet()
    {
        List<string> list = new List<string>() {
       animal.name,
        animal.health.ToString(),
       animal.attack.ToString(),
        animal.defence.ToString()
        };

        SpreadsheetManager.Append(new GSTU_Search(animal.associatedSheet, animal.associatedWorksheet), new ValueRange(list), null);
    }

    /// <summary>
    /// Adds the new animal to the spreadsheet online at the location defined as start cell, if no start cell defined will write from A1
    /// </summary>
    void WriteToSheet()
    {
        List<string> list = new List<string>();

        list.Add(animal.name);
        list.Add(animal.health.ToString());
        list.Add(animal.attack.ToString());
        list.Add(animal.defence.ToString());

        SpreadsheetManager.Write(new GSTU_Search(animal.associatedSheet, animal.associatedWorksheet, "G10"), new ValueRange(list), null);
    }

    /// <summary>
    /// Finds and updates the rows data based on an entry row data, in this example i am using the name as the unique id to find the starting cell for the row
    /// If the spreadsheet is cashed then no need to do the read and can just pass into the update
    /// </summary>
    void UpdateAnimalInformationOnSheet()
    {
        SpreadsheetManager.Read(new GSTU_Search(animal.associatedSheet, animal.associatedWorksheet), UpdateAnimalInformation);
    }
    private void UpdateAnimalInformation(GstuSpreadSheet ss)
    {
        BatchRequestBody updateRequest = new BatchRequestBody();
        updateRequest.Add(ss[animal.name, "Health"].AddCellToBatchUpdate(animal.associatedSheet, animal.associatedWorksheet, animal.health.ToString()));
        updateRequest.Add(ss[animal.name, "Defence"].AddCellToBatchUpdate(animal.associatedSheet, animal.associatedWorksheet, animal.health.ToString()));
        updateRequest.Add(ss[animal.name, "Attack"].AddCellToBatchUpdate(animal.associatedSheet, animal.associatedWorksheet, animal.health.ToString()));
        updateRequest.Send(animal.associatedSheet, animal.associatedWorksheet, null);
        ///Although this does work if requires that the list is set up in the correct order to write the data correctly, the above solution provides a more robust solution as the cells
        /// already know what to update.
        /*string cellRef = ss[animal.name, "Name"].CellRef(); //get the cell ref where the animal start, i know this because name is my first field for the data
        List<string> list = new List<string>();
        list.Add(animal.name);
        list.Add(animal.health.ToString());
        list.Add(animal.attack.ToString());
        list.Add(animal.defence.ToString());
        SpreadsheetManager.Write(new GSTU_Search(animal.associatedSheet, animal.associatedWorksheet, cellRef), list, null);*/
    }

    /// <summary>
    /// Finds the cell we need to update and then updates the information
    /// If the spreadsheet is cashed then no need to do the read and can just pass into the update
    /// </summary>
    private void UpdateAnimalHealth()
    {
        SpreadsheetManager.Read(new GSTU_Search(animal.associatedSheet, animal.associatedWorksheet), UpdateAnimalHealth);
    }
    private void UpdateAnimalHealth(GstuSpreadSheet ss)
    {
        ss[animal.name, "Name"].UpdateCellValue(animal.associatedSheet, animal.associatedWorksheet, animal.health.ToString()); 
    }
}
#endif
