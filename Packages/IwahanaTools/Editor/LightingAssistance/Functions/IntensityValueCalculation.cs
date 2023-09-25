using UnityEngine;
using UnityEditor;

namespace LightingAssistance
{
    public class IntensityValueCalculationer : MonoBehaviour
    {
        public static void IntensityValueCalculation(float intensityValue, FourArithmeticOperations selectedOperation)
        {
            GameObject[] selectionObjects = Selection.gameObjects;
            foreach (GameObject selectionObject in selectionObjects)
            {
                Light light = selectionObject.GetComponent<Light>();
                if (light != null)
                {
                    if (selectedOperation == FourArithmeticOperations.Addition)
                    {
                        light.intensity += intensityValue;
                    }
                    else
                    {
                        light.intensity *= intensityValue;
                    }
                }
            }
        }
    }
}
