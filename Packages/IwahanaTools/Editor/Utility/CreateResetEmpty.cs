using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;



public class CreateTransformResetObject : Editor
{
    [MenuItem("GameObject/Create Reset Empty", false, -1)]
    private static void CreateTransformReset(MenuCommand menuCommand)
    {
        Transform parentTransform = null;

        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            parentTransform = prefabStage.prefabContentsRoot.transform;
        }
        
        if (Selection.activeTransform != null)
        {
           GameObject parentObject = Selection.activeGameObject;
            parentTransform = parentObject != null ? parentObject.transform : null;
        }

        GameObject go = new GameObject("GameObject");
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        if (parentTransform != null)
        {
            go.transform.SetParent(parentTransform, false);
        }

        Selection.activeGameObject = go;

        Undo.RegisterCreatedObjectUndo(go, "Create Transform Reset Object");
    }
}
