using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;

public class Renamer : EditorWindow
{
    Vector2 scrollPos;
    int spacePixel = 8;
    
    bool addPrefixAndSuffixAccordion = true;
    bool replaceAccordion = true;
    bool renameByTexture = true;
    bool objectNumberAccordion = true;

    
    string prefix = "Prefix";
    
    string suffix = "Suffix";
    string delimiter = "_";
    string searchString = "";
    string replaceString = "";

    [MenuItem("Bit Assist Tools/リネーマー", priority =50)]
    public static void ShowWindow()
    {
        GetWindow<Renamer>("リネーマー");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        addPrefixAndSuffixAccordion = EditorGUILayout.Foldout(addPrefixAndSuffixAccordion, "接頭辞と接尾辞を追加");
        if (addPrefixAndSuffixAccordion)
        {
            GUILayout.Space(spacePixel);

            prefix = EditorGUILayout.TextField("接頭辞", prefix);
            suffix = EditorGUILayout.TextField("接尾辞", suffix);
            delimiter = EditorGUILayout.TextField("接尾辞", delimiter);

            if (GUILayout.Button("実行"))
            {
                AddPrefixAndSuffix(prefix, suffix, delimiter);
            }

            GUILayout.Space(spacePixel);
        }

        replaceAccordion = EditorGUILayout.Foldout(replaceAccordion, "置換");
        if (replaceAccordion)
        {
            GUILayout.Space(spacePixel);

            searchString = EditorGUILayout.TextField("検索", searchString);
            replaceString = EditorGUILayout.TextField("置換", replaceString);

            if (GUILayout.Button("実行"))
            {
                ReplaceString(searchString, replaceString);
            }

            GUILayout.Space(spacePixel);
        }

        renameByTexture = EditorGUILayout.Foldout(renameByTexture, "テクスチャを基準にリネーム");
        if (renameByTexture)
        {
            GUILayout.Space(spacePixel);

            if (GUILayout.Button("マテリアルをテクスチャの名前でリネーム"))
            {
                RenameToTextureName();
            }

            if (GUILayout.Button("オブジェクトをマテリアルの名前でリネーム"))
            {
                RenameToMaterialName();
            }

            GUILayout.Space(spacePixel);
        }

        objectNumberAccordion = EditorGUILayout.Foldout(objectNumberAccordion, "オブジェクトに番号を付与/除去");
        if (objectNumberAccordion)
        {
            GUILayout.Space(spacePixel);

            if (GUILayout.Button("オブジェクトに番号を付与"))
            {
                AddObjectNumber();
            }

            if (GUILayout.Button("オブジェクトから番号を除去"))
            {
                RemoveObjectNumber();
            }

            GUILayout.Space(spacePixel);
        }
    
        EditorGUILayout.EndScrollView();
    }


    static void AddPrefixAndSuffix(string prefix, string suffix, string delimiter)
    {
        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            Undo.RecordObject(selectedObject, "Add Prefix And Suffix");
            selectedObject.name = prefix + delimiter + selectedObject.name;
            selectedObject.name = selectedObject.name + delimiter + suffix;
        }
    }

    static void ReplaceString(string searchString, string replaceString)
    {
        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            Undo.RecordObject(selectedObject, "Replace String");
            selectedObject.name = selectedObject.name.Replace(searchString, replaceString);
        }
    }

    static void RenameToTextureName()
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

    static void RenameToMaterialName()
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

    static void AddObjectNumber()
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

    static int GetDepth(Transform transform)
    {
        int depth = 0;
        while (transform.parent != null)
        {
            depth++;
            transform = transform.parent;
        }
        return depth;
    }

    static void RemoveObjectNumber()
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