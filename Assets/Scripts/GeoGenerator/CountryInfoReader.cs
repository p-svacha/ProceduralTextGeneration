using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CountryInfoReader
{
    public static List<Country> GetCountries()
    {
        List<Country> countries = new List<Country>();

        // Read country info
        string line;
        System.IO.StreamReader countryFile = new System.IO.StreamReader("Assets/Resources/Geographical/countryInfo.txt");
        line = countryFile.ReadLine(); // Skip first line
        while ((line = countryFile.ReadLine()) != null)
        {
            string sep = "\t";
            string[] splits = line.Split(sep.ToCharArray());
            countries.Add(new Country()
            {
                ISO = splits[0],
                Name = splits[4]
            });
        }
        countryFile.Close();

        return countries;
    }
}
