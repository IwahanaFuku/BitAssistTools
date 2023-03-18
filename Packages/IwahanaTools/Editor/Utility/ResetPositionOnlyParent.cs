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