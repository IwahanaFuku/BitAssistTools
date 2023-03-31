using UnityEngine;
using UnityEditor;

public class SelectAssetEditor : EditorWindow
{
    private string filePath = "Assets/";

    [MenuItem("Iwahana Tools/ファイルパス選択", priority = 18)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<SelectAssetEditor>("ファイルパス選択");
    }

    private void OnGUI()
    {
        GUILayout.Label("パス:", EditorStyles.boldLabel);
        filePath = EditorGUILayout.TextField(filePath);

        if (GUILayout.Button("選択"))
        {
            ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)));
        }
    }
}
