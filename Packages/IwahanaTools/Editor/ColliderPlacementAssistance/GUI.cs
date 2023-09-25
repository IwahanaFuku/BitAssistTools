using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ColliderPlacemenAssistancer
    {
    public class ColliderPlacemenAssistanceEditorWindow : EditorWindow
    {
        Vector2 scrollPos;
        int spacePixel = 8;

        bool isConvertBoxColliderToTransformExpanded = true;
        bool isConvertTransformToBoxColliderExpanded = false;
        bool isRotationalFrequencyExpanded = false;

        float rotationalFrequency = 360f;

        [MenuItem("Iwahana Tools/コライダー配置支援ツール", priority = 11)]
        public static void ShowWindow()
        {
            GetWindow<ColliderPlacemenAssistanceEditorWindow>("コライダー配置支援ツール");
        }

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            isConvertBoxColliderToTransformExpanded = EditorGUILayout.Foldout(isConvertBoxColliderToTransformExpanded, "ボックスコライダーからトランスフォームへ変換");
            if (isConvertBoxColliderToTransformExpanded)
            {
                GUILayout.Space(spacePixel);

                if (GUILayout.Button("ボックスコライダーからトランスフォームへ"))
                {
                    ConvertBoxColliderToTransformer.ConvertBoxColliderToTransform();
                }

                GUILayout.Space(spacePixel);
            }

            isConvertTransformToBoxColliderExpanded = EditorGUILayout.Foldout(isConvertTransformToBoxColliderExpanded, "トランスフォームからボックスコライダーへ変換");
            if (isConvertTransformToBoxColliderExpanded)
            {
                GUILayout.Space(spacePixel);

                if (GUILayout.Button("トランスフォームからボックスコライダーへ"))
                {
                    ConvertBoxColliderToTransformer.ConvertTransformeToBoxCollider();
                }

                if (GUILayout.Button("トランスフォームをリセットしてボックスコライダーへ　※Rotate不可※"))
                {
                    ConvertBoxColliderToTransformer.ConvertTransformeToBoxColliderReset();
                }

                GUILayout.Space(spacePixel);
            }

            isRotationalFrequencyExpanded = EditorGUILayout.Foldout(isRotationalFrequencyExpanded, "回転複製");
            if (isRotationalFrequencyExpanded)
            {
                RotationalFrequencyeGUI();
            }

            EditorGUILayout.EndScrollView();
        }

        void RotationalFrequencyeGUI()
        {
            rotationalFrequency = EditorGUILayout.Slider("度数制限:", rotationalFrequency, 1, 360);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("回転複製"))
            {
                RotateDuplicater.RotateDuplicate(rotationalFrequency);
            }
            
            GUILayout.Space(spacePixel);
        }
    }
}