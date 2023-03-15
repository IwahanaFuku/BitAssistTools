using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SelectionHistory : EditorWindow
{
    private List<Object[]> historyList = new List<Object[]>();
    private int maxHistoryCount = 10;
    private List<int> selectedIndex = new List<int>();
    private List<bool> selected = new List<bool>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Selection History")]
    static void Init()
    {
        SelectionHistory window = (SelectionHistory)EditorWindow.GetWindow(typeof(SelectionHistory));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Selection History", EditorStyles.boldLabel);

        // Save Button
        if (GUILayout.Button("Save Selection"))
        {
            Object[] selectedObjects = Selection.objects;
            if (selectedObjects.Length > 0)
            {
                if (historyList.Count >= maxHistoryCount)
                {
                    historyList.RemoveAt(0);
                }
                historyList.Add(selectedObjects);
                selectedIndex.Clear();
                selectedIndex.Add(historyList.Count - 1);
                selected.Clear();
                selected.Add(false);
            }
        }

        // History List
        if (historyList.Count > 0)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < historyList.Count; i++)
            {
                bool isSelected = selectedIndex.Contains(i);
                isSelected = GUILayout.Toggle(isSelected, "Selection " + i);
                if (isSelected && !selectedIndex.Contains(i))
                {
                    selectedIndex.Add(i);
                    selected.Add(false);
                }
                else if (!isSelected && selectedIndex.Contains(i))
                {
                    int indexToRemove = selectedIndex.IndexOf(i);
                    selectedIndex.RemoveAt(indexToRemove);
                    selected.RemoveAt(indexToRemove);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        // Load Button
        bool loadEnabled = (selectedIndex.Count > 0);
        GUI.enabled = loadEnabled;
        if (GUILayout.Button("Load Selection"))
        {
            List<Object> selectedObjects = new List<Object>();
            foreach (int i in selectedIndex)
            {
                selectedObjects.AddRange(historyList[i]);
            }
            Selection.objects = selectedObjects.ToArray();
        }

        // Delete Button
        GUI.enabled = (selectedIndex.Count > 0);
        if (GUILayout.Button("Delete Selection"))
        {
            List<int> indicesToRemove = new List<int>();
            for (int i = 0; i < selectedIndex.Count; i++)
            {
                if (selected[i])
                {
                    indicesToRemove.Add(selectedIndex[i]);
                }
            }
            indicesToRemove.Sort();
            indicesToRemove.Reverse();
            foreach (int index in indicesToRemove)
            {
                historyList.RemoveAt(index);
            }
            selectedIndex.Clear();
            selected.Clear();
        }

        // Clear Button
        GUI.enabled = (historyList.Count > 0);
        if (GUILayout.Button("Clear History"))
        {
            historyList.Clear();
            selectedIndex.Clear();
            selected.Clear();
        }
        GUI.enabled = true;
    }
}
