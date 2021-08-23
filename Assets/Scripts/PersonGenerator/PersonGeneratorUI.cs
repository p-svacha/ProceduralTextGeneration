using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PersonGenerator
{
    public class PersonGeneratorUI : MonoBehaviour
    {
        [Header("Prefabs")]
        public PersonDisplay PersonDisplayPrefab;

        [Header("Elements")]
        public InputField NumPeopleInput;
        public Dropdown OriginDropdown;
        public Dropdown OriginSpecificDropdown;
        public Dropdown SexDropdown;
        public Button GenerateButton;

        public GameObject OutputContainer;

        private const int DefaultNumNames = 5;

        public List<string> Regions = new List<string>();
        public Dictionary<string, List<string>> FemaleForenames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> MaleForenames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> Surnames = new Dictionary<string, List<string>>();


        #region UI

        // Used in editor by main menu button
        public void GoToMainMenu() 
        {
            SceneManager.LoadScene(sceneName: "MainMenu");
        }

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
            ReadData();

            NumPeopleInput.text = DefaultNumNames + "";

            foreach (string region in Regions) OriginSpecificDropdown.options.Add(new Dropdown.OptionData(region));
            OriginDropdown.onValueChanged.AddListener(OriginDropdown_OnValueChanged);
            foreach (OriginType type in System.Enum.GetValues(typeof(OriginType))) OriginDropdown.options.Add(new Dropdown.OptionData(type.ToString()));
            OriginDropdown.value = 1; OriginDropdown.value = 0;
            foreach (SexType type in System.Enum.GetValues(typeof(SexType))) SexDropdown.options.Add(new Dropdown.OptionData(type.ToString()));
            SexDropdown.value = 1; SexDropdown.value = 0;

            GenerateButton.onClick.AddListener(GenerateButton_OnClick);
            
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

            Regions = Regions.OrderBy(x => x).ToList();

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



        private GenerationSettings GetGenerationSettings()
        {
            List<string> originList = new List<string>();
            OriginType originType = (OriginType)System.Enum.Parse(typeof(OriginType), OriginDropdown.options[OriginDropdown.value].text);
            if (originType == OriginType.Mixed || originType == OriginType.Unspecified) originList = Regions;
            else if (originType == OriginType.Specific) originList = new List<string>() { OriginSpecificDropdown.options[OriginSpecificDropdown.value].text };

            List<string> sexList = new List<string>();
            SexType sexType = (SexType)System.Enum.Parse(typeof(SexType), SexDropdown.options[SexDropdown.value].text);
            if (sexType == SexType.Mixed || sexType == SexType.Unspecified) sexList = new List<string>() { "m", "f" };
            else if (sexType == SexType.Male) sexList = new List<string>() { "m" };
            else if (sexType == SexType.Female) sexList = new List<string>() { "f" };

            int numPeople = int.Parse(NumPeopleInput.text);
            if (numPeople > 10) numPeople = 10;
            return new GenerationSettings(numPeople, originList, sexList, originType , sexType);
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
            // Both origin and sex undefined
            if (settings.OriginFilterType == OriginType.Unspecified && settings.SexFilterType == SexType.Unspecified)
            {
                List<string> forenameCandidates = new List<string>();
                foreach (KeyValuePair<string, List<string>> kvp in FemaleForenames) forenameCandidates.AddRange(kvp.Value);
                foreach (KeyValuePair<string, List<string>> kvp in MaleForenames) forenameCandidates.AddRange(kvp.Value);

                List<string> surnameCandidates = new List<string>();
                foreach (KeyValuePair<string, List<string>> kvp in Surnames) surnameCandidates.AddRange(kvp.Value);

                string forename = forenameCandidates[Random.Range(0, forenameCandidates.Count)];
                string surname = surnameCandidates[Random.Range(0, surnameCandidates.Count)];

                return new Person(forename, surname, "", "");
            }

            // Only origin undefined
            else if (settings.OriginFilterType == OriginType.Unspecified)
            {
                string targetSex = settings.Sex[Random.Range(0, settings.Sex.Count)];

                List<string> forenameCandidates = new List<string>();
                if (targetSex == "f")
                {
                    foreach (KeyValuePair<string, List<string>> kvp in FemaleForenames) forenameCandidates.AddRange(kvp.Value);
                }
                else
                {
                    foreach (KeyValuePair<string, List<string>> kvp in MaleForenames) forenameCandidates.AddRange(kvp.Value);
                }

                List<string> surnameCandidates = new List<string>();
                foreach (KeyValuePair<string, List<string>> kvp in Surnames) surnameCandidates.AddRange(kvp.Value);

                string forename = forenameCandidates[Random.Range(0, forenameCandidates.Count)];
                string surname = surnameCandidates[Random.Range(0, surnameCandidates.Count)];

                return new Person(forename, surname, "", targetSex);
            }

            // Only sex undefined
            else if (settings.SexFilterType == SexType.Unspecified)
            {
                string targetOrigin = settings.Origins[Random.Range(0, settings.Origins.Count)];
                List<string> forenameCandidates = new List<string>();
                forenameCandidates.AddRange(FemaleForenames[targetOrigin]);
                forenameCandidates.AddRange(MaleForenames[targetOrigin]);
                List<string> surnameCandidates = Surnames[targetOrigin];

                string forename = forenameCandidates[Random.Range(0, forenameCandidates.Count)];
                string surname = surnameCandidates[Random.Range(0, surnameCandidates.Count)];

                return new Person(forename, surname, targetOrigin, "");
            }

            // Origin and sex specified
            else
            {
                string targetOrigin = settings.Origins[Random.Range(0, settings.Origins.Count)];
                string targetSex = settings.Sex[Random.Range(0, settings.Sex.Count)];

                List<string> forenameCandidates = new List<string>();
                if (targetSex == "m") forenameCandidates = MaleForenames[targetOrigin];
                else forenameCandidates = FemaleForenames[targetOrigin];
                List<string> surnameCandidates = Surnames[targetOrigin];

                string forename = forenameCandidates[Random.Range(0, forenameCandidates.Count)];
                string surname = surnameCandidates[Random.Range(0, surnameCandidates.Count)];

                return new Person(forename, surname, targetOrigin, targetSex);
            }
        }



        #endregion

        #region UI Actions

        private void GenerateButton_OnClick()
        {
            GenerationSettings settings = GetGenerationSettings();
            List<Person> generatedPeople = GeneratePeople(settings);
            //foreach (Person p in generatedPeople) Debug.Log(p.Attributes.Sex + "/" + p.Attributes.Origin + " " + p.FirstName + " " + p.LastName);
            DisplayGeneratedPeople(generatedPeople);
        }

        private void OriginDropdown_OnValueChanged(int value)
        {
            if (value == 1) OriginSpecificDropdown.gameObject.SetActive(true);
            else OriginSpecificDropdown.gameObject.SetActive(false);
        }

        #endregion
    }
}
