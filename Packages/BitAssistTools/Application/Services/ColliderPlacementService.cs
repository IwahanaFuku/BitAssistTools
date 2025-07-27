using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Application.ViewModels;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// 抽象的な位置を表現する列挙型
    /// ColliderPlacement機能で使用
    /// </summary>
    public enum AbstractPosition
    {
        /// <summary>左揃え</summary>
        Leftside,
        /// <summary>中心</summary>
        Center,
        /// <summary>右揃え</summary>
        Rightside
    }

    /// <summary>
    /// ColliderPlacement機能のアプリケーションサービス
    /// ViewModelとドメインモデルを橋渡し
    /// </summary>
    public class ColliderPlacementService
    {
        /// <summary>
        /// 2点間の距離と方向を計算
        /// </summary>
        public static (float distance, Vector3 direction, Vector3 center) CalculateConnectionData(Vector3 fromPos, Vector3 toPos)
        {
            Vector3 direction = toPos - fromPos;
            float distance = direction.magnitude;
            Vector3 center = (fromPos + toPos) * 0.5f;
            
            return (distance, direction.normalized, center);
        }

        /// <summary>
        /// エッジコライダーのGameObjectを生成（旧実装互換）
        /// </summary>
        public static List<GameObject> GenerateEdgeColliders(List<GameObject> points, GameObject parent, bool isClosed, float edgeWidth, float boxHeight, AbstractPosition position)
        {
            List<GameObject> result = new List<GameObject>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 pos1 = points[i].transform.position;
                Vector3 pos2 = points[i + 1].transform.position;
                
                result.Add(CreateEdgeCollider(pos1, pos2, parent, edgeWidth, boxHeight, position, i));
            }

            if (isClosed && points.Count >= 2)
            {
                Vector3 firstPos = points[0].transform.position;
                Vector3 lastPos = points[points.Count - 1].transform.position;
                
                result.Add(CreateEdgeCollider(lastPos, firstPos, parent, edgeWidth, boxHeight, position, points.Count - 1));
            }

            return result;
        }

        /// <summary>
        /// コーナーコライダーのGameObjectを生成（旧実装互換）
        /// </summary>
        public static List<GameObject> GenerateCornerColliders(List<GameObject> points, GameObject parent, bool isClosed, float cornerWidth, float edgeWidth, float boxHeight, AbstractPosition position)
        {
            List<GameObject> result = new List<GameObject>();

            if (points.Count >= 3)
            {
                for (int i = 1; i < points.Count - 1; i++)
                {
                    Vector3 pos1 = points[i - 1].transform.position;
                    Vector3 pos2 = points[i].transform.position;
                    Vector3 pos3 = points[i + 1].transform.position;

                    result.Add(CreateCornerCollider(pos1, pos2, pos3, parent, cornerWidth, edgeWidth, boxHeight, position, i));
                }
            }

            if (isClosed && points.Count >= 3)
            {
                // 始点
                Vector3 pos1 = points[points.Count - 1].transform.position;
                Vector3 pos2 = points[0].transform.position;
                Vector3 pos3 = points[1].transform.position;

                result.Insert(0, CreateCornerCollider(pos1, pos2, pos3, parent, cornerWidth, edgeWidth, boxHeight, position, 0));

                // 終点
                pos1 = points[points.Count - 2].transform.position;
                pos2 = points[points.Count - 1].transform.position;
                pos3 = points[0].transform.position;

                result.Add(CreateCornerCollider(pos1, pos2, pos3, parent, cornerWidth, edgeWidth, boxHeight, position, points.Count - 1));
            }
            
            return result;
        }

        /// <summary>
        /// エッジコライダーを作成
        /// </summary>
        private static GameObject CreateEdgeCollider(Vector3 position1, Vector3 position2, GameObject parent, float edgeWidth, float boxHeight, AbstractPosition position, int index)
        {
            var box = CreateBoxGameObject();
            float size = Vector3.Distance(position1, position2);
            
            box.transform.position = (position1 + position2) / 2;
            box.transform.LookAt(position2);
            box.transform.localScale = new Vector3(edgeWidth, boxHeight, size);

            box.transform.position = position1 + (box.transform.forward * (size / 2.0f)) + (box.transform.up * (boxHeight / 2.0f));

            switch (position)
            {
                case AbstractPosition.Leftside:
                    box.transform.position -= box.transform.right * (edgeWidth / 2.0f);
                    break;
                case AbstractPosition.Rightside:
                    box.transform.position += box.transform.right * (edgeWidth / 2.0f);
                    break;
                default:
                    break;
            }

            if (parent != null)
            {
                Undo.SetTransformParent(box.transform, parent.transform, "Set Edge Parent");
            }

            box.name = $"Edge_{index}";
            return box;
        }

        /// <summary>
        /// コーナーコライダーを作成
        /// </summary>
        private static GameObject CreateCornerCollider(Vector3 beforeVec, Vector3 mainVec, Vector3 afterVec, GameObject parent, float cornerWidth, float edgeWidth, float boxHeight, AbstractPosition position, int index)
        {
            var box = CreateBoxGameObject();

            Vector3 vecA = (beforeVec - mainVec).normalized;
            Vector3 vecB = (afterVec - mainVec).normalized;
            float angle = Vector3.Angle(vecA, vecB) * Mathf.Deg2Rad / 2;
            float length = CalculateAdjacentSideLength(cornerWidth / 2, angle);
            Vector3 middleVector = (vecA + vecB) / 2.0f;

            box.transform.position = mainVec;

            if (length == Mathf.Infinity)
            {
                length = 0;
            }

            box.transform.localScale = new Vector3(cornerWidth, boxHeight, length);
            box.transform.LookAt(box.transform.position + middleVector);

            box.transform.position += box.transform.up * (boxHeight / 2.0f) + box.transform.forward * length / 2;

            float hypotenuse = edgeWidth / Mathf.Sin(angle);

            if (beforeVec == mainVec || mainVec == afterVec || beforeVec == afterVec)
            {
                if (parent != null)
                {
                    box.transform.SetParent(parent.transform);
                }
                box.name = $"Corner_{index}";
                return box;
            }

            if (!IsVector3COnRight(beforeVec, mainVec, afterVec))
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

            if (parent != null)
            {
                Undo.SetTransformParent(box.transform, parent.transform, "Set Corner Parent");
            }

            box.name = $"Corner_{index}";
            return box;
        }

        /// <summary>
        /// ボックスGameObjectを作成（Undo対応）
        /// </summary>
        private static GameObject CreateBoxGameObject()
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(box, "Create Box Collider");
            return box;
        }

        /// <summary>
        /// 隣接辺の長さを計算
        /// </summary>
        public static float CalculateAdjacentSideLength(float oppositeSideLength, float thetaInRadians)
        {
            return oppositeSideLength / Mathf.Tan(thetaInRadians);
        }

        /// <summary>
        /// ベクトルが右側にあるかを判定
        /// </summary>
        public static bool IsVector3COnRight(Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            Vector3 ab = vec2 - vec1;
            Vector3 acRight = Vector3.Cross(vec3 - vec1, new Vector3(0, 1, 0));
            float cross = Vector3.Dot(new Vector3(ab.x, 0, ab.z).normalized, new Vector3(acRight.x, 0, acRight.z).normalized);

            return cross < 0;
        }

        /// <summary>
        /// コーナーオブジェクトのTransformDataを計算
        /// </summary>
        public static TransformData CalculateCornerTransform(Vector3 position, float size)
        {
            return new TransformData(
                position, 
                Quaternion.identity, 
                Vector3.one * size
            );
        }

        /// <summary>
        /// ポイント間の接続経路を計算
        /// </summary>
        public static List<(int fromIndex, int toIndex)> CalculateConnectionPath(int pointCount, bool isClosed)
        {
            var connections = new List<(int, int)>();
            
            if (pointCount < 2)
                return connections;
            
            // 連続する点を接続
            for (int i = 0; i < pointCount - 1; i++)
            {
                connections.Add((i, i + 1));
            }
            
            // 閉じる場合は最後と最初を接続
            if (isClosed && pointCount > 2)
            {
                connections.Add((pointCount - 1, 0));
            }
            
            return connections;
        }

        /// <summary>
        /// 回転分割に基づいた角度リストを計算
        /// </summary>
        public static List<float> CalculateRotationAngles(float totalDegrees, int divisions)
        {
            var angles = new List<float>();
            
            if (divisions <= 0)
                return angles;
            
            float angleStep = totalDegrees / divisions;
            
            for (int i = 0; i < divisions; i++)
            {
                angles.Add(i * angleStep);
            }
            
            return angles;
        }

        /// <summary>
        /// BoxColliderをGameObjectに適用
        /// </summary>
        public static GameObject CreateBoxColliderGameObject(
            string name, 
            TransformData transformData, 
            Transform parent = null)
        {
            var gameObject = new GameObject(name);
            
            // Transform設定
            gameObject.transform.position = transformData.Position;
            gameObject.transform.rotation = transformData.Rotation;
            gameObject.transform.localScale = transformData.LocalScale;
            
            // 親設定
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            
            // BoxCollider追加
            var boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one; // localScaleで調整されるため
            
            return gameObject;
        }

        /// <summary>
        /// 既存のGameObjectリストをクリーンアップ
        /// </summary>
        public static void CleanupGameObjects(List<GameObject> gameObjects)
        {
            if (gameObjects == null) return;
            
            foreach (var obj in gameObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            
            gameObjects.Clear();
        }

        /// <summary>
        /// 親オブジェクトの子をリストに追加
        /// </summary>
        public static List<GameObject> GetChildrenGameObjects(GameObject parent)
        {
            var children = new List<GameObject>();
            
            if (parent != null)
            {
                foreach (Transform child in parent.transform)
                {
                    children.Add(child.gameObject);
                }
            }
            
            return children;
        }

        /// <summary>
        /// ポイントの有効性を検証
        /// </summary>
        public static bool ValidatePoints(List<PointObjectInfo> points, out string errorMessage)
        {
            errorMessage = null;
            
            if (points == null || points.Count == 0)
            {
                errorMessage = "ポイントリストが空です";
                return false;
            }
            
            if (points.Count < 2)
            {
                errorMessage = "最低2個のポイントが必要です";
                return false;
            }
            
            int validPointCount = 0;
            foreach (var point in points)
            {
                if (point?.GameObject != null)
                {
                    validPointCount++;
                }
            }
            
            if (validPointCount < 2)
            {
                errorMessage = "有効なポイントが2個未満です";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// デフォルトの親オブジェクトを作成
        /// </summary>
        public static GameObject CreateDefaultParent(string name)
        {
            var parent = new GameObject(name);
            return parent;
        }

        // --- Rotational Duplication Logic (from RotateDuplicater.cs) ---

        public static void RotateDuplicate(GameObject selectedObject, float rotationalFrequency)
        {
            if (selectedObject == null) return;

            GameObject[] boundsObjects = GetBoundsObjects(selectedObject);
            Vector3 boundsObjectsCenterPosition = CalculateCenterPosition(boundsObjects);
            Bounds updateBoundingBox = GetUpdateBoundingBox(boundsObjects);
            int numOfCopy = ReturnNumOfCopy(selectedObject, updateBoundingBox, boundsObjectsCenterPosition, rotationalFrequency);
            float theta = CalculateRotationalFrequencyPerCopy(numOfCopy, rotationalFrequency);
            CopyTransforms(selectedObject, boundsObjects, numOfCopy, theta);
        }

        private static GameObject[] GetBoundsObjects(GameObject selectedObject)
        {
            List<GameObject> boundsObjectsList = new List<GameObject>();

            if (selectedObject.transform.childCount == 0)
            {
                boundsObjectsList.Add(selectedObject);
            }
            else
            {
                foreach (Transform childTransform in selectedObject.transform)
                {
                    boundsObjectsList.Add(childTransform.gameObject);
                }
            }

            return boundsObjectsList.ToArray();
        }

        private static Vector3 CalculateCenterPosition(GameObject[] gameObjects)
        {
            Vector3 centerPosition = Vector3.zero;

            foreach (GameObject obj in gameObjects)
            {
                if (obj != null)
                {
                    centerPosition += obj.transform.position;
                }
            }

            centerPosition /= gameObjects.Length;

            return centerPosition;
        }

        private static Bounds GetUpdateBoundingBox(GameObject[] boundsObjects)
        {
            Bounds bounds = new Bounds();
            bool hasBounds = false;

            foreach (GameObject boundsObject in boundsObjects)
            {
                Renderer renderer = boundsObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }
            return bounds;
        }

        private static int ReturnNumOfCopy(GameObject selectedObject, Bounds bounds, Vector3 centerPosition, float rotationalFrequency)
        {
            var selectedPosition = selectedObject.transform.position;

            Vector3 edgeCenterX = new Vector3(ExtendValues(centerPosition.x - selectedPosition.x, bounds.size.x / 2), centerPosition.y, centerPosition.z - selectedPosition.z);
            Vector3 edgeCenterZ = new Vector3(centerPosition.x - selectedPosition.x, centerPosition.y, ExtendValues(centerPosition.z - selectedPosition.z, bounds.size.z / 2));

            float theta;
            int numOfCopy;

            theta = Mathf.Atan2(edgeCenterX.x, edgeCenterZ.z) * Mathf.Rad2Deg * 2;
            float modifyAngle = ModifyAngle(FlipAngle(NormalizeAngle(theta)));
            numOfCopy = Convert.ToInt32(rotationalFrequency / modifyAngle);

            return numOfCopy;
        }
        private static float NormalizeAngle(float angle)
        {
            if (angle < 0)
            {
                return Mathf.Abs(angle);
            }
            else
            {
                return angle;
            }
        }
        private static float FlipAngle(float angle)
        {
            if ((angle > 45 && angle <= 90) || (angle > 225 && angle <= 270))
            {
                return 180 - angle;
            }
            else if (angle > 135 && angle <= 225 || (angle > 315 && angle <= 360))
            {
                return 360 - angle;
            }
            else
            {
                return angle;
            }
        }

        private static float ModifyAngle(float angle)
        {
            if (angle > 90 && angle <= 180)
            {
                return angle - 90;
            }
            else if (angle > 180 && angle <= 270)
            {
                return angle - 180;
            }
            else if (angle > 270 && angle <= 360)
            {
                return angle - 270;
            }
            else
            {
                return angle;
            }
        }

        private static float ExtendValues(float firstValue, float secondValue)
        {
            return (firstValue < 0) ? firstValue - secondValue : firstValue + secondValue;
        }

        private static float CalculateRotationalFrequencyPerCopy(int numOfCopy, float rotationalFrequency)
        {
            if (numOfCopy == 0) return 0; // ゼロ除算を避ける
            float theta = rotationalFrequency / numOfCopy;
            return theta;
        }

        private static void CopyTransforms(GameObject selectedObject, GameObject[] boundsObjects, int numOfCopy, float theta)
        {
            Vector3 rotationAxis = Vector3.up;
            Undo.RecordObjects(boundsObjects, "Rotate Duplicate");

            var copyTransformsList = new List<Transform>();
            copyTransformsList.Add(selectedObject.transform);

            for (int i = 0; i < numOfCopy + 1; i++)
            {
                Quaternion rotationQuaternion = Quaternion.AngleAxis(theta * i, rotationAxis);
                GameObject newObject = UnityEngine.Object.Instantiate(selectedObject, selectedObject.transform.position, selectedObject.transform.rotation * rotationQuaternion);
                newObject.name = selectedObject.name;
                copyTransformsList.Add(newObject.transform);

                Undo.RegisterCreatedObjectUndo(newObject, "Rotate Duplicate");
            }

            Transform[] copyTransforms = copyTransformsList.ToArray();
            ParentCenterAligner(copyTransforms);
        }

        // --- Parent Center Aligner Logic (from CommonFunctions.cs) ---

        public static void ParentCenterAligner(Transform[] selectedTransforms)
        {
            if (selectedTransforms == null || selectedTransforms.Length == 0) return;

            int firstIndex = selectedTransforms.Min(t => t.GetSiblingIndex());

            Vector3 center = Vector3.zero;
            foreach (Transform transform in selectedTransforms)
            {
                center += transform.position;
            }
            center /= selectedTransforms.Length;

            var parent = selectedTransforms[0].parent; // 最初のオブジェクトの親を基準とする
            if (parent != null)
            {
                center = parent.InverseTransformPoint(center);
            }

            var group = new GameObject("Group");
            group.transform.position = center;
            Undo.RegisterCreatedObjectUndo(group, "Group Selected");
            group.transform.SetSiblingIndex(firstIndex);

            group.transform.SetParent(parent, false);
            foreach (var transform in selectedTransforms.OrderBy(t => t.GetSiblingIndex()))
            {
                Undo.SetTransformParent(transform, group.transform, "Group Selected");
            }
            Selection.activeGameObject = group;
        }
    }
}