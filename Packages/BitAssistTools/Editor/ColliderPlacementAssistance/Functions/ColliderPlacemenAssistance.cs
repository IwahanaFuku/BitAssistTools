using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ColliderPlacemenAssistancer
{
    public class ConvertBoxColliderToTransformer
    {
        public static void ConvertBoxColliderToTransform()
        {
            foreach (var selectedObject in Selection.gameObjects)
            {
                BoxCollider boxCollider = selectedObject.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    Debug.LogError("Selected GameObject does not have a BoxCollider component.");
                    continue;
                }

                if (selectedObject.GetComponent<MeshRenderer>() == null)
                {
                    MeshRenderer meshRenderer = Undo.AddComponent<MeshRenderer>(selectedObject);
                    meshRenderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                    meshRenderer.enabled = false;
                }

                if (selectedObject.GetComponent<MeshFilter>() == null)
                {
                    MeshFilter meshFilter = Undo.AddComponent<MeshFilter>(selectedObject);
                    meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                }

                if (boxCollider == null)
                {
                    Debug.LogWarning("Selected GameObject does not have a BoxCollider component.");
                    continue;
                }

                Undo.RecordObjects(new Object[] { selectedObject.transform, boxCollider }, "Convert BoxCollider Transform");

                selectedObject.transform.position = selectedObject.transform.TransformPoint(boxCollider.center);
                selectedObject.transform.localScale = Vector3.Scale(selectedObject.transform.localScale, boxCollider.size);

                boxCollider.center = Vector3.zero;
                boxCollider.size = Vector3.one;
            }
        }

        public static void ConvertTransformeToBoxCollider()
        {
            Transform[] transforms = Selection.transforms;
            foreach (Transform transform in transforms)
            {
                MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();
                if (meshFilter != null || meshRenderer != null)
                {
                    Undo.DestroyObjectImmediate(meshFilter);
                    Undo.DestroyObjectImmediate(meshRenderer);
                }

                BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    boxCollider = Undo.AddComponent<BoxCollider>(transform.gameObject);
                }

                boxCollider.size = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
                Vector3 center = Vector3.zero;
                Renderer renderer = transform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    center = renderer.bounds.center - transform.position;
                }
                boxCollider.center = center;

                transform.localScale = Vector3.one;
            }
        }


        public static void ConvertTransformeToBoxColliderReset()
        {
            Transform[] transforms = Selection.transforms;
            foreach (Transform transform in transforms)
            {
                BoxCollider boxCollider = transform.gameObject.GetComponent<BoxCollider>();

                MeshFilter meshFilter = transform.gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Undo.DestroyObjectImmediate(meshFilter);
                }

                MeshRenderer meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    Undo.DestroyObjectImmediate(meshRenderer);
                }

                if (boxCollider == null)
                {
                    boxCollider = transform.gameObject.AddComponent<BoxCollider>();
                }

                Vector3 center = transform.TransformPoint(boxCollider.center);
                Vector3 size = Vector3.Scale(transform.lossyScale, boxCollider.size);

                Quaternion rotation = Quaternion.Inverse(transform.rotation) * boxCollider.transform.rotation;

                Undo.RecordObject(boxCollider, "Transform To Box Collider");
                Undo.RecordObject(transform, "Transform To Box Collider");

                boxCollider.center = center;
                boxCollider.size = size;
                boxCollider.transform.localRotation = rotation;

                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                EditorUtility.SetDirty(boxCollider);
                EditorUtility.SetDirty(transform);
            }                
        }
    }
}
