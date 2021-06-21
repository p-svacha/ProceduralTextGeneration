using GoogleSheetsToUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PersonGenerator
{
    public class PersonGeneratorUI : MonoBehaviour
    {
        // UI
        public PersonDisplay PersonDisplayPrefab;
        public GameObject OutputContainer;
        public Button GenerateButton;

        // Data Loading - not used atm
        private int NumRows = 1000;
        [HideInInspector]
        public string associatedSheet = "1ZkDcDbQ3p1x-xhNPGtv2PU1_BQIErTawoS_vgFCoZ5Q";
        [HideInInspector]
        public string associatedWorksheet = "Names";
        GstuSpreadSheet Spreadsheet;

        private const int DefaultNumNames = 5;

        public List<string> Regions = new List<string>();
        public Dictionary<string, List<string>> FemaleForenames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> MaleForenames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> Surnames = new Dictionary<string, List<string>>();


        #region UI

        private void DisplayGeneratedPeople(List<Person> people)
        {
            foreach (Transform child in OutputContainer.transform) GameObject.Destroy(child.gameObject);

            foreach(Person p in people)
            {
                PersonDisplay display = Instantiate(PersonDisplayPrefab, OutputContainer.transform);
                display.Init(p);
            }
        }

        #endregion

        #region Generation

        // Start is called before the first frame update
        private void Start()
        {
            GenerateButton.onClick.AddListener(GenerateButton_OnClick);
            //LoadAllData();
            ReadData();
        }

        /*
        private void LoadAllData()
        {
            SpreadsheetManager.ReadPublicSpreadsheet(new GSTU_Search(associatedSheet, associatedWorksheet, "A1", "D" + NumRows), OnDataLoaded);
            Debug.Log("Data loaded");
        }
        private void OnDataLoaded(GstuSpreadSheet spreadsheet)
        {
            Spreadsheet = spreadsheet;
            for(int i = 2; i < NumRows; i++)
            {
                //Debug.Log("Reading row " + i);
                List<GSTU_Cell> row;
                try
                {
                    row = Spreadsheet.rows[i];
                } catch(System.Exception e)
                {
                    break;
                }

                if (row[0].value != null && row[0].value != "")
                {
                    Person entry = new Person(row[0].value, row[1].value, row[2].value, row[3].value);
                    FullData.Add(entry);
                }
            }
        }
        */

        private void ReadData()
        {
            string line;

            System.IO.StreamReader regionsFile = new System.IO.StreamReader("Assets/Resources/Geographical/regionlist.txt");
            while ((line = regionsFile.ReadLine()) != null) Regions.Add(line.ToLower().Replace(" ", "-").Replace(",", ""));
            regionsFile.Close();

            foreach(string region in Regions)
            {
                

                // male forenames
                MaleForenames.Add(region, new List<string>());
                System.IO.StreamReader mForenamesFile = new System.IO.StreamReader("Assets/Resources/Geographical/forenames/" + region + "_forenames_male.txt");
                while ((line = mForenamesFile.ReadLine()) != null) MaleForenames[region].Add(line);

                // female forenames
                FemaleForenames.Add(region, new List<string>());
                System.IO.StreamReader fForenamesFile = new System.IO.StreamReader("Assets/Resources/Geographical/forenames/" + region + "_forenames_female.txt");
                while ((line = fForenamesFile.ReadLine()) != null) FemaleForenames[region].Add(line);

                // surnames
                Surnames.Add(region, new List<string>());
                System.IO.StreamReader surnamesFile = new System.IO.StreamReader("Assets/Resources/Geographical/surnames/" + region + "_surnames.txt");
                while ((line = surnamesFile.ReadLine()) != null) Surnames[region].Add(line);
            }
        }

        private void GenerateButton_OnClick()
        {
            GenerationSettings settings = GetGenerationSettings();
            List<Person> generatedPeople = GeneratePeople(settings);
            //foreach (Person p in generatedPeople) Debug.Log(p.Attributes.Sex + "/" + p.Attributes.Origin + " " + p.FirstName + " " + p.LastName);
            DisplayGeneratedPeople(generatedPeople);
        }

        private GenerationSettings GetGenerationSettings()
        {
            return new GenerationSettings(DefaultNumNames, Regions, new List<string>() { "m", "f" }, FilterType.Specific, FilterType.Specific);
        }


        private List<Person> GeneratePeople(GenerationSettings settings)
        {
            List<Person> people = new List<Person>();
            for(int i = 0; i < settings.NumNames; i++)
            {
                Person newPerson = GeneratePerson(settings);
                Debug.Log(newPerson.Origin + "/" + newPerson.Sex + ": " + newPerson.FirstName + " " + newPerson.LastName);
                people.Add(newPerson);
            }
            return people;
        }

        private Person GeneratePerson(GenerationSettings settings)
        {
            string targetOrigin = settings.Origins[Random.Range(0, settings.Origins.Count)];
            string targetSex = settings.Sex[Random.Range(0, settings.Sex.Count)];

            string firstName = "";
            if (targetSex == "m") firstName = MaleForenames[targetOrigin][Random.Range(0, MaleForenames[targetOrigin].Count)]; 
            else firstName = FemaleForenames[targetOrigin][Random.Range(0, FemaleForenames[targetOrigin].Count)];
            string lastName = Surnames[targetOrigin][Random.Range(0, Surnames[targetOrigin].Count)];
            return new Person(firstName, lastName, targetOrigin, targetSex);
        }



        #endregion
    }
}
