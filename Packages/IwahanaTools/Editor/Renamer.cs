using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;

public class Renamer : EditorWindow
{
    private bool renameToMaterialNameAccordion = true;
    private bool renameToTextureNameAccordion = true;
    private bool addObjectNumberAccordion = true;
    private bool removeObjectNumberAccordion = true;

    [MenuItem("Iwahana Tools/リネーマー", priority = 10)]
    public static void ShowWindow()
    {
        GetWindow<Renamer>("リネーマー");
    }

    void OnGUI()
    {
        renameToTextureNameAccordion = EditorGUILayout.Foldout(renameToTextureNameAccordion, "マテリアルをテクスチャの名前でリネーム");
        if (renameToTextureNameAccordion)
        {
            if (GUILayout.Button("実行"))
            {
                RenameToTextureName();
            }
        }

        renameToMaterialNameAccordion = EditorGUILayout.Foldout(renameToMaterialNameAccordion, "オブジェクトをマテリアルの名前でリネーム");
        if (renameToMaterialNameAccordion)
        {
            if (GUILayout.Button("実行"))
            {
                RenameToMaterialName();
            }
        }

        addObjectNumberAccordion = EditorGUILayout.Foldout(addObjectNumberAccordion, "オブジェクトに番号を付与");
        if (addObjectNumberAccordion)
        {
            if (GUILayout.Button("実行"))
            {
                AddObjectNumber();
            }
        }

        removeObjectNumberAccordion = EditorGUILayout.Foldout(removeObjectNumberAccordion, "オブジェクトから番号を除去");
        if (removeObjectNumberAccordion)
        {
            if (GUILayout.Button("実行"))
            {
                RemoveObjectNumber();
            }
        }
    }


    private void RenameToTextureName()
    {
        Undo.RecordObjects(Selection.gameObjects, "Change Texture Name");
        Object[] selectedObjects = Selection.objects;

        foreach (Object selectedObject in selectedObjects)
        {
            Material selectedMaterial = selectedObject as Material;
            Texture albedoTexture = selectedMaterial.GetTexture("_MainTex");

            if (selectedMaterial != null)
            {
                string selectedMaterialName = AssetDatabase.GetAssetPath(selectedMaterial);
                string textureName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(albedoTexture));
                if(selectedMaterial.name != "")
                {
                    Debug.Log(textureName);
                    AssetDatabase.RenameAsset(selectedMaterialName, textureName);
                    AssetDatabase.SaveAssets();             
                }
            }
        }
    }

    private void RenameToMaterialName()
    {
        Undo.RecordObjects(Selection.gameObjects, "Change Material Name");
        GameObject[] selectedObjects = Selection.gameObjects;
        
        foreach (GameObject obj in selectedObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null)
            {
                continue;
            }

            Material mat = renderer.sharedMaterial;
            if (mat == null)
            {
                continue;
            }

            obj.name = mat.name;
        }
    }

    private void AddObjectNumber()
    {
        Undo.RecordObjects(Selection.gameObjects, "Add Object Number");
        GameObject[] selectedObjects = Selection.gameObjects;

        System.Array.Sort(selectedObjects, (x, y) => 
        {
            int depthDifference = GetDepth(x.transform) - GetDepth(y.transform);
            if (depthDifference != 0)
            {
                return depthDifference;
            }
            return x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex());
        });

        Selection.objects = selectedObjects;
        Dictionary<string, int> names = new Dictionary<string, int>();
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            string newName;
            if (names.ContainsKey(selectedObjects[i].name))
            {
                newName = selectedObjects[i].name + " " + "(" + (names[selectedObjects[i].name] + 1).ToString() + ")";
                names[selectedObjects[i].name]++;
            }
            else
            {
                newName = selectedObjects[i].name + " " + "(" + (1).ToString() + ")";
                names[selectedObjects[i].name] = 1;
            }
            selectedObjects[i].name = newName;
        }
    }

    private int GetDepth(Transform transform)
    {
        int depth = 0;
        while (transform.parent != null)
        {
            depth++;
            transform = transform.parent;
        }
        return depth;
    }

    private void RemoveObjectNumber()
    {
        Undo.RecordObjects(Selection.gameObjects, "Remove Object Number");
        GameObject[] gameObjects = Selection.gameObjects;
        foreach (GameObject gameObject in gameObjects)
        {
            string name = gameObject.name;
            string newName = Regex.Replace(name, @"\s*\(\d+\)$", "");
            gameObject.name = newName;
        }
    }
}