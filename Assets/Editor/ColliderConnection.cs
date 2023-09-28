using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ColliderPlacemenAssistancer
{
    public static class BoxColliderPlacementer
    {
        public static List<GameObject> GenerateBoxesBetweenObjects(List<GameObject> objects, GameObject parent, bool isClose, float boxWidth, float boxHeight, AbstractPosition position)
        {
            List<GameObject> result = new List<GameObject>(); // 結果のボックスを格納するリスト

            for (int i = 0; i < objects.Count - 1; i++)
            {
                Vector3 object1Position = objects[i].transform.position;
                Vector3 object2Position = objects[i + 1].transform.position;
                
                result.Add(DistanceBoxBetweenObjects(GenerateBox(), parent, object1Position, object2Position, boxWidth, boxHeight, position));
            }

            if (isClose && objects.Count >= 2)
            {
                Vector3 firstObjectPosition = objects[0].transform.position;
                Vector3 lastObjectPosition = objects[objects.Count - 1].transform.position;
                
                result.Add(DistanceBoxBetweenObjects(GenerateBox(), parent, lastObjectPosition, firstObjectPosition, boxWidth, boxHeight, position));
            }

            if (!isClose && objects.Count >= 3)
            {
                for (int i = 1; i < objects.Count - 1; i++)
                {
                    Vector3 object1Position = objects[i - 1].transform.position;
                    Vector3 object2Position = objects[i].transform.position;
                    Vector3 object3Position = objects[i + 1].transform.position;

                    result.Add(GenerateCornerBox(GenerateBox(), parent, object2Position, object1Position, object3Position, boxWidth, boxHeight, position));
                }
            }

            return result;
        }

        private static GameObject GenerateBox()
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            return box;
        }

        public static GameObject DistanceBoxBetweenObjects(GameObject box, GameObject parent, Vector3 position1, Vector3 position2, float boxWidth, float boxHeight, AbstractPosition position)
        {
            float size = Vector3.Distance(position1, position2);
            
            box.transform.position = (position1 + position2) / 2;

            box.transform.LookAt(position2);
            box.transform.localScale = new Vector3(boxWidth, boxHeight, size);

            // ボックスの位置を調整
            switch (position)
            {
                case AbstractPosition.Inside:
                    box.transform.position = position1 + (box.transform.forward * (size / 2.0f)) + (box.transform.up * (boxHeight / 2.0f)) - (box.transform.right * (boxWidth / 2.0f));
                    break;

                case AbstractPosition.Center:
                    box.transform.position = position1 + (box.transform.forward * (size / 2.0f)) + (box.transform.up * (boxHeight / 2.0f));
                    break;

                case AbstractPosition.Outside:
                    box.transform.position = position1 + (box.transform.forward * (size / 2.0f)) + (box.transform.up * (boxHeight / 2.0f)) + (box.transform.right * (boxWidth / 2.0f));
                    break;

                default:
                    break;
            }

            Undo.RegisterCreatedObjectUndo(box, "Generate Box Collider");

            if (parent != null)
            {
                box.transform.parent = parent.transform;
            }

            return box;
        }

        static GameObject GenerateCornerBox(GameObject box, GameObject parent, Vector3 vec1, Vector3 vec2, Vector3 vec3, float boxWidth, float boxHeight, AbstractPosition position)
        {
            Vector3 vecA = (vec2 - vec1).normalized;
            Vector3 vecB = (vec3 - vec1).normalized;
            float angle = Vector3.Angle(vecA, vecB) * Mathf.Deg2Rad / 2;
            float length = CalculateAdjacentSideLength(boxWidth / 2, angle);
            Vector3 middleVector = (vecA + vecB) / 2.0f;

            box.transform.position = vec1;
            box.transform.localScale = new Vector3(boxWidth, boxWidth, length * 2);
            box.transform.LookAt(box.transform.position + middleVector);

            if (parent != null)
            {
                box.transform.parent = parent.transform;
            }

            return box;
        }

        static float CalculateAdjacentSideLength(float oppositeSideLength, float thetaInRadians)
        {
            float adjacentSideLength = oppositeSideLength / Mathf.Tan(thetaInRadians);

            return adjacentSideLength;
        }
    }
}
