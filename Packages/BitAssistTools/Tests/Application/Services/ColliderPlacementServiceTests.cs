using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.Services;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Domain.Models.Transform;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class ColliderPlacementServiceTests
    {
        private GameObject _point1;
        private GameObject _point2;
        private GameObject _point3;
        private GameObject _parentObject;
        private List<GameObject> _createdObjects;

        [SetUp]
        public void SetUp()
        {
            // テスト用のポイントオブジェクトを作成
            _point1 = new GameObject("Point1");
            _point1.transform.position = Vector3.zero;

            _point2 = new GameObject("Point2");
            _point2.transform.position = new Vector3(5, 0, 0);

            _point3 = new GameObject("Point3");
            _point3.transform.position = new Vector3(5, 0, 5);

            _parentObject = new GameObject("Parent");
            _createdObjects = new List<GameObject>();
        }

        [TearDown]
        public void TearDown()
        {
            // 全ての "Group" オブジェクトを削除
            var allGroupObjects = GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name.Contains("Group"))
                .ToArray();
            foreach (var group in allGroupObjects)
            {
                UnityEngine.Object.DestroyImmediate(group);
            }

            // テスト用オブジェクトを削除
            if (_point1 != null) UnityEngine.Object.DestroyImmediate(_point1);
            if (_point2 != null) UnityEngine.Object.DestroyImmediate(_point2);
            if (_point3 != null) UnityEngine.Object.DestroyImmediate(_point3);
            if (_parentObject != null) UnityEngine.Object.DestroyImmediate(_parentObject);

            // 作成されたオブジェクトを削除
            foreach (var obj in _createdObjects)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            _createdObjects.Clear();

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void CalculateConnectionData_WithValidPoints_ReturnsCorrectData()
        {
            // Arrange
            Vector3 from = Vector3.zero;
            Vector3 to = new Vector3(3, 4, 0);

            // Act
            var (distance, direction, center) = ColliderPlacementService.CalculateConnectionData(from, to);

            // Assert
            Assert.AreEqual(5.0f, distance, 0.001f); // 3-4-5 triangle
            Assert.AreEqual(new Vector3(0.6f, 0.8f, 0f), direction, "Direction should be normalized");
            Assert.AreEqual(new Vector3(1.5f, 2f, 0f), center, "Center should be midpoint");
        }

        [Test]
        public void CalculateConnectionData_WithSamePoints_ReturnsZeroDistance()
        {
            // Arrange
            Vector3 samePoint = new Vector3(1, 2, 3);

            // Act
            var (distance, direction, center) = ColliderPlacementService.CalculateConnectionData(samePoint, samePoint);

            // Assert
            Assert.AreEqual(0.0f, distance, 0.001f);
            Assert.AreEqual(samePoint, center);
        }

        [Test]
        public void GenerateEdgeColliders_WithValidPoints_CreatesCorrectNumberOfEdges()
        {
            // Arrange
            var points = new List<GameObject> { _point1, _point2, _point3 };
            bool isClosed = false;

            // Act
            var edgeColliders = ColliderPlacementService.GenerateEdgeColliders(
                points, _parentObject, isClosed, 1.0f, 2.0f, AbstractPosition.Center);

            // Assert
            Assert.AreEqual(2, edgeColliders.Count); // 3点で非閉鎖 = 2エッジ
            _createdObjects.AddRange(edgeColliders);

            // 名前の確認
            Assert.AreEqual("Edge_0", edgeColliders[0].name);
            Assert.AreEqual("Edge_1", edgeColliders[1].name);

            // 親設定の確認
            Assert.AreEqual(_parentObject.transform, edgeColliders[0].transform.parent);
            Assert.AreEqual(_parentObject.transform, edgeColliders[1].transform.parent);
        }

        [Test]
        public void GenerateEdgeColliders_WithClosedPath_CreatesAdditionalEdge()
        {
            // Arrange
            var points = new List<GameObject> { _point1, _point2, _point3 };
            bool isClosed = true;

            // Act
            var edgeColliders = ColliderPlacementService.GenerateEdgeColliders(
                points, _parentObject, isClosed, 1.0f, 2.0f, AbstractPosition.Center);

            // Assert
            Assert.AreEqual(3, edgeColliders.Count); // 3点で閉鎖 = 3エッジ
            _createdObjects.AddRange(edgeColliders);
        }

        [Test]
        public void GenerateCornerColliders_WithValidPoints_CreatesCorrectNumberOfCorners()
        {
            // Arrange
            var points = new List<GameObject> { _point1, _point2, _point3 };
            bool isClosed = false;

            // Act
            var cornerColliders = ColliderPlacementService.GenerateCornerColliders(
                points, _parentObject, isClosed, 1.0f, 0.5f, 2.0f, AbstractPosition.Center);

            // Assert
            Assert.AreEqual(1, cornerColliders.Count); // 3点で非閉鎖 = 1コーナー（中央のみ）
            _createdObjects.AddRange(cornerColliders);

            // 名前の確認
            Assert.AreEqual("Corner_1", cornerColliders[0].name);

            // 親設定の確認
            Assert.AreEqual(_parentObject.transform, cornerColliders[0].transform.parent);
        }

        [Test]
        public void GenerateCornerColliders_WithClosedPath_CreatesAdditionalCorners()
        {
            // Arrange
            var points = new List<GameObject> { _point1, _point2, _point3 };
            bool isClosed = true;

            // Act
            var cornerColliders = ColliderPlacementService.GenerateCornerColliders(
                points, _parentObject, isClosed, 1.0f, 0.5f, 2.0f, AbstractPosition.Center);

            // Assert
            Assert.AreEqual(3, cornerColliders.Count); // 3点で閉鎖 = 3コーナー
            _createdObjects.AddRange(cornerColliders);
        }

        [Test]
        public void CalculateAdjacentSideLength_WithValidValues_ReturnsCorrectLength()
        {
            // Arrange
            float opposite = 5.0f;
            float angle = Mathf.PI / 4; // 45度

            // Act
            float adjacent = ColliderPlacementService.CalculateAdjacentSideLength(opposite, angle);

            // Assert
            Assert.AreEqual(5.0f, adjacent, 0.001f); // tan(45°) = 1, so adjacent = opposite
        }

        [Test]
        public void IsVector3COnRight_WithRightSideVector_ReturnsTrue()
        {
            // Arrange
            Vector3 vec1 = Vector3.zero;
            Vector3 vec2 = Vector3.forward;
            Vector3 vec3 = Vector3.right; // 右側

            // Act
            bool isOnRight = ColliderPlacementService.IsVector3COnRight(vec1, vec2, vec3);

            // Assert
            Assert.IsTrue(isOnRight); // 右側なので true
        }

        [Test]
        public void IsVector3COnRight_WithLeftSideVector_ReturnsFalse()
        {
            // Arrange
            Vector3 vec1 = Vector3.zero;
            Vector3 vec2 = Vector3.forward;
            Vector3 vec3 = Vector3.left; // 左側

            // Act
            bool isOnRight = ColliderPlacementService.IsVector3COnRight(vec1, vec2, vec3);

            // Assert
            Assert.IsFalse(isOnRight); // 左側なので false
        }

        [Test]
        public void IsVector3COnRight_WithDiagonalVectors_ReturnsExpectedResult()
        {
            // Arrange - より明確な左右判定のテスト
            Vector3 vec1 = Vector3.zero;
            Vector3 vec2 = new Vector3(1, 0, 1); // 右前方向
            Vector3 vec3 = new Vector3(-1, 0, 1); // 左前方向（vec2の基準で左側）

            // Act
            bool isOnRight = ColliderPlacementService.IsVector3COnRight(vec1, vec2, vec3);

            // Assert  
            // vec2 = (1,0,1), vec3 = (-1,0,1) の場合
            // ab = (1,0,1), ac = (-1,0,1)
            // cross = ab.x * ac.z - ab.z * ac.x = 1 * 1 - 1 * (-1) = 1 + 1 = 2 > 0
            // 正の値なので左側 = false
            Assert.IsFalse(isOnRight);
        }

        [Test]
        public void CalculateCornerTransform_WithValidValues_ReturnsCorrectTransform()
        {
            // Arrange
            Vector3 position = new Vector3(1, 2, 3);
            float size = 2.5f;

            // Act
            var transformData = ColliderPlacementService.CalculateCornerTransform(position, size);

            // Assert
            Assert.AreEqual(position, transformData.Position);
            Assert.AreEqual(Quaternion.identity, transformData.Rotation);
            Assert.AreEqual(Vector3.one * size, transformData.LocalScale);
        }

        [Test]
        public void CalculateConnectionPath_WithOpenPath_ReturnsCorrectConnections()
        {
            // Arrange
            int pointCount = 4;
            bool isClosed = false;

            // Act
            var connections = ColliderPlacementService.CalculateConnectionPath(pointCount, isClosed);

            // Assert
            Assert.AreEqual(3, connections.Count); // 4点で非閉鎖 = 3接続
            Assert.AreEqual((0, 1), connections[0]);
            Assert.AreEqual((1, 2), connections[1]);
            Assert.AreEqual((2, 3), connections[2]);
        }

        [Test]
        public void CalculateConnectionPath_WithClosedPath_ReturnsAdditionalConnection()
        {
            // Arrange
            int pointCount = 4;
            bool isClosed = true;

            // Act
            var connections = ColliderPlacementService.CalculateConnectionPath(pointCount, isClosed);

            // Assert
            Assert.AreEqual(4, connections.Count); // 4点で閉鎖 = 4接続
            Assert.AreEqual((3, 0), connections[3]); // 最後と最初を接続
        }

        [Test]
        public void CalculateConnectionPath_WithInsufficientPoints_ReturnsEmptyList()
        {
            // Arrange
            int pointCount = 1;
            bool isClosed = false;

            // Act
            var connections = ColliderPlacementService.CalculateConnectionPath(pointCount, isClosed);

            // Assert
            Assert.AreEqual(0, connections.Count);
        }

        [Test]
        public void CalculateRotationAngles_WithValidValues_ReturnsCorrectAngles()
        {
            // Arrange
            float totalDegrees = 360f;
            int divisions = 4;

            // Act
            var angles = ColliderPlacementService.CalculateRotationAngles(totalDegrees, divisions);

            // Assert
            Assert.AreEqual(4, angles.Count);
            Assert.AreEqual(0f, angles[0], 0.001f);
            Assert.AreEqual(90f, angles[1], 0.001f);
            Assert.AreEqual(180f, angles[2], 0.001f);
            Assert.AreEqual(270f, angles[3], 0.001f);
        }

        [Test]
        public void CalculateRotationAngles_WithZeroDivisions_ReturnsEmptyList()
        {
            // Arrange
            float totalDegrees = 360f;
            int divisions = 0;

            // Act
            var angles = ColliderPlacementService.CalculateRotationAngles(totalDegrees, divisions);

            // Assert
            Assert.AreEqual(0, angles.Count);
        }

        [Test]
        public void CreateBoxColliderGameObject_WithValidData_CreatesCorrectObject()
        {
            // Arrange
            string name = "TestBox";
            var transformData = new TransformData(
                new Vector3(1, 2, 3),
                Quaternion.Euler(0, 45, 0),
                new Vector3(2, 3, 4)
            );

            // Act
            var gameObject = ColliderPlacementService.CreateBoxColliderGameObject(
                name, transformData, _parentObject.transform);

            _createdObjects.Add(gameObject);

            // Assert
            Assert.AreEqual(name, gameObject.name);
            Assert.AreEqual(transformData.Position, gameObject.transform.position);
            Assert.AreEqual(transformData.Rotation, gameObject.transform.rotation);
            Assert.AreEqual(transformData.LocalScale, gameObject.transform.localScale);
            Assert.AreEqual(_parentObject.transform, gameObject.transform.parent);

            var boxCollider = gameObject.GetComponent<BoxCollider>();
            Assert.IsNotNull(boxCollider);
            Assert.AreEqual(Vector3.one, boxCollider.size);
        }

        [Test]
        public void CreateBoxColliderGameObject_WithoutParent_CreatesObjectWithoutParent()
        {
            // Arrange
            string name = "TestBox";
            var transformData = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);

            // Act
            var gameObject = ColliderPlacementService.CreateBoxColliderGameObject(name, transformData);

            _createdObjects.Add(gameObject);

            // Assert
            Assert.IsNull(gameObject.transform.parent);
        }

        [Test]
        public void CleanupGameObjects_WithValidList_RemovesAllObjects()
        {
            // Arrange
            var objectsToCleanup = new List<GameObject>
            {
                new GameObject("Test1"),
                new GameObject("Test2"),
                new GameObject("Test3")
            };

            // Act
            ColliderPlacementService.CleanupGameObjects(objectsToCleanup);

            // Assert
            Assert.AreEqual(0, objectsToCleanup.Count);
        }

        [Test]
        public void CleanupGameObjects_WithNullList_DoesNotThrow()
        {
            // Arrange
            List<GameObject> nullList = null;

            // Act & Assert
            Assert.DoesNotThrow(() => ColliderPlacementService.CleanupGameObjects(nullList));
        }

        [Test]
        public void GetChildrenGameObjects_WithParentHavingChildren_ReturnsAllChildren()
        {
            // Arrange
            var child1 = new GameObject("Child1");
            var child2 = new GameObject("Child2");
            child1.transform.SetParent(_parentObject.transform);
            child2.transform.SetParent(_parentObject.transform);

            try
            {
                // Act
                var children = ColliderPlacementService.GetChildrenGameObjects(_parentObject);

                // Assert
                Assert.AreEqual(2, children.Count);
                Assert.IsTrue(children.Contains(child1));
                Assert.IsTrue(children.Contains(child2));
            }
            finally
            {
                // Cleanup
                if (child1 != null) UnityEngine.Object.DestroyImmediate(child1);
                if (child2 != null) UnityEngine.Object.DestroyImmediate(child2);
            }
        }

        [Test]
        public void GetChildrenGameObjects_WithNullParent_ReturnsEmptyList()
        {
            // Arrange & Act
            var children = ColliderPlacementService.GetChildrenGameObjects(null);

            // Assert
            Assert.AreEqual(0, children.Count);
        }

        [Test]
        public void ValidatePoints_WithValidPoints_ReturnsTrue()
        {
            // Arrange
            var points = new List<PointObjectInfo>
            {
                new PointObjectInfo { GameObject = _point1 },
                new PointObjectInfo { GameObject = _point2 }
            };

            // Act
            bool isValid = ColliderPlacementService.ValidatePoints(points, out string errorMessage);

            // Assert
            Assert.IsTrue(isValid);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void ValidatePoints_WithEmptyList_ReturnsFalse()
        {
            // Arrange
            var emptyPoints = new List<PointObjectInfo>();

            // Act
            bool isValid = ColliderPlacementService.ValidatePoints(emptyPoints, out string errorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("ポイントリストが空です", errorMessage);
        }

        [Test]
        public void ValidatePoints_WithInsufficientValidPoints_ReturnsFalse()
        {
            // Arrange
            var points = new List<PointObjectInfo>
            {
                new PointObjectInfo { GameObject = _point1 },
                new PointObjectInfo { GameObject = null } // 無効なポイント
            };

            // Act
            bool isValid = ColliderPlacementService.ValidatePoints(points, out string errorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("有効なポイントが2個未満です", errorMessage);
        }

        [Test]
        public void CreateDefaultParent_WithValidName_CreatesParentObject()
        {
            // Arrange
            string parentName = "TestParent";

            // Act
            var parent = ColliderPlacementService.CreateDefaultParent(parentName);

            _createdObjects.Add(parent);

            // Assert
            Assert.AreEqual(parentName, parent.name);
            Assert.IsNotNull(parent);
        }

        [Test]
        public void ParentCenterAligner_WithValidTransforms_CreatesGroupAtCenter()
        {
            // Arrange
            var transforms = new Transform[] { _point1.transform, _point2.transform };
            
            // テスト前の位置を記録
            Vector3 point1Pos = _point1.transform.position;
            Vector3 point2Pos = _point2.transform.position;
            Vector3 expectedCenter = (point1Pos + point2Pos) * 0.5f;
            
            Debug.Log($"Point1 position: {point1Pos}");
            Debug.Log($"Point2 position: {point2Pos}");
            Debug.Log($"Expected center: {expectedCenter}");

            // Act
            ColliderPlacementService.ParentCenterAligner(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject, "Group object should be created");
            _createdObjects.Add(groupObject);

            Debug.Log($"Actual group position: {groupObject.transform.position}");
            
            // より寛容な比較を使用
            Vector3 actualPosition = groupObject.transform.position;
            Assert.AreEqual(expectedCenter.x, actualPosition.x, 0.01f, "X coordinate should match");
            Assert.AreEqual(expectedCenter.y, actualPosition.y, 0.01f, "Y coordinate should match");
            Assert.AreEqual(expectedCenter.z, actualPosition.z, 0.01f, "Z coordinate should match");

            // 親子関係の確認
            Assert.AreEqual(groupObject.transform, _point1.transform.parent);
            Assert.AreEqual(groupObject.transform, _point2.transform.parent);
        }

        [Test]
        public void ParentCenterAligner_WithNullArray_DoesNotThrow()
        {
            // Arrange
            Transform[] nullTransforms = null;

            // Act & Assert
            Assert.DoesNotThrow(() => ColliderPlacementService.ParentCenterAligner(nullTransforms));
        }

        [Test]
        public void ParentCenterAligner_WithEmptyArray_DoesNotThrow()
        {
            // Arrange
            Transform[] emptyTransforms = new Transform[0];

            // Act & Assert
            Assert.DoesNotThrow(() => ColliderPlacementService.ParentCenterAligner(emptyTransforms));
        }
    }
}