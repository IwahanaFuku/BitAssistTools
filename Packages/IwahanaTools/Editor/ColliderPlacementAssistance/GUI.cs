using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ColliderPlacemenAssistancer
    {
    public class ColliderPlacemenAssistanceEditorWindow : EditorWindow
    {
        Vector2 scrollPos;
        int spacePixel = 8;

        private bool isConvertBoxColliderToTransformExpanded = true;
        private bool isConvertTransformToBoxColliderExpanded = false;
        private bool isRotationalFrequencyExpanded = false;

        private float rotationalFrequency = 360f;

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
                ConvertBoxColliderToTransformer ConvertBoxColliderToTransformerInstance = new ConvertBoxColliderToTransformer();

                GUILayout.Space(spacePixel);

                if (GUILayout.Button("ボックスコライダーからトランスフォームへ"))
                {
                    ConvertBoxColliderToTransformerInstance.ConvertBoxColliderToTransform();
                }

                GUILayout.Space(spacePixel);
            }

            isConvertTransformToBoxColliderExpanded = EditorGUILayout.Foldout(isConvertTransformToBoxColliderExpanded, "トランスフォームからボックスコライダーへ変換");
            if (isConvertTransformToBoxColliderExpanded)
            {
                ConvertBoxColliderToTransformer ConvertBoxColliderToTransformerInstance = new ConvertBoxColliderToTransformer();

                GUILayout.Space(spacePixel);

                if (GUILayout.Button("トランスフォームからボックスコライダーへ"))
                {
                    ConvertBoxColliderToTransformerInstance.ConvertTransformeToBoxCollider();
                }

                if (GUILayout.Button("トランスフォームをリセットしてボックスコライダーへ　※Rotate不可※"))
                {
                    ConvertBoxColliderToTransformerInstance.ConvertTransformeToBoxColliderReset();
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
            RotateDuplicater RotateDuplicaterInstance = new RotateDuplicater();

            rotationalFrequency = EditorGUILayout.Slider("度数制限:", rotationalFrequency, 1, 360);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("回転複製"))
            {
                RotateDuplicaterInstance.RotateDuplicate(rotationalFrequency);
            }
            
            GUILayout.Space(spacePixel);
        }
    }
}