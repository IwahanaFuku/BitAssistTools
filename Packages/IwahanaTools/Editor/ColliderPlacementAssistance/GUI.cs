using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ColliderPlacemenAssistancer;

public class ColliderPlacemenAssistanceEditorWindow : EditorWindow
{
    Vector2 scrollPos;

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

        isConvertBoxColliderToTransformExpanded = EditorGUILayout.Foldout(isConvertBoxColliderToTransformExpanded, "トランスフォームからボックスコライダー変換");
        if (isConvertBoxColliderToTransformExpanded)
        {
            ConvertBoxColliderToTransformer ConvertBoxColliderToTransformerInstance = new ConvertBoxColliderToTransformer();
            if (GUILayout.Button("ボックスコライダーからトランスフォームへ"))
            {
                ConvertBoxColliderToTransformerInstance.ConvertBoxColliderToTransform();
            }
        }

        isConvertTransformToBoxColliderExpanded = EditorGUILayout.Foldout(isConvertTransformToBoxColliderExpanded, "ボックスコライダーからトランスフォーム変換");
        if (isConvertTransformToBoxColliderExpanded)
        {
            ConvertBoxColliderToTransformer ConvertBoxColliderToTransformerInstance = new ConvertBoxColliderToTransformer();
            if (GUILayout.Button("トランスフォームからボックスコライダーへ"))
            {
                ConvertBoxColliderToTransformerInstance.ConvertTransformeToBoxCollider();
            }

            if (GUILayout.Button("トランスフォームをリセットしてボックスコライダーへ　※Rotate不可※"))
            {
                ConvertBoxColliderToTransformerInstance.ConvertTransformeToBoxColliderReset();
            }
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

        if (GUILayout.Button("回転複製"))
        {
            RotateDuplicaterInstance.RotateDuplicate(rotationalFrequency);
        }
    }
}