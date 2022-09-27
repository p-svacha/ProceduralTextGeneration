using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace PersonGenerator
{
    public class UI_SaveToTxtWindow : MonoBehaviour
    {
        private const string SAVE_PATH = "Assets/Resources/Geographical/output/output.txt";

        public Text Title;
        public Text Subtitle;
        public Toggle FirstnameToggle;
        public Toggle LastnameToggle;
        public Toggle StreetToggle;
        public Toggle CityToggle;
        public Toggle CountryToggle;
        public Button CancelButton;
        public Button SaveButton;

        private List<Person> People = new List<Person>();

        private void Start()
        {
            CancelButton.onClick.AddListener(CancelButton_OnClick);
            SaveButton.onClick.AddListener(SaveButton_OnClick);
        }

        public void Init(List<Person> people)
        {
            People = people;
            gameObject.SetActive(true);
            Title.text = "Saving " + People.Count + " people to";
            Subtitle.text = SAVE_PATH;
        }

        private void CancelButton_OnClick()
        {
            gameObject.SetActive(false);
        }

        private void SaveButton_OnClick()
        {
            List<string> outputLines = new List<string>();
            foreach(Person p in People)
            {
                string line = "";
                if (FirstnameToggle.isOn) line += p.FirstName + ";";
                if (LastnameToggle.isOn) line += p.LastName + ";";
                if (StreetToggle.isOn) line += p.Street + ";";
                if (CityToggle.isOn) line += p.City + ";";
                if (CountryToggle.isOn) line += p.Origin.Name + ";";
                line = line.TrimEnd(';');
                line = line.Replace("\"", "");
                outputLines.Add(line);
            }

            using (StringWriter writer = new StringWriter())
            {
                foreach (string s in outputLines) writer.WriteLine(s);
                File.WriteAllText(SAVE_PATH, writer.ToString());
            }

            gameObject.SetActive(false);
        }
    }
}
