using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class MissingScriptsRemover
{
    [MenuItem("Bit Assist Tools/Utility/Missing Scriptsを削除", priority = 70)]
    static void RemoveMissingScriptsFromSelected()
    {
        var selectedGameObjects = Selection.gameObjects;
        var allChildren = selectedGameObjects.SelectMany(go => go.GetComponentsInChildren<Transform>(true))
                                            .Select(t => t.gameObject);

        var prefabs = new HashSet<Object>();
        int removedComponentCount = 0;
        int removedGameObjectCount = 0;

        foreach (var gameObject in allChildren)
        {
            int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

            if (missingScriptCount > 0)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
                {
                    RemoveMissingScriptsFromPrefabSource(gameObject, prefabs, ref removedComponentCount, ref removedGameObjectCount);
                    missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                    if (missingScriptCount == 0)
                        continue;
                }

                Undo.RegisterCompleteObjectUndo(gameObject, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                removedComponentCount += missingScriptCount;
                removedGameObjectCount++;
            }
        }

        Debug.Log($" {removedGameObjectCount} 個のゲームオブジェクトから、{removedComponentCount} 個のミッシングになったスクリプトコンポーネントを削除しました。");
    }

    static void RemoveMissingScriptsFromPrefabSource(GameObject instance, HashSet<Object> prefabs, ref int compCount, ref int goCount)
    {
        var sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);
        if (sourcePrefab == null || !prefabs.Add(sourcePrefab))
            return;

        RemoveMissingScriptsFromPrefabSource(sourcePrefab, prefabs, ref compCount, ref goCount);

        int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(sourcePrefab);
        if (missingScriptCount > 0)
        {
            Undo.RegisterCompleteObjectUndo(sourcePrefab, "Remove missing scripts");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(sourcePrefab);
            compCount += missingScriptCount;
            goCount++;
        }
    }
}
