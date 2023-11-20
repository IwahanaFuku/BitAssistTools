using UnityEngine;
using UnityEditor;
using System.Linq;
using static System.Convert;
using System.Collections.Generic;

namespace PlacemenAssistance
{
    public class RandomPosition
    {
        public static void RandomPositionObjects(float radius, AxisFlags axisFlags)
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

    public class RandomRotate
    {
        public static void RandomRotateObjects(float rotationAmount, AxisFlags axisFlags, AnimationCurve curve)
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
                        float value = Random.value;
                        float curveValue = curve.Evaluate(value);
                        xRotation = Mathf.Floor(curveValue / rotationAmount) * rotationAmount;
                        selectedObject.transform.Rotate(Vector3.right, xRotation, Space.World);
                    }

                    if (axisFlags.Y)
                    {
                        float value = Random.value;
                        float curveValue = curve.Evaluate(value);
                        yRotation = Mathf.Floor(curveValue / rotationAmount) * rotationAmount;
                        selectedObject.transform.Rotate(Vector3.up, yRotation, Space.World);
                    }

                    if (axisFlags.Z)
                    {
                        float value = Random.value;
                        float curveValue = curve.Evaluate(value);
                        zRotation = Mathf.Floor(curveValue / rotationAmount) * rotationAmount;
                        selectedObject.transform.Rotate(Vector3.forward, zRotation, Space.World);
                    }                    
                }
            }
        }
    }

    public class RandomScale
    {
        public static void RandomScaleObjects(bool isDetail, float scaleRatio, float minimum, float maximum)
        {
            Undo.RecordObjects(Selection.objects, "Randomize Scale");
            foreach (Object selectedObject in Selection.objects)
            {
                Transform transform = (selectedObject as GameObject).transform;
                float newscale = 1f;

                if(isDetail == false)
                {
                    newscale = newscale * Random.Range(minimum, maximum);
                }

                float scale = scaleRatio * newscale;
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        public static void RandomDetailScaleObjects(float scaleRatio, Vector3 minScale, Vector3 maxScale)
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
}
