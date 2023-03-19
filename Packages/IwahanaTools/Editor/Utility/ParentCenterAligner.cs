using UnityEngine;
using UnityEditor;
using System.Linq;


public class CommonFunctions
{
    public static void ParentCenterAligner(Transform[] selectedTransforms)
    {
        int firstIndex = selectedTransforms.Min(t => t.GetSiblingIndex());

        Vector3 center = Vector3.zero;
        foreach (Transform transform in selectedTransforms)
        {
            center += transform.position;
        }
        center /= selectedTransforms.Length;
        var group = new GameObject("Group");
        group.transform.position = center;
        Undo.RegisterCreatedObjectUndo(group, "Group Selected");
        group.transform.SetSiblingIndex(firstIndex);

        group.transform.SetParent(Selection.activeTransform.parent, false);
        foreach (var transform in selectedTransforms.OrderBy(t => t.GetSiblingIndex()))
        {
            Undo.SetTransformParent(transform, group.transform, "Group Selected");
        }
        Selection.activeGameObject = group;
    }
}

