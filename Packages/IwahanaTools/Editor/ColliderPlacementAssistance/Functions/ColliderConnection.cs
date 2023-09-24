using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ColliderPlacemenAssistancer
{
    public static class BoxColliderPlacementer
    {
        public static void GenerateBoxesBetweenObjects(List<GameObject> objects, bool isClose, float boxWidth, float boxHeight, AbstractPosition position, GameObject parent)
        {
            for (int i = 0; i < objects.Count - 1; i++)
            {
                Vector3 object1Position = objects[i].transform.position;
                Vector3 object2Position = objects[i + 1].transform.position;

                GenerateDistanceBoxBetweenObjects(object1Position, object2Position, boxWidth, boxHeight, position, parent);
            }

            if (isClose && objects.Count >= 2)
            {
                Vector3 firstObjectPosition = objects[0].transform.position;
                Vector3 lastObjectPosition = objects[objects.Count - 1].transform.position;
                GenerateDistanceBoxBetweenObjects(lastObjectPosition, firstObjectPosition, boxWidth, boxHeight, position, parent);
            }
        }

        private static void GenerateDistanceBoxBetweenObjects(Vector3 position1, Vector3 position2, float boxWidth, float boxHeight, AbstractPosition position, GameObject parent)
        {
            float size = Vector3.Distance(position1, position2);
            
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.position = (position1 + position2) / 2;

            box.transform.LookAt(position2);
            box.transform.localScale = new Vector3(boxWidth, boxHeight, size);

            // ボックスの位置を調整
            switch (position)
            {
                case AbstractPosition.Inside:
                    // 内側に移動するための計算ロジックを実装
                    box.transform.position = position1 + (box.transform.forward * (size / 2.0f)) - (box.transform.up * (boxHeight / 2.0f)) + (box.transform.right * (boxWidth / 2.0f));
                    break;

                case AbstractPosition.Center:
                    // 中心に移動するための計算ロジックを実装
                    break;

                case AbstractPosition.Outside:
                    // 外側に移動するための計算ロジックを実装
                    box.transform.position = position1 + (box.transform.forward * (size / 2.0f)) - (box.transform.up * (boxHeight / 2.0f)) - (box.transform.right * (boxWidth / 2.0f));
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
        }
    }
}