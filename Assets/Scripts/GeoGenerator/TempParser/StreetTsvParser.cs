using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PersonGenerator
{
    public class StreetTsvParser : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SplitTsv("Assets/Resources/Geographical/planet-latest_geonames.tsv", 1000000);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void SplitTsv(string path, int splitLines)
        {
            List<string> splitFileLines = new List<string>();
            int lineCounter = 0;
            int fileCounter = 0;
            string line;
            StreamReader fileReader = new StreamReader(path);
            TextWriter fileWriter = null;

            // line = fileReader.ReadLine(); // skip first line

            while ((line = fileReader.ReadLine()) != null)
            {
                if(lineCounter % splitLines == 0)
                {
                    if(fileWriter != null) fileWriter.Close();
                    string splitFilePath = "Assets/Resources/Geographical/streets/" + "streets_" + fileCounter + ".txt";
                    fileWriter = new StreamWriter(splitFilePath);
                    fileCounter++;
                }
                fileWriter.WriteLine(line);
                lineCounter++;
            }
            fileReader.Close();
        }
    }
}
