using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace PlacemenAssistance
{
    public class SpecialDuplicater
    {
        public void SpecialDuplicate(CoordinateReference coordinateReference, TransformData duplicateTransform, int copyCount)
        {
            Transform[] selectedTransforms = Selection.transforms;
            foreach (Transform selectedTransform in selectedTransforms)
            {
                GameObject selectedObject = selectedTransform.gameObject;
                GameObject newObject;
                TransformData newTransform = new TransformData(selectedTransform.position, selectedTransform.rotation, selectedTransform.localScale);
                List<Transform> copyTransforms = new List<Transform>();
                copyTransforms.Add(selectedTransform);

                if (copyCount < 1) { break; }

                for (int i = 0; i < copyCount; i++)
                {
                    if (coordinateReference == CoordinateReference.Global)
                    {
                        newTransform.position += duplicateTransform.position;
                        newTransform.rotation = Quaternion.AngleAxis(duplicateTransform.rotation.eulerAngles.x, Vector3.right) * newTransform.rotation;
                        newTransform.rotation = Quaternion.AngleAxis(duplicateTransform.rotation.eulerAngles.y, Vector3.up) * newTransform.rotation;
                        newTransform.rotation = Quaternion.AngleAxis(duplicateTransform.rotation.eulerAngles.z, Vector3.forward) * newTransform.rotation;
                        newTransform.localScale = Vector3.Scale(newTransform.localScale, duplicateTransform.localScale);
                    }
                    else if (coordinateReference == CoordinateReference.Local)
                    {
                        newTransform.position += selectedTransform.TransformDirection(duplicateTransform.position);
                        newTransform.rotation = newTransform.rotation * Quaternion.Euler(selectedTransform.TransformDirection(duplicateTransform.rotation.eulerAngles));
                        newTransform.localScale = Vector3.Scale(newTransform.localScale, duplicateTransform.localScale);
                    }

                    PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(selectedObject);
                    if(prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                    {
                        GameObject assetComponentOrGameObject = (GameObject)GetPrefabGameObject(selectedObject);
                        newObject = PrefabUtility.InstantiatePrefab(assetComponentOrGameObject) as GameObject;
                    }
                    else
                    {
                        newObject = UnityEngine.Object.Instantiate(selectedTransform.gameObject);
                    }
                    
                    newObject.transform.position = newTransform.position;
                    newObject.transform.rotation = newTransform.rotation;
                    newObject.transform.localScale = newTransform.localScale;
                    newObject.name = selectedTransform.name;

                    Undo.RegisterCreatedObjectUndo(newObject, "Create Copy");
                    copyTransforms.Add(newObject.transform);
                }

                CommonFunctions.ParentCenterAligner(copyTransforms.ToArray());
            }
        }

        GameObject GetPrefabGameObject(GameObject obj)
        {
            UnityEngine.Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            string path = "";
            if (prefab != null) path = UnityEditor.AssetDatabase.GetAssetPath(prefab);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}