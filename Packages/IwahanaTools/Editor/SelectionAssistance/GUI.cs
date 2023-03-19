using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SelectionsAssistanceEditorWindow : EditorWindow
{
    private Rect firstRect = new Rect();
    private Rect secondRect = new Rect();
    private GUIStyle firstStyle = null;

    private List<Object[]> historyList = new List<Object[]>();
    private int maxHistoryCount = 50;
    private List<int> selectedIndex = new List<int>();
    private List<bool> selected = new List<bool>();
    private Vector2 scrollPos;
    
    [MenuItem("Iwahana Tools/選択支援ツール", priority = 10)]
    public static void ShowWindow()
    {
        GetWindow<SelectionsAssistanceEditorWindow>("選択支援ツール");
    }
    
    void OnGUI()
    {
        CalculateRects();
        SetBackgroundStyles();

        GUILayout.BeginArea(firstRect, firstStyle);
        SelectionHistoryGUI();
        GUILayout.EndArea();

        GUILayout.BeginArea(secondRect);
        SelectionFunctionsGUI();
        GUILayout.EndArea();
    }

    void SelectionHistoryGUI()
    {
        EditorGUILayout.LabelField("選択履歴の保存");
        if (GUILayout.Button("保存"))
        {
            SaveSelectionGUI();
        }

        if (historyList.Count > 0)
        {
            HistoryListGUI();
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ロード"))
        {
            LoadSelectionGUI();
        }

        if (GUILayout.Button("削除"))
        {
            DeleatSelectionGUI();
        }

        if (GUILayout.Button("クリア"))
        {
            ClearSelectionGUI();
        }
    }

    void SaveSelectionGUI()
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
        else
        {
            Debug.LogError("Select one or more objects.");
        }
    }

    void HistoryListGUI()
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

    void LoadSelectionGUI()
    {
        List<Object> selectedObjects = new List<Object>();
        foreach (int i in selectedIndex)
        {
            selectedObjects.AddRange(historyList[i]);
        }
        Selection.objects = selectedObjects.ToArray();
    }

    void DeleatSelectionGUI()
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

    void ClearSelectionGUI()
    {
        historyList.Clear();
        selectedIndex.Clear();
        selected.Clear();
    }

    void SelectionFunctionsGUI()
    {
        EditorGUILayout.LabelField("選択ツール");
    }

    private void CalculateRects()
    {
        firstRect.Set(0, 0, position.width / 2, position.height);
        secondRect.Set(position.width / 2, 0, position.width / 2, position.height);
    }

    private void SetBackgroundStyles()
    {
        if (firstStyle == null)
        {
            firstStyle = new GUIStyle(GUI.skin.box);
            firstStyle.normal.background = CreateColoredTexture(EditorGUIUtility.isProSkin ? 0.2f : 0.8f);
        }
    }

    private Texture2D CreateColoredTexture(float brightness)
    {
        Color bgColor = GUI.backgroundColor;
        Color adjustedColor = new Color(bgColor.r * brightness, bgColor.g * brightness, bgColor.b * brightness);
        int w = (int)firstRect.width;
        int h = (int)firstRect.height;
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = adjustedColor;
        }
        Texture2D result = new Texture2D(w, h);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}