using UnityEngine;
using UnityEditor;
using System.Linq;
using static System.Convert;
using System.Collections.Generic;

namespace PlacemenAssistance
{
    public class RandomScale : MonoBehaviour
    {
        public void RandomScaleObjects(bool isDetail, float scaleRatio, float minimum, float maximum)
        {
            Undo.RecordObjects(Selection.objects, "Randomize Scale");
            foreach (var selectedObject in Selection.objects)
            {
                var transform = (selectedObject as GameObject).transform;
                var newscale = 1f;

                if(isDetail == false)
                {
                    newscale = newscale * Random.Range(minimum, maximum);
                }

                var scale = scaleRatio * newscale;
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        public void RandomDetailScaleObjects(float scaleRatio, Vector3 minScale, Vector3 maxScale)
        {
            foreach (Transform transform in Selection.transforms)
            {
                Vector3 scale = transform.localScale;
                scale.x = scaleRatio * Random.Range(Mathf.Min(minScale.x, maxScale.x), Mathf.Max(minScale.x, maxScale.x));
                scale.y = scaleRatio * Random.Range(Mathf.Min(minScale.y, maxScale.y), Mathf.Max(minScale.y, maxScale.y));
                scale.z = scaleRatio * Random.Range(Mathf.Min(minScale.z, maxScale.z), Mathf.Max(minScale.z, maxScale.z));

                transform.localScale = scale;
            }
        }
    }

    public class RandomRotate : MonoBehaviour
    {
        public void RandomRotateObjects(float rotationAmount, AxisFlags axisFlags)
        {
            Undo.RecordObjects(Selection.transforms, "Random Rotate Objects");

            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                if (selectedObject != null)
                {
                    float xRotation = 0f;
                    float yRotation = 0f;
                    float zRotation = 0f;

                    if (axisFlags.X)
                    {
                        xRotation = Mathf.Floor(Random.Range(0, 360) / rotationAmount) * rotationAmount;
                    }

                    if (axisFlags.Y)
                    {
                        yRotation = Mathf.Floor(Random.Range(0, 360) / rotationAmount) * rotationAmount;
                    }

                    if (axisFlags.Z)
                    {
                        zRotation = Mathf.Floor(Random.Range(0, 360) / rotationAmount) * rotationAmount;
                    }

                    selectedObject.transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
                }
            }
        }
    }

    public class RandomPosition : MonoBehaviour
    {
        public void RandomPositionObjects(float radius, AxisFlags axisFlags)
        {
            Transform[] selectedTransforms = Selection.transforms;

            Undo.RegisterCompleteObjectUndo(selectedTransforms, "Random Position");

            foreach (Transform selectedTransform in selectedTransforms)
            {
                Vector3 randomPosition = Random.insideUnitSphere * radius;

                if (!axisFlags.X)
                {
                    randomPosition.x = 0;
                }

                if (!axisFlags.Y)
                {
                    randomPosition.y = 0;
                }

                if (!axisFlags.Z)
                {
                    randomPosition.z = 0;
                }

                selectedTransform.position += randomPosition;
            }

            Undo.RecordObjects(selectedTransforms, "Random Position");

            foreach (Transform selectedTransform in selectedTransforms)
            {
                EditorUtility.SetDirty(selectedTransform.gameObject);
            }
        }
    }
}
