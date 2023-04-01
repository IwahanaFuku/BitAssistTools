using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SelectionAsistance;
using System.IO;

public class SelectionsAssistanceEditorWindow : EditorWindow
{
    Rect firstRect = new Rect();
    Rect secondRect = new Rect();
    GUIStyle firstStyle = null;

    List<SelectionObjectsData> historyList = new List<SelectionObjectsData>();
    int maxHistoryCount = 20;
    Vector2 scrollPos;

    bool isHierarchicalSelectionExpanded = true;
    int parentCount = 1;
    bool onlyOrder = true;
    bool selectChildlen = false;
    
    [MenuItem("Iwahana Tools/選択支援ツール", priority = 12)]
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
        GUILayout.Label("選択履歴の保存");

        SelectionHistoryFunction SelectionHistoryFunctionInstance = new SelectionHistoryFunction();
        maxHistoryCount = (int)EditorGUILayout.FloatField("  履歴保存数", maxHistoryCount);
        EditorGUILayout.LabelField("  選択履歴");

        if (GUILayout.Button("追加"))
        {
            historyList = SelectionHistoryFunctionInstance.AddSelection(maxHistoryCount, historyList);
        }

        if (historyList.Count > 0)
        {
            HistoryListGUI();
        }

         GUILayout.FlexibleSpace();
        if (GUILayout.Button("ロード"))
        {
            SelectionHistoryFunctionInstance.LoadSelection(historyList);
        }

        if (GUILayout.Button("削除"))
        {
            historyList = SelectionHistoryFunctionInstance.DeleatSelection(historyList);
        }

        if (GUILayout.Button("全選択"))
        {
            historyList = SelectionHistoryFunctionInstance.AllSelection(historyList);
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

    void SelectionFunctionsGUI()
    {
        isHierarchicalSelectionExpanded = EditorGUILayout.Foldout(isHierarchicalSelectionExpanded, "階層選択");
        if (isHierarchicalSelectionExpanded)
        {
            hierarchicalSelectionGUI();
        }
    }

    void hierarchicalSelectionGUI()
    {
        HierarchicalSelectionObjects HierarchicalSelecterInstance = new HierarchicalSelectionObjects();
        parentCount = EditorGUILayout.IntSlider("基準とする親の階層", parentCount, 1, 5);
        onlyOrder = EditorGUILayout.Toggle("選択したオブジェクトと同じ順番", onlyOrder);
        selectChildlen = EditorGUILayout.Toggle("子オブジェクトをすべて選択する", selectChildlen);

        if (GUILayout.Button("選択"))
        {
            HierarchicalSelecterInstance.HierarchicalSelecter(parentCount, onlyOrder, selectChildlen);
        }
        
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