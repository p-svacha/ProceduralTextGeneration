using System.Collections;
using System.Collections.Generic;
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
        {"Gemeinde", " AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz" },
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

    private const char WordStartChar = '˨';
    private const char WordEndChar = '˩';

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

            int[] layers = new int[] { AcceptedChars[category].Length * SegmentLength, AcceptedChars[category].Length * SegmentLength / 2, AcceptedChars[category].Length * SegmentLength / 2, AcceptedChars[category].Length };

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
            float[] input = WordToNeuralNetInput(category, word);
            float[] output = network.FeedForward(input);
            char c = NeuralNetOutputToChar(category, output, skewValue);
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

                float[] networkInput = WordToNeuralNetInput(category, trainWord);
                float[] expectedOutput = CharToNeuralNetExpectedOutput(category, expectedNextChar);

                network.FeedForward(networkInput);
                network.BackPropagation(expectedOutput);

                if (TrainingIterations.ContainsKey(category)) TrainingIterations[category]++;
                else TrainingIterations.Add(category, 1);

            }
        }
    }

    private float[] WordToNeuralNetInput(string category, string word)
    {
        float[] input = new float[AcceptedChars[category].Length * SegmentLength];

        for(int i = 0; i < SegmentLength; i++)
        {
            int startIndex = i * AcceptedChars[category].Length;
            if(i >= word.Length) // word too short, fill with 0's
            {
                for(int j = 0; j < AcceptedChars[category].Length; j++)
                {
                    input[startIndex + j] = 0;
                }
            }
            else
            {
                char c = word[word.Length - 1 - i]; // fill char node with 1, rest 0
                int charIndex = AcceptedChars[category].IndexOf(c);
                for (int j = 0; j < AcceptedChars[category].Length; j++)
                {
                    if(j == charIndex) input[startIndex + j] = 1;
                    else input[startIndex + j] = 0;
                }
            }
        }

        return input;
    }

    private float[] CharToNeuralNetExpectedOutput(string category, char c)
    {
        float[] expectedOutput = new float[AcceptedChars[category].Length];
        int charIndex = AcceptedChars[category].IndexOf(c);
        //if (!AcceptedChars.Contains(c)) throw new System.Exception("Char " + c + " not found in accepted chars.");

        for (int i = 0; i < AcceptedChars[category].Length; i++)
        {
            if (i == charIndex) expectedOutput[i] = 1;
            else expectedOutput[i] = 0;
        }

        return expectedOutput;
    }

    private char NeuralNetOutputToChar(string category, float[] output, float skewValue)
    {
        Dictionary<char, float> charProbabilites = new Dictionary<char, float>();
        for (int i = 0; i < AcceptedChars[category].Length; i++)
        {
            float baseValue = output[i];
            float skewedValue = output[i] * output[i];
            float realValue = baseValue + ((skewedValue - baseValue) * skewValue);
            charProbabilites.Add(AcceptedChars[category][i], realValue); // skew randomness in favor of probable outputs
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
}
