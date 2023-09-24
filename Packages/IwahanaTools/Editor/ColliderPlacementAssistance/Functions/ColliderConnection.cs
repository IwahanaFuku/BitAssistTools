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

            // 生成されたボックスを親オブジェクトの子として設定
            if (parent != null)
            {
                box.transform.parent = parent.transform;
            }

            return box;
        }
    }
}
