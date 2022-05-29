using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PersonGenerator
{
    public class OsmParser : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            FindStreets();
        }

        private void FindStreets()
        {
            Dictionary<string, List<string>> streetsByCountry = new Dictionary<string, List<string>>();

            // Read
            for (int i = 0; i < 24; i++)
            {
                string line;
                string path = "Assets/Resources/Geographical/osm/" + "streets_" + i + ".txt";
                StreamReader fileReader = new StreamReader(path);
                while ((line = fileReader.ReadLine()) != null)
                {
                    string sep = "\t";
                    string[] splits = line.Split(sep.ToCharArray());
                    string name = splits[0];
                    //string altNames = splits[1];
                    //string osmType = splits[2];
                    //string osmId = splits[3];
                    string osmClass = splits[4];
                    string countryCode = splits.Length > 15 ? splits[15] : "";

                    if(osmClass == "highway" && countryCode != "")
                    {
                        if (streetsByCountry.ContainsKey(countryCode)) streetsByCountry[countryCode].Add(name);
                        else streetsByCountry.Add(countryCode, new List<string>() { name });
                    }
                }
                fileReader.Close();
            }

            // Write
            foreach (KeyValuePair<string, List<string>> kvp in streetsByCountry)
            {
                string path = "Assets/Resources/Geographical/streets/" + kvp.Key + "_streets.txt";
                TextWriter tw = new StreamWriter(path);
                foreach (string s in kvp.Value) tw.WriteLine(s);
                tw.Close();
            }
        }
    }
}
