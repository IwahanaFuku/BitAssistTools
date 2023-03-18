using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlacemenAssistance
{
    public class ObjectTransformChanger : MonoBehaviour
    {
        public void ObjectTransformChange(TransformData changeTransform)
        {
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject.transform, "Transform Adder");

                gameObject.transform.position += changeTransform.position;
                gameObject.transform.rotation *= changeTransform.rotation;
                gameObject.transform.localScale += changeTransform.localScale;
            }
        }       
    }

    public class ObjectDistributor : MonoBehaviour
    {
        public void SortAndDistributeObjects(Axis axis)
        {
            var sortedPositions = GetSortedPositions(axis);
            UpdatePositions(axis, sortedPositions);
        }

        private List<KeyValuePair<int, Vector3>> GetSortedPositions(Axis axis)
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

        private void UpdatePositions(Axis axis, List<KeyValuePair<int, Vector3>> sortedPositions)
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


    public class ObjectAligner : MonoBehaviour
    {
        private Transform[] selectedTransforms;

        public void AlignObjects(Axis axis, AlignType alignType)
        {
            selectedTransforms = Selection.transforms;
            var targetValue = CalculateTargetValue(selectedTransforms, axis, alignType);
            UpdatePositions(selectedTransforms, axis, targetValue);
        }

        private float CalculateTargetValue(Transform[] transforms, Axis axis, AlignType alignType)
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

        private void UpdatePositions(Transform[] transforms, Axis axis, float targetValue)
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


    public class ObjectPaddinger : MonoBehaviour
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