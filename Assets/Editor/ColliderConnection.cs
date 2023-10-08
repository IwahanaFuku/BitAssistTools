using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ColliderPlacemenAssistancer
{
    public static class BoxColliderPlacementer
    {
        public static List<GameObject> GenerateBoxesBetweenObjects(List<GameObject> objects, GameObject parent, bool isClose, float boxWidth, float boxHeight, AbstractPosition position)
        {
            List<GameObject> result = new List<GameObject>();

            //辺の生成
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

            //角の生成
            if (objects.Count >= 3)
            {
                for (int i = 1; i < objects.Count - 1; i++)
                {
                    Vector3 object1Position = objects[i - 1].transform.position;
                    Vector3 object2Position = objects[i].transform.position;
                    Vector3 object3Position = objects[i + 1].transform.position;

                    result.Add(GenerateCornerBox(GenerateBox(), parent, object2Position, object1Position, object3Position, boxWidth, boxHeight, position));
                }
            }

            if (isClose && objects.Count >= 3)
            {
                //始点
                Vector3 object1Position = objects[objects.Count - 1].transform.position;
                Vector3 object2Position = objects[0].transform.position;
                Vector3 object3Position = objects[1].transform.position;

                result.Add(GenerateCornerBox(GenerateBox(), parent, object2Position, object1Position, object3Position, boxWidth, boxHeight, position));

                //終点
                object1Position = objects[objects.Count - 2].transform.position;
                object2Position = objects[objects.Count - 1].transform.position;
                object3Position = objects[0].transform.position;

                result.Add(GenerateCornerBox(GenerateBox(), parent, object2Position, object1Position, object3Position, boxWidth, boxHeight, position));
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

            box.transform.position = position1 + (box.transform.forward * (size / 2.0f)) + (box.transform.up * (boxHeight / 2.0f));

            switch (position)
            {
                case AbstractPosition.Leftside:
                    box.transform.position -= box.transform.right * (boxWidth / 2.0f);
                    break;

                case AbstractPosition.Rightside:
                    box.transform.position += box.transform.right * (boxWidth / 2.0f);
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
            box.transform.localScale = new Vector3(boxWidth, boxHeight, length);
            box.transform.LookAt(box.transform.position + middleVector);

            box.transform.position += box.transform.up * (boxHeight / 2.0f) + box.transform.forward * length / 2;

            float hypotenuse = boxWidth / Mathf.Sin(angle);
            
            if(! IsVector3COnRight(vec2, vec1, vec3))
            {
                
                switch (position)
                {
                    case AbstractPosition.Leftside:
                        
                        break;

                    case AbstractPosition.Center:
                        box.transform.position += box.transform.forward * hypotenuse / 2;
                        break;

                    case AbstractPosition.Rightside:
                    box.transform.position += box.transform.forward * hypotenuse;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (position)
                {
                    case AbstractPosition.Leftside:
                        box.transform.position += box.transform.forward * hypotenuse;
                        
                        break;

                    case AbstractPosition.Center:
                        box.transform.position += box.transform.forward * hypotenuse / 2;
                        break;

                    case AbstractPosition.Rightside:
                        break;

                    default:
                        break;
                }
            }
            return box; 
        }

        static float CalculateAdjacentSideLength(float oppositeSideLength, float thetaInRadians)
        {
            // 隣接辺の長さを計算
            float adjacentSideLength = oppositeSideLength / Mathf.Tan(thetaInRadians);

            return adjacentSideLength;
        }

        static bool IsVector3COnRight(Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            //基準点からの方向ベクトルを求める
            Vector3 ab = vec2 - vec1;
            //acベクトルの右方向ベクトルを求める
            Vector3 acRight = Vector3.Cross(vec3 - vec1, new Vector3(0, 1, 0));
            //内積を求める
            float cross = Vector3.Dot(new Vector3(ab.x, 0, ab.z).normalized, new Vector3(acRight.x, 0, acRight.z).normalized);

            return cross < 0;
        }
    }
}
