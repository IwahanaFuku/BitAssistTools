using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlacemenAssistance
{
    public class Roundinger
    {
        public void Rounding(int multiplier, TransformFlags roundingTransformFlags)
        {
            GameObject[] selectionTransforms = Selection.gameObjects;
            foreach (var selectedObject in selectionTransforms)
            {
                Transform selectedTransform = selectedObject.transform;
                Undo.RecordObject(selectedTransform, "Rounding");

                if(roundingTransformFlags.position)
                {
                    Vector3 roundPosition = GetRoundVector3(selectedTransform.position, multiplier);
                    selectedTransform.position = roundPosition;
                }
                if(roundingTransformFlags.rotation)
                {
                    Vector3 roundRotation = GetRoundVector3(selectedTransform.eulerAngles, multiplier);
                    selectedTransform.eulerAngles = roundRotation;

                }
                if(roundingTransformFlags.scale)
                {
                    Vector3 roundScale = GetRoundVector3(selectedTransform.localScale, multiplier);
                    selectedTransform.localScale = roundScale;
                }

                Undo.FlushUndoRecordObjects();
            }
        }

        Vector3 GetRoundVector3(Vector3 vector3, int roundPoint)
        {
            float digit = Mathf.Pow(10, roundPoint);

            float x = vector3.x * digit;
            float y = vector3.y * digit;
            float z = vector3.z * digit;
    
            x = Mathf.Round(x) / digit;
            y = Mathf.Round(y) / digit;
            z = Mathf.Round(z) / digit;

            vector3 = new Vector3(x, y, z);
            return vector3;
        }
    }

    public class ObjectTransformChanger
    {
        public void ObjectTransformChange(CoordinateReference coordinateReference, TransformData changeTransform)
    {
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            Undo.RecordObject(gameObject.transform, "Transform Adder");

            if (coordinateReference == CoordinateReference.Global)
            {
                gameObject.transform.position += changeTransform.position;
                gameObject.transform.rotation *= changeTransform.rotation;
                gameObject.transform.localScale += changeTransform.localScale;
            }
            else if (coordinateReference == CoordinateReference.Local)
            {
                Transform transform = gameObject.transform;
                gameObject.transform.localPosition += transform.TransformDirection(changeTransform.position);
                gameObject.transform.localRotation *= changeTransform.rotation;
                gameObject.transform.localScale += changeTransform.localScale;
            }
        }
    }     
    }

    public class ObjectDistributor
    {
        public void SortAndDistributeObjects(Axis axis)
        {
            var sortedPositions = GetSortedPositions(axis);
            UpdatePositions(axis, sortedPositions);
        }

        List<KeyValuePair<int, Vector3>> GetSortedPositions(Axis axis)
        {
            var positions = Selection.gameObjects
                .Select((obj, index) => new KeyValuePair<int, Vector3>(index, obj.transform.position))
                .ToList();

            switch (axis)
            {
                case Axis.X:
                    return positions.OrderBy(pair => pair.Value.x).ToList();
                case Axis.Y:
                    return positions.OrderBy(pair => pair.Value.y).ToList();
                case Axis.Z:
                    return positions.OrderBy(pair => pair.Value.z).ToList();
                default:
                    return positions;
            }
        }

        void UpdatePositions(Axis axis, List<KeyValuePair<int, Vector3>> sortedPositions)
        {
            var axisIndex = (int)axis;
            var firstPosition = sortedPositions.First().Value[axisIndex];
            var lastPosition = sortedPositions.Last().Value[axisIndex];
            var interval = (lastPosition - firstPosition) / (sortedPositions.Count - 1);

            Undo.RecordObjects(Selection.transforms, "Distribute Objects");

            for (var i = 0; i < sortedPositions.Count; i++)
            {
                var newPosition = sortedPositions[i].Value;
                newPosition[axisIndex] = firstPosition + i * interval;
                Selection.gameObjects[sortedPositions[i].Key].transform.position = newPosition;
            }

            Undo.FlushUndoRecordObjects();
        }
    }


    public class ObjectAligner
    {
        Transform[] selectedTransforms;

        public void AlignObjects(Axis axis, AlignType alignType)
        {
            selectedTransforms = Selection.transforms;
            var targetValue = CalculateTargetValue(selectedTransforms, axis, alignType);
            UpdatePositions(selectedTransforms, axis, targetValue);
        }

        float CalculateTargetValue(Transform[] transforms, Axis axis, AlignType alignType)
        {
            var values = new float[transforms.Length];

            for (int i = 0; i < transforms.Length; i++)
            {
                switch (axis)
                {
                    case Axis.X:
                        values[i] = transforms[i].position.x;
                        break;
                    case Axis.Y:
                        values[i] = transforms[i].position.y;
                        break;
                    case Axis.Z:
                        values[i] = transforms[i].position.z;
                        break;
                }
            }

            switch (alignType)
            {
                case AlignType.Maximum:
                    return Mathf.Max(values);
                case AlignType.Center:
                    return values.Average();
                case AlignType.Minimum:
                    return Mathf.Min(values);
                default:
                    return 0f;
            }
        }

        void UpdatePositions(Transform[] transforms, Axis axis, float targetValue)
        {
            Undo.RecordObjects(transforms, "Align Objects");

            foreach (Transform transform in transforms)
            {
                var pos = transform.position;

                switch (axis)
                {
                    case Axis.X:
                        pos.x = targetValue;
                        break;
                    case Axis.Y:
                        pos.y = targetValue;
                        break;
                    case Axis.Z:
                        pos.z = targetValue;
                        break;
                }

                transform.position = pos;
            }

            Undo.FlushUndoRecordObjects();
        }
    }


    public class ObjectPaddinger
    {
        public void PaddingObjects(bool isPaddingReferencePointExpanded, float paddingSpace,
                                   Transform referencePoint, AxisFlags axisFlags)
        {
            Undo.RecordObjects(Selection.transforms, "Apply Padding");

            Transform[] objects = Selection.transforms;
            Vector3 reference = Vector3.zero;

            if (isPaddingReferencePointExpanded && referencePoint != null)
            {
                reference = referencePoint.position;
            }
            else
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    reference += objects[i].position;
                }

                reference /= objects.Length;
            }
            for (int i = 0; i < objects.Length; i++)
            {
                Vector3 direction = objects[i].position - reference;
                
                float paddingX = direction.x;
                float paddingY = direction.y;
                float paddingZ = direction.z;

                Vector3 paddingDirection = direction * paddingSpace;
                
                if (axisFlags.X)
                {
                    paddingX = paddingDirection.x;
                }

                if (axisFlags.Y)
                {
                    paddingY = paddingDirection.y;
                }

                if (axisFlags.Z)
                {
                    paddingZ = paddingDirection.z;
                }
                
                objects[i].position = reference + new Vector3(paddingX, paddingY, paddingZ);
            }
        }
    }
}