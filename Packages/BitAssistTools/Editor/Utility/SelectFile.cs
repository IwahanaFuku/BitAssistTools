using UnityEngine;
using UnityEditor;

public class SelectAssetEditor : EditorWindow
{
    string filePath = "Assets/";

    [MenuItem("Bit Assist Tools/ファイルパス選択", priority = 51)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<SelectAssetEditor>("ファイルパス選択");
    }

    void OnGUI()
    {
        GUILayout.Label("パス:", EditorStyles.boldLabel);
        filePath = EditorGUILayout.TextField(filePath);

        if (GUILayout.Button("選択"))
        {
            ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)));
        }
    }
}
