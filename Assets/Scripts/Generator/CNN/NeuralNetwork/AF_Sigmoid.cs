using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AF_Sigmoid : ActivationFunction
{
    public override float GetDerivativeValue(float f)
    {
        return GetValue(f) * (1 - GetValue(f));
    }

    public override float GetValue(float f)
    {
        return 1.0f / (1.0f + (float)Math.Exp(-f));
    }
}
