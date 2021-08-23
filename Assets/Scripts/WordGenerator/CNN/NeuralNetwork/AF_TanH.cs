using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AF_TanH : ActivationFunction
{
    public override float GetDerivativeValue(float f)
    {
        return 1 - (f * f);
    }

    public override float GetValue(float f)
    {
        return (float)Math.Tanh(f);
    }
}
