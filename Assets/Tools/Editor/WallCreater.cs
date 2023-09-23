using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NewBehaviourScript : Editor
{

    [MenuItem("Naya_Tools/WallCreater")]
    static void Creat()
    {
        
        GameObject a = Selection.gameObjects[0];
        GameObject b = Selection.gameObjects[1];

        float size = Vector3.Distance(a.transform.position, b.transform.position);
        a.transform.LookAt(b.transform);

        GameObject obj =  GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.parent = a.transform.parent;

        obj.transform.position = (a.transform.position + b.transform.position) / 2;
        obj.transform.rotation = a.transform.rotation;
        obj.transform.localScale = new Vector3(1, 1, size);

    }
}
