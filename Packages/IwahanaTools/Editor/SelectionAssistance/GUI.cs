using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SelectionAsistance;

public class SelectionsAssistanceEditorWindow : EditorWindow
{
    private Rect firstRect = new Rect();
    private Rect secondRect = new Rect();
    private GUIStyle firstStyle = null;

    private List<SelectionObjectsData> historyList = new List<SelectionObjectsData>();
    private int maxHistoryCount = 20;
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
        EditorGUILayout.LabelField("選択履歴");
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

        if (GUILayout.Button("全選択"))
        {
            AllSelectionGUI();
        }
    }

    void SaveSelectionGUI()
    {
        SelectionObjectsData[] selectionObjectsData = new SelectionObjectsData[1];
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects.Length > 0)
        {
            if (historyList.Count >= maxHistoryCount)
            {
                historyList.RemoveAt(0);
            }

            SelectionObjectsData newData = new SelectionObjectsData(false, "Selection " + historyList.Count, selectedObjects);
            historyList.Add(newData);
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
                GUILayout.BeginHorizontal();
                SelectionObjectsData data = historyList[i];
                data.isChecked = GUILayout.Toggle(data.isChecked, "");
                data.name = EditorGUILayout.TextField("", data.name);
                historyList[i] = data;
                GUILayout.EndHorizontal();
            }
        EditorGUILayout.EndScrollView();
    }

    void LoadSelectionGUI()
    {
        List<Object> selectedObjects = new List<Object>();
        foreach (SelectionObjectsData data in historyList)
        {
            if (data.isChecked && data.objects != null)
            {
                selectedObjects.AddRange(data.objects);
            }
        }
        Selection.objects = selectedObjects.ToArray();
    }

    void DeleatSelectionGUI()
    {
        List<int> indicesToRemove = new List<int>();
        for (int i = 0; i < historyList.Count; i++)
        {
            SelectionObjectsData data = historyList[i];
            if (data.isChecked)
            {
                indicesToRemove.Add(i);
            }
        }

        if (indicesToRemove.Count > 0)
        {
            indicesToRemove.Sort();
            indicesToRemove.Reverse();
            foreach (int index in indicesToRemove)
            {
                historyList.RemoveAt(index);
            }
        }
    }


    void AllSelectionGUI()
    {
        bool allTrue = true;
        foreach (SelectionObjectsData data in historyList)
        {
            if (!data.isChecked)
            {
                allTrue = false;
                break;
            }
        }

        for (int i = 0; i < historyList.Count; i++)
        {
            SelectionObjectsData data = historyList[i];
            data.isChecked = !allTrue;
            historyList[i] = data;
        }
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