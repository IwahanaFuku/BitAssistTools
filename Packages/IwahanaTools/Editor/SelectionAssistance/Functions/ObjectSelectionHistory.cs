using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SelectionAsistance
{
    public class SelectionHistory : EditorWindow
    {    
        List<Object[]> historyList = new List<Object[]>();
        int maxHistoryCount = 10;
        List<int> selectedIndex = new List<int>();
        List<bool> selected = new List<bool>();
        Vector2 scrollPos;

        void SaveSelection()
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
    }
}
