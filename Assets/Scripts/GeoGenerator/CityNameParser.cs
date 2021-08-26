using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CityNameParser : MonoBehaviour
{
    private Dictionary<string, List<string>> CityNames = new Dictionary<string, List<string>>(); // Key = country ISO, Value = city list

    private void Start()
    {
        ReadCountryInfo();
        ReadCityData();
        //foreach (KeyValuePair<string, string> kvp in Countries) Debug.Log(CityNames[kvp.Key].Count + " names found for " + kvp.Value);
        SaveCityDataByCountry();
    }

    private void ReadCountryInfo()
    {
        // Read country info
        List<Country> countries = CountryInfoReader.GetCountries();
        foreach(Country country in countries) CityNames.Add(country.ISO, new List<string>());
    }

    private void ReadCityData()
    {
        string line;
        System.IO.StreamReader countryFile = new System.IO.StreamReader("Assets/Resources/Geographical/cityData.txt");
        line = countryFile.ReadLine(); // skip first line
        while ((line = countryFile.ReadLine()) != null)
        {
            string sep = "\t";
            string[] splits = line.Split(sep.ToCharArray());
            string countryIso = splits[1];
            string cityName = splits[2];
            CityNames[countryIso].Add(cityName);
        }
        countryFile.Close();
    }

    private void SaveCityDataByCountry()
    {
        foreach (KeyValuePair<string, List<string>> kvp in CityNames)
        {
            string path = "Assets/Resources/Geographical/cities/" + kvp.Key + "_cities.txt";
            TextWriter tw = new StreamWriter(path);
            foreach (string s in kvp.Value) tw.WriteLine(s);
            tw.Close();
        }
    }
}
