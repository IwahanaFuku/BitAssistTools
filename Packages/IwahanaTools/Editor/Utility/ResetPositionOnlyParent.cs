using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ResetPositionOnlyParent
    {
    [MenuItem( "CONTEXT/Transform/Reset Position Only Parent" )]
    private static void Reset( MenuCommand command )
    {
        var parent = command.context as Transform;
        var tempParent = new GameObject();
        var children = parent.Cast<Transform>().ToList();


        Undo.RecordObject(parent, "Reset Position Only Parent");
        foreach (var child in children)
        {
            Undo.RecordObject(child, "Reset Position Only Parent");
            child.parent = tempParent.transform;
        }

        parent.position = Vector3.zero;

        foreach (var child in children)
        {
            Undo.RecordObject(child, "Reset Position Only Parent");
            child.parent = parent;
        }

        Undo.RecordObject(parent, "Reset Position Only Parent");
        GameObject.DestroyImmediate(tempParent);
    }
}

public static class ResetPivotToParent
{
    [MenuItem("CONTEXT/Transform/Reset Pivot To Parent")]
    private static void Reset(MenuCommand command)
    {
        var parent = command.context as Transform;
        var tempParent = new GameObject();
        var children = parent.Cast<Transform>().ToList();


        Undo.RecordObject(parent, "Reset Position Only Parent");
        foreach (var child in children)
        {
            Undo.RecordObject(child, "Reset Position Only Parent");
            child.parent = tempParent.transform;
        }
        
        GameObject selectedObject = Selection.activeGameObject;
        /* Transform[] childrenTransform = selectedObject.GetComponentsInChildren<Transform>(true);
        Vector3 center = Vector3.zero;
        int numOfChildren = 0;
        foreach (Transform child in childrenTransform)
        {
            Debug.Log(child.name);
            center += child.position;
            numOfChildren += 1;
        }
        center /= numOfChildren;
        Debug.Log(center); */

        Transform[] childrenTransform = selectedObject.GetComponentsInChildren<Transform>(true);

        // 取得した子オブジェクトをログに表示
        foreach (Transform child in childrenTransform)
        {
            Debug.Log(child.gameObject.name);
        }
        parent.position = Vector3.zero;

        foreach (var child in children)
        {
            Undo.RecordObject(child, "Reset Position Only Parent");
            child.parent = parent;
        }

        Undo.RecordObject(parent, "Reset Position Only Parent");
        GameObject.DestroyImmediate(tempParent);
    }
}