using UnityEngine;
using UnityEditor;

namespace LightingAssistance
{
    public class LightIntensityEditor : EditorWindow
    {
        private float intensityValue = 1f; // ライトの強度に加算する値
        private FourArithmeticOperations selectedOperation = FourArithmeticOperations.Addition;

        [MenuItem("Iwahana Tools/ライティング支援ツール", priority = 12)]
        public static void ShowWindow()
        {
            GetWindow<LightIntensityEditor>("ライティング支援ツール");
        }

        void OnGUI()
        {

            selectedOperation = (FourArithmeticOperations)GUILayout.SelectionGrid(
                (int)selectedOperation,
                new string[] { "加算", "乗算" },
                2);

            intensityValue = EditorGUILayout.FloatField("Intensity Value", intensityValue);
            
            if (GUILayout.Button("適用"))
            {
                foreach (GameObject obj in Selection.gameObjects)
                {
                    Light light = obj.GetComponent<Light>();
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
}
