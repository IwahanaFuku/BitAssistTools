using UnityEngine;
using UnityEditor;
using System.Linq;


public class CommonFunctions
{
    public static void ParentCenterAligner(Transform[] selectedTransforms)
    {
        int firstIndex = selectedTransforms.Min(t => t.GetSiblingIndex());

        var go = new GameObject("Group");
        Undo.RegisterCreatedObjectUndo(go, "Group Selected");
        go.transform.SetSiblingIndex(firstIndex);

        go.transform.SetParent(Selection.activeTransform.parent, false);
        foreach (var transform in selectedTransforms.OrderBy(t => t.GetSiblingIndex()))
        {
            Undo.SetTransformParent(transform, go.transform, "Group Selected");
        }
        Selection.activeGameObject = go;
    }
}

