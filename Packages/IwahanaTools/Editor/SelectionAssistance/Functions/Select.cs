using UnityEditor;
using UnityEngine;

public class ObjectSelector : EditorWindow
{
    private int parentHierarchy = 1;

    [MenuItem("Tools/Object Selector")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ObjectSelector));
    }

    private void OnGUI()
    {
        GUILayout.Label("Object Selector", EditorStyles.boldLabel);

        parentHierarchy = EditorGUILayout.IntField("Parent Hierarchy", parentHierarchy);

        if (GUILayout.Button("Select"))
        {
            SelectObjects();
        }
    }

    private void SelectObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No object selected");
            return;
        }

        GameObject firstSelectedObject = selectedObjects[0];
        Transform parent = firstSelectedObject.transform.parent;

        for (int i = 0; i < parentHierarchy && parent != null; i++)
        {
            parent = parent.parent;
        }

        if (parent == null)
        {
            Debug.LogWarning("Invalid parent hierarchy level");
            return;
        }

        GameObject[] siblings = new GameObject[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
        {
            siblings[i] = parent.GetChild(i).gameObject;
        }

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            int siblingIndex = siblings[i].transform.GetSiblingIndex();
            Selection.activeGameObject = parent.GetChild(siblingIndex).gameObject;
        }
    }

    private void OnSelectionChange()
    {
        Repaint();
    }
}
