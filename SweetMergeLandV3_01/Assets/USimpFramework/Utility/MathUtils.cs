using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USimpFramework.Utility
{
    public static class MathUtils
    {
        public static float Round(float value, int decimalNumber)
        {
            int fraction = (int)Mathf.Pow(10, decimalNumber);
            return (float)Mathf.Round(value * fraction) / fraction;
        }

        public static float Remap(float inValue, float inMin, float inMax, float outMin, float outMax)
        {
            return outMin + (inValue - inMin) * (outMax - outMin) / (inMax - inMin);

        }
    }
}
