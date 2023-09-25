using UnityEngine;
using UnityEditor;

namespace LightingAssistance
{
    public class LightIntensityEditor : EditorWindow
    {
        bool isIntensityValueCalculationExpanded = true;

        float intensityValue = 1f;
        FourArithmeticOperations selectedOperation = FourArithmeticOperations.Addition;

        [MenuItem("Iwahana Tools/ライティング支援ツール", priority = 12)]
        public static void ShowWindow()
        {
            GetWindow<LightIntensityEditor>("ライティング支援ツール");
        }

        void OnGUI()
        {

            isIntensityValueCalculationExpanded = EditorGUILayout.Foldout(isIntensityValueCalculationExpanded, "Intensityの変更");
            if (isIntensityValueCalculationExpanded)
            {
                IntensityValueCalculationGUI();
            }
        }

        void IntensityValueCalculationGUI()
        {
            selectedOperation = (FourArithmeticOperations)GUILayout.SelectionGrid((int)selectedOperation, new string[] { "加算", "乗算" }, 2);
            intensityValue = EditorGUILayout.FloatField("Intensity Value", intensityValue);
            
            if (GUILayout.Button("適用"))
            {
                IntensityValueCalculationer.IntensityValueCalculation(intensityValue, selectedOperation);
            }
        }
    }
}
