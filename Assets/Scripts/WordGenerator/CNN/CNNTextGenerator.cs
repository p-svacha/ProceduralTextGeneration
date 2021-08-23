using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CNNTextGenerator
{
    /// <summary>
    /// Key is the word type  (i.e. "Planet" or "Country")
    /// Value is a list with all input words
    /// </summary>
    public Dictionary<string, List<string>> InputWords;

    /// <summary>
    /// The neural network for each word category. Each network takes the last n letters as an input and the probability for each letter as next letter as output. 
    /// </summary>
    public Dictionary<string, NeuralNetwork> Networks;

    public Dictionary<string, int> TrainingIterations;

    /// <summary>
    /// Only chars in this string are seen as valid inputs
    /// </summary>
    private Dictionary<string, string> AcceptedChars = new Dictionary<string, string>()
    {
        {"Planet", "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz" },
        {"Country", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz-'" },
        {"Province", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz-'" },
        {"Gemeinde", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZzÄäÖöÜü-().'èéâ" },
        {"Mineral", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz" },
        {"TrackmaniaMapNames", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz1234567890-#" },
        {"FaberSongs", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz" },
        {"FaberSongText", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz" },
        {"Usernames", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz1234567890-_" },
        {"Test", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz" },
    };

    /// <summary>
    /// Defines how many letters back are given the network as an input to predict the next letter
    /// </summary>
    private const int SegmentLength = 8;

    private const char WordStartChar = '>';
    private const char WordEndChar = '<';

    public string LoadedAcceptedChars;
    public NeuralNetwork LoadedNetwork;

    public CNNTextGenerator()
    {
        InputWords = new Dictionary<string, List<string>>();
        Networks = new Dictionary<string, NeuralNetwork>();
        TrainingIterations = new Dictionary<string, int>();

        foreach (string category in InputDataReader.WordCategories.Keys)
        {
            AcceptedChars[category] += WordStartChar;
            AcceptedChars[category] += WordEndChar;
            List<string> inputWords = InputDataReader.GetInputWords(category, AcceptedChars[category]);
            InputWords.Add(category, inputWords);

            int[] layers = new int[] { AcceptedChars[category].Length * SegmentLength, AcceptedChars[category].Length * SegmentLength / 2, AcceptedChars[category].Length * SegmentLength / 4, AcceptedChars[category].Length };

            NeuralNetwork network = new NeuralNetwork(layers, ActivationFunctionType.Sigmoid);
            Networks.Add(category, network);
        }
    }

    public string GenerateWord(string category, float skewValue, string start)
    {
        NeuralNetwork network = Networks[category];
        string word = WordStartChar + start + "";

        while(word[word.Length - 1] != WordEndChar && word.Length < 50)
        {
            float[] input = WordToNeuralNetInput(AcceptedChars[category], word);
            float[] output = network.FeedForward(input);
            char c = NeuralNetOutputToChar(AcceptedChars[category], output, skewValue);
            word += c;
        }

        return word.Substring(1, word.Length - 2); // Remove word start and end char
    }

    public string GenerateLoadedNetworkWord(float skewValue, string start)
    {
        string word = WordStartChar + start + "";

        while (word[word.Length - 1] != WordEndChar && word.Length < 50)
        {
            float[] input = WordToNeuralNetInput(LoadedAcceptedChars, word);
            float[] output = LoadedNetwork.FeedForward(input);
            char c = NeuralNetOutputToChar(LoadedAcceptedChars, output, skewValue);
            word += c;
        }

        return word.Substring(1, word.Length - 2); // Remove word start and end char
    }

    public void TrainOnce(string category)
    {
        NeuralNetwork network = Networks[category];

        string randomWord = InputWords[category][Random.Range(0, InputWords[category].Count)];
        randomWord = WordStartChar + randomWord + WordEndChar;
        // Train for each letter the next expected letter
        for(int i = 1; i < randomWord.Length /*+ SegmentLength - 1*/; i++)
        {
            string trainWord = "???";
            char expectedNextChar = '?';

            if (i >= randomWord.Length)
            {
                int startIndex = i - SegmentLength;
                int length = randomWord.Length - startIndex - 1;
                if (startIndex > 0)
                {
                    trainWord = randomWord.Substring(startIndex, length);
                    expectedNextChar = WordEndChar;
                }
            }
            else
            {
                trainWord = randomWord.Substring(Mathf.Max(0, i - SegmentLength), Mathf.Min(SegmentLength, i));
                expectedNextChar = randomWord[i];
            }

            if (trainWord != "???" && expectedNextChar != '?')
            {
                //Debug.Log(randomWord + " || i = " + i + ": trainWord = " + trainWord + ", expectedNextLetter = " + expectedNextChar);

                float[] networkInput = WordToNeuralNetInput(AcceptedChars[category], trainWord);
                float[] expectedOutput = CharToNeuralNetExpectedOutput(AcceptedChars[category], expectedNextChar);

                network.FeedForward(networkInput);
                network.BackPropagation(expectedOutput);

                if (TrainingIterations.ContainsKey(category)) TrainingIterations[category]++;
                else TrainingIterations.Add(category, 1);

            }
        }
    }

    private float[] WordToNeuralNetInput(string acceptedChars, string word)
    {
        float[] input = new float[acceptedChars.Length * SegmentLength];

        for(int i = 0; i < SegmentLength; i++)
        {
            int startIndex = i * acceptedChars.Length;
            if(i >= word.Length) // word too short, fill with 0's
            {
                for(int j = 0; j < acceptedChars.Length; j++)
                {
                    input[startIndex + j] = 0;
                }
            }
            else
            {
                char c = word[word.Length - 1 - i]; // fill char node with 1, rest 0
                int charIndex = acceptedChars.IndexOf(c);
                for (int j = 0; j < acceptedChars.Length; j++)
                {
                    if(j == charIndex) input[startIndex + j] = 1;
                    else input[startIndex + j] = 0;
                }
            }
        }

        return input;
    }

    private float[] CharToNeuralNetExpectedOutput(string acceptedChars, char c)
    {
        float[] expectedOutput = new float[acceptedChars.Length];
        int charIndex = acceptedChars.IndexOf(c);
        //if (!AcceptedChars.Contains(c)) throw new System.Exception("Char " + c + " not found in accepted chars.");

        for (int i = 0; i < acceptedChars.Length; i++)
        {
            if (i == charIndex) expectedOutput[i] = 1;
            else expectedOutput[i] = 0;
        }

        return expectedOutput;
    }

    private char NeuralNetOutputToChar(string acceptedChars, float[] output, float skewValue)
    {
        Dictionary<char, float> charProbabilites = new Dictionary<char, float>();
        for (int i = 0; i < acceptedChars.Length; i++)
        {
            float baseValue = output[i];
            float skewedValue = output[i] * output[i];
            float realValue = baseValue + ((skewedValue - baseValue) * skewValue);
            charProbabilites.Add(acceptedChars[i], realValue); // skew randomness in favor of probable outputs
        }

        float probSum = charProbabilites.Values.Sum();
        float rng = Random.Range(0f, probSum);
        float tmpProb = 0;
        foreach(KeyValuePair<char, float> kvp in charProbabilites)
        {
            tmpProb += kvp.Value;
            if (rng < tmpProb) return kvp.Key;
        }
        return '?';
    }

    #region IO

    /// <summary>
    /// Saves the text generator CNN of a given category in a text file at the current path
    /// </summary>
    public string SaveCnn(string category, string path)
    {
        NeuralNetwork network = Networks[category];

        if (!File.Exists(path))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                // 1. Line: accepted chars
                sw.Write(AcceptedChars[category]);

                // 2. Line: network layer sizes
                sw.Write("\n");
                for (int i = 0; i < network.layer.Length; i++)
                {
                    sw.Write(network.layer[i]);
                    if (i < network.layer.Length - 1) sw.Write(",");
                }

                // Every line from here contains the weight of the n'th layer
                foreach (NeuralNetwork.Layer layer in network.layers)
                {
                    sw.Write("\n");
                    for (int i = 0; i < layer.weights.GetLength(0); i++)
                    {
                        for (int j = 0; j < layer.weights.GetLength(1); j++)
                        {
                            sw.Write(layer.weights[i, j]);
                            if (i < layer.weights.GetLength(0) - 1 || j < layer.weights.GetLength(1) - 1) sw.Write(",");
                        }
                    }
                }
            }

            return "Network for " + category + " successfully saved in " + path;
        }
        else return "Did not save CNN because one already exists for this category. Delete it in Assets/Resources/SavedNetworks if you want to save.";
    }

    public string LoadCnn(string category)
    {
        string path = "Assets/Resources/SavedNetworks/" + category + ".txt";
        if (File.Exists(path))
        {
            StreamReader file = new StreamReader(path);

            string line;
            List<string> lines = new List<string>();
            while((line = file.ReadLine()) != null)
            {
                lines.Add(line);
            }

            LoadedAcceptedChars = lines[0];

            List<int> layers = new List<int>();
            string[] layerSizes = lines[1].Split(',');
            foreach (string s in layerSizes) layers.Add(int.Parse(s));

            List<float[,]> weights = new List<float[,]>();
            for(int i = 2; i < lines.Count; i++)
            {
                string[] layerWeightsS = lines[i].Split(',');
                float[,] layerWeightsF = new float[layers[i - 1], layers[i - 2]];
                for (int j = 0; j < layerWeightsS.Length; j++)
                {
                    float value = float.Parse(layerWeightsS[j]);
                    int x = j / layers[i - 2];
                    int y = j % layers[i - 2];
                    layerWeightsF[x, y] = value;
                }
                weights.Add(layerWeightsF);
            }

            LoadedNetwork = new NeuralNetwork(layers, weights, ActivationFunctionType.Sigmoid);

            file.Close();

            return "Network for " + category + " succesfully loaded.";
        }
        else return "No saved network found for category " + category;
    }

    #endregion
}
