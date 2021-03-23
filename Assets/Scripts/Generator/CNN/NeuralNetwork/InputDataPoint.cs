using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDataPoint
{
    public float[] Data { get; private set; }

    public InputDataPoint(float[] data)
    {
        Data = data;
    }

    public InputDataPoint(Texture2D tex)
    {
        Data = new float[tex.width * tex.height * 3];

        int counter = 0;
        for(int y = 0; y < tex.height; y++)
        {
            for(int x = 0; x < tex.width; x++)
            {
                Color c = tex.GetPixel(x, y);
                Data[counter] = c.r;
                Data[counter + 1] = c.g;
                Data[counter + 2] = c.b;
                counter += 3;
            }
        }
    }
}
