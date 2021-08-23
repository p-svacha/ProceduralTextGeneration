using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TMTrackNameGen
{
    public class TMXNameGenerator : MonoBehaviour
    {
        public Text OutputPrefab;
        public Transform[] Containers;

        void Start()
        {
            InputDataReader.Init();
            MarkovChainWordGenerator WordGenerator = new MarkovChainWordGenerator();
            for(int i = 0; i < 5; i++)
            {
                int nGramLength = i * 2 + 3;
                List<string> words = new List<string>();
                while(words.Count < 10)
                {
                    string word = WordGenerator.GenerateWord("TrackmaniaMapNames", nGramLength);
                    if (!words.Contains(word) && !WordGenerator.InputWords["TrackmaniaMapNames"].Contains(word)) words.Add(word);
                }
                foreach(string s in words)
                {
                    Text output = GameObject.Instantiate(OutputPrefab, Containers[i]);
                    output.text = s;
                }
            }
        }
    }
}
