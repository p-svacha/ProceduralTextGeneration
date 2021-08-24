using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        public Text OutputSubtitleText;
        public GameObject OutputContainer;

        private const int DefaultNumNames = 5;

        public List<Country> Countries;
        public Dictionary<string, List<string>> FemaleForenames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> MaleForenames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> Surnames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> Cities = new Dictionary<string, List<string>>();


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

            foreach (Country country in Countries) OriginSpecificDropdown.options.Add(new Dropdown.OptionData(country.Name));
            OriginDropdown.onValueChanged.AddListener(OriginDropdown_OnValueChanged);
            foreach (OriginType type in System.Enum.GetValues(typeof(OriginType))) OriginDropdown.options.Add(new Dropdown.OptionData(type.ToString()));
            OriginDropdown.value = 1; OriginDropdown.value = 0;
            foreach (SexType type in System.Enum.GetValues(typeof(SexType))) SexDropdown.options.Add(new Dropdown.OptionData(type.ToString()));
            SexDropdown.value = 1; SexDropdown.value = 0;

            GenerateButton.onClick.AddListener(GenerateButton_OnClick);
            
        }

        private void ReadData()
        {
            string line;

            Countries = CountryInfoReader.GetCountries();
            Countries = Countries.OrderBy(x => x.Name).ToList();
            List<Country> validCountries = new List<Country>();

            foreach(Country country in Countries)
            {
                string maleForenamesPath = "Assets/Resources/Geographical/forenames/" + country.Name.ToLower().Replace(" ", "-").Replace(",", "") + "_forenames_male.txt";
                string femaleForenamesPath = "Assets/Resources/Geographical/forenames/" + country.Name.ToLower().Replace(" ", "-").Replace(",", "") + "_forenames_female.txt";
                string surnamesPath = "Assets/Resources/Geographical/surnames/" + country.Name.ToLower().Replace(" ", "-").Replace(",", "") + "_surnames.txt";
                string cityPath = "Assets/Resources/Geographical/cities/" + country.ISO + "_cities.txt";

                if (!File.Exists(maleForenamesPath) || !File.Exists(femaleForenamesPath) || !File.Exists(surnamesPath) || !File.Exists(cityPath))
                {
                    Debug.LogWarning("Not all data (male forenames, female forenames, surnames, city names) found for " + country.Name + " (" + country.ISO + ")");
                    continue;
                }

                validCountries.Add(country);

                // male forenames
                MaleForenames.Add(country.ISO, new List<string>());
                System.IO.StreamReader mForenamesFile = new System.IO.StreamReader(maleForenamesPath);
                while ((line = mForenamesFile.ReadLine()) != null) MaleForenames[country.ISO].Add(line);
                mForenamesFile.Close();

                // female forenames
                FemaleForenames.Add(country.ISO, new List<string>());
                System.IO.StreamReader fForenamesFile = new System.IO.StreamReader(femaleForenamesPath);
                while ((line = fForenamesFile.ReadLine()) != null) FemaleForenames[country.ISO].Add(line);
                fForenamesFile.Close();

                // surnames
                Surnames.Add(country.ISO, new List<string>());
                System.IO.StreamReader surnamesFile = new System.IO.StreamReader(surnamesPath);
                while ((line = surnamesFile.ReadLine()) != null) Surnames[country.ISO].Add(line);
                surnamesFile.Close();

                // cities
                Cities.Add(country.ISO, new List<string>());
                System.IO.StreamReader citiesFile = new System.IO.StreamReader(surnamesPath);
                while ((line = citiesFile.ReadLine()) != null) Cities[country.ISO].Add(line);
                citiesFile.Close();
            }

            Countries = validCountries;
        }



        private GenerationSettings GetGenerationSettings()
        {
            List<Country> originList = new List<Country>();
            OriginType originType = (OriginType)System.Enum.Parse(typeof(OriginType), OriginDropdown.options[OriginDropdown.value].text);
            if (originType == OriginType.Mixed || originType == OriginType.Unspecified) originList = Countries;
            else if (originType == OriginType.Specific) originList = new List<Country>() { Countries[OriginSpecificDropdown.value] };

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
            string originText = settings.OriginFilterType == OriginType.Specific ? settings.Origins[0].Name : settings.OriginFilterType.ToString();
            OutputSubtitleText.text = "Amount: " + settings.NumNames + ", Origin: " + originText + ", Sex: " + settings.SexFilterType.ToString();

            List<Person> people = new List<Person>();
            for(int i = 0; i < settings.NumNames; i++)
            {
                Person newPerson = GeneratePerson(settings);
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

                return new Person(forename, surname, null, "", "");
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

                return new Person(forename, surname, null, targetSex, "");
            }

            // Only sex undefined
            else if (settings.SexFilterType == SexType.Unspecified)
            {
                Country targetOrigin = settings.Origins[Random.Range(0, settings.Origins.Count)];
                List<string> forenameCandidates = new List<string>();
                forenameCandidates.AddRange(FemaleForenames[targetOrigin.ISO]);
                forenameCandidates.AddRange(MaleForenames[targetOrigin.ISO]);
                List<string> surnameCandidates = Surnames[targetOrigin.ISO];
                List<string> cityCandidates = Cities[targetOrigin.ISO];

                string forename = forenameCandidates[Random.Range(0, forenameCandidates.Count)];
                string surname = surnameCandidates[Random.Range(0, surnameCandidates.Count)];
                string city = cityCandidates[Random.Range(0, cityCandidates.Count)];

                return new Person(forename, surname, targetOrigin, "", city);
            }

            // Origin and sex specified
            else
            {
                Country targetOrigin = settings.Origins[Random.Range(0, settings.Origins.Count)];
                string targetSex = settings.Sex[Random.Range(0, settings.Sex.Count)];

                List<string> forenameCandidates = new List<string>();
                if (targetSex == "m") forenameCandidates = MaleForenames[targetOrigin.ISO];
                else forenameCandidates = FemaleForenames[targetOrigin.ISO];
                List<string> surnameCandidates = Surnames[targetOrigin.ISO];
                List<string> cityCandidates = Cities[targetOrigin.ISO];

                string forename = forenameCandidates[Random.Range(0, forenameCandidates.Count)];
                string surname = surnameCandidates[Random.Range(0, surnameCandidates.Count)];
                string city = cityCandidates[Random.Range(0, cityCandidates.Count)];

                return new Person(forename, surname, targetOrigin, targetSex, city);
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
