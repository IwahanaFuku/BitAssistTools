using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SelectionAsistance
{
    public class SelectionHistoryFunction
    {    
        public static List<SelectionObjectsData> AddSelection(int maxHistoryCount, List<SelectionObjectsData> historyList)
        {
            SelectionObjectsData[] selectionObjectsData = new SelectionObjectsData[1];
            Object[] selectedObjects = Selection.objects;
            if (selectedObjects.Length > 0)
            {
                if (historyList.Count >= maxHistoryCount)
                {
                    historyList.RemoveAt(0);
                }

                SelectionObjectsData newData = new SelectionObjectsData(true, "Selections " + "(" + historyList.Count + ")", selectedObjects);
                historyList.Add(newData);
            }
            else
            {
                Debug.LogError("Select one or more objects.");
            }

            return historyList;
        }

        public static void LoadSelection(List<SelectionObjectsData> historyList)
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

        public static List<SelectionObjectsData> DeleatSelection(List<SelectionObjectsData> historyList)
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
            return historyList;
        }

        public static List<SelectionObjectsData> AllSelection(List<SelectionObjectsData> historyList)
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
            return historyList;
        }
    }
}
