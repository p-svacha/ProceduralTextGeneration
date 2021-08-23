using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivationFunction
{
    public abstract float GetValue(float f);
    public abstract float GetDerivativeValue(float f);
}
