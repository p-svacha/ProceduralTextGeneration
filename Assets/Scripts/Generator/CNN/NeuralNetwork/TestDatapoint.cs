using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TestDatapoint
{
    public string word { get; set; }
    public string language { get; set; }
    public float[] input { get; set; }
    public float[] output { get; set; }

    public TestDatapoint(string word, string language, float[] input, float[] output)
    {
        this.word = word;
        this.language = language;
        this.input = input;
        this.output = output;
    }
}

