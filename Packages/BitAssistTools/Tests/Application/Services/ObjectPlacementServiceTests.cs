using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class ObjectPlacementServiceTests
    {
        private GameObject _testObject1;
        private GameObject _testObject2;
        private GameObject _testPrefab;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _testObject1 = new GameObject("TestObject1");
            _testObject1.transform.position = Vector3.zero;
            _testObject1.transform.rotation = Quaternion.identity;
            _testObject1.transform.localScale = Vector3.one;

            _testObject2 = new GameObject("TestObject2");
            _testObject2.transform.position = new Vector3(1, 0, 0);
            _testObject2.transform.rotation = Quaternion.identity;
            _testObject2.transform.localScale = Vector3.one;

            // テスト用のPrefabを作成
            _testPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _testPrefab.name = "TestPrefab";
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_testObject1 != null) UnityEngine.Object.DestroyImmediate(_testObject1);
            if (_testObject2 != null) UnityEngine.Object.DestroyImmediate(_testObject2);
            if (_testPrefab != null) UnityEngine.Object.DestroyImmediate(_testPrefab);

            // 作成されたオブジェクトをクリーンアップ
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in createdObjects)
            {
                if (obj.name.Contains("Copy") || obj.name.Contains("Group") || obj.name.Contains("TestPrefab"))
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void ExecuteSpecialDuplicate_WithGlobalCoordinate_CreatesCorrectCopies()
        {
            // Arrange
            UnityEditor.Selection.activeGameObject = _testObject1;
            var coordinate = CoordinateSystem.Global;
            var offset = new TransformData(
                new Vector3(2, 0, 0), 
                Quaternion.identity, 
                Vector3.one
            );
            int copyCount = 2;

            // Act
            ObjectPlacementService.ExecuteSpecialDuplicate(coordinate, offset, copyCount);

            // Assert
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            var copies = System.Array.FindAll(createdObjects, obj => obj.name == "TestObject1" && obj != _testObject1);
            
            Assert.AreEqual(copyCount, copies.Length, "Should create the specified number of copies");
            
            if (copies.Length >= 2)
            {
                // 複利効果の確認：最初のコピーは(2,0,0)、次のコピーは(4,0,0)
                Assert.AreEqual(2f, copies[0].transform.position.x, 0.001f, "First copy should be at offset position");
                Assert.AreEqual(4f, copies[1].transform.position.x, 0.001f, "Second copy should show compound effect");
            }
        }

        [Test]
        public void ExecuteSpecialDuplicate_WithLocalCoordinate_CreatesCorrectCopies()
        {
            // Arrange
            _testObject1.transform.rotation = Quaternion.Euler(0, 45, 0); // 45度回転
            UnityEditor.Selection.activeGameObject = _testObject1;
            var coordinate = CoordinateSystem.Local;
            var offset = new TransformData(
                new Vector3(1, 0, 0), // ローカルX軸方向
                Quaternion.identity, 
                Vector3.one
            );
            int copyCount = 1;

            // Act
            ObjectPlacementService.ExecuteSpecialDuplicate(coordinate, offset, copyCount);

            // Assert
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            var copies = System.Array.FindAll(createdObjects, obj => obj.name == "TestObject1" && obj != _testObject1);
            
            Assert.AreEqual(copyCount, copies.Length, "Should create the specified number of copies");
            
            if (copies.Length >= 1)
            {
                // ローカル座標でのオフセットが適用されていることを確認
                var expectedPosition = _testObject1.transform.TransformDirection(Vector3.right);
                Assert.AreNotEqual(Vector3.right, copies[0].transform.position, "Position should be transformed by local coordinate");
            }
        }

        [Test]
        public void ExecuteSpecialDuplicate_WithZeroCopyCount_DoesNotCreateCopies()
        {
            // Arrange
            UnityEditor.Selection.activeGameObject = _testObject1;
            var coordinate = CoordinateSystem.Global;
            var offset = new TransformData(Vector3.one, Quaternion.identity, Vector3.one);
            int copyCount = 0;

            var originalObjectCount = GameObject.FindObjectsOfType<GameObject>().Length;

            // Act
            ObjectPlacementService.ExecuteSpecialDuplicate(coordinate, offset, copyCount);

            // Assert
            var currentObjectCount = GameObject.FindObjectsOfType<GameObject>().Length;
            Assert.AreEqual(originalObjectCount, currentObjectCount, "Should not create any copies with zero count");
        }

        [Test]
        public void ExecuteSpecialDuplicate_WithMultipleSelectedObjects_DuplicatesAllObjects()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };
            var coordinate = CoordinateSystem.Global;
            var offset = new TransformData(Vector3.up, Quaternion.identity, Vector3.one);
            int copyCount = 1;

            // Act
            ObjectPlacementService.ExecuteSpecialDuplicate(coordinate, offset, copyCount);

            // Assert
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            var object1Copies = System.Array.FindAll(createdObjects, obj => obj.name == "TestObject1" && obj != _testObject1);
            var object2Copies = System.Array.FindAll(createdObjects, obj => obj.name == "TestObject2" && obj != _testObject2);
            
            Assert.AreEqual(copyCount, object1Copies.Length, "Should create copies of first object");
            Assert.AreEqual(copyCount, object2Copies.Length, "Should create copies of second object");
        }

        [Test]
        public void ExecuteObjectSwapper_WithValidPrefab_ReplacesSelectedObjects()
        {
            // Arrange
            UnityEditor.Selection.activeGameObject = _testObject1;
            var originalPosition = _testObject1.transform.position;
            var originalRotation = _testObject1.transform.rotation;
            var originalScale = _testObject1.transform.localScale;

            // Act
            ObjectPlacementService.ExecuteObjectSwapper(_testPrefab);

            // Assert
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            var replacementObjects = System.Array.FindAll(createdObjects, obj => obj.name.Contains("TestPrefab"));
            
            Assert.AreEqual(1, replacementObjects.Length, "Should create one replacement object");
            
            if (replacementObjects.Length > 0)
            {
                var replacement = replacementObjects[0];
                Assert.AreEqual(originalPosition, replacement.transform.position, "Should preserve original position");
                Assert.AreEqual(originalRotation, replacement.transform.rotation, "Should preserve original rotation");
                Assert.AreEqual(originalScale, replacement.transform.localScale, "Should preserve original scale");
            }

            // 元のオブジェクトが削除されていることを確認
            var originalObjects = System.Array.FindAll(createdObjects, obj => obj == _testObject1);
            Assert.AreEqual(0, originalObjects.Length, "Original object should be destroyed");
        }

        [Test]
        public void ExecuteObjectSwapper_WithNullPrefab_LogsErrorAndDoesNothing()
        {
            // Arrange
            UnityEditor.Selection.activeGameObject = _testObject1;
            var originalObjectCount = GameObject.FindObjectsOfType<GameObject>().Length;

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Prefab is not set"));
            ObjectPlacementService.ExecuteObjectSwapper(null);

            var currentObjectCount = GameObject.FindObjectsOfType<GameObject>().Length;
            Assert.AreEqual(originalObjectCount, currentObjectCount, "Should not modify scene when prefab is null");
        }

        [Test]
        public void ExecuteObjectSwapper_WithMultipleSelectedObjects_ReplacesAllObjects()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            ObjectPlacementService.ExecuteObjectSwapper(_testPrefab);

            // Assert
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            var replacementObjects = System.Array.FindAll(createdObjects, obj => obj.name.Contains("TestPrefab"));
            
            Assert.AreEqual(2, replacementObjects.Length, "Should create replacement for each selected object");

            // 元のオブジェクトが削除されていることを確認
            var originalObjects1 = System.Array.FindAll(createdObjects, obj => obj == _testObject1);
            var originalObjects2 = System.Array.FindAll(createdObjects, obj => obj == _testObject2);
            Assert.AreEqual(0, originalObjects1.Length, "First original object should be destroyed");
            Assert.AreEqual(0, originalObjects2.Length, "Second original object should be destroyed");
        }

        [Test]
        public void ExecuteSpecialDuplicate_WithScaleOffset_AppliesCompoundScaling()
        {
            // Arrange
            UnityEditor.Selection.activeGameObject = _testObject1;
            var coordinate = CoordinateSystem.Global;
            var offset = new TransformData(
                Vector3.zero, 
                Quaternion.identity, 
                new Vector3(2, 1, 1) // X軸を2倍に
            );
            int copyCount = 2;

            // Act
            ObjectPlacementService.ExecuteSpecialDuplicate(coordinate, offset, copyCount);

            // Assert
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            var copies = System.Array.FindAll(createdObjects, obj => obj.name == "TestObject1" && obj != _testObject1);
            
            Assert.AreEqual(copyCount, copies.Length, "Should create the specified number of copies");
            
            if (copies.Length >= 2)
            {
                // 複利効果の確認：最初のコピーは(2,1,1)、次のコピーは(4,1,1)
                Assert.AreEqual(2f, copies[0].transform.localScale.x, 0.001f, "First copy should have 2x scale");
                Assert.AreEqual(4f, copies[1].transform.localScale.x, 0.001f, "Second copy should show compound scaling (2x2=4)");
            }
        }

        [Test]
        public void ExecuteSpecialDuplicate_WithRotationOffset_AppliesCompoundRotation()
        {
            // Arrange
            UnityEditor.Selection.activeGameObject = _testObject1;
            var coordinate = CoordinateSystem.Global;
            var offset = new TransformData(
                Vector3.zero, 
                Quaternion.Euler(0, 90, 0), // Y軸周りに90度回転
                Vector3.one
            );
            int copyCount = 2;

            // Act
            ObjectPlacementService.ExecuteSpecialDuplicate(coordinate, offset, copyCount);

            // Assert
            var createdObjects = GameObject.FindObjectsOfType<GameObject>();
            var copies = System.Array.FindAll(createdObjects, obj => obj.name == "TestObject1" && obj != _testObject1);
            
            Assert.AreEqual(copyCount, copies.Length, "Should create the specified number of copies");
            
            if (copies.Length >= 2)
            {
                // 複利効果の確認：最初のコピーは90度、次のコピーは180度
                Assert.AreEqual(90f, copies[0].transform.rotation.eulerAngles.y, 1f, "First copy should have 90 degree rotation");
                Assert.AreEqual(180f, copies[1].transform.rotation.eulerAngles.y, 1f, "Second copy should show compound rotation (90+90=180)");
            }
        }

        [Test]
        public void ExecuteObjectSwapper_WithParentedObject_PreservesParentRelationship()
        {
            // Arrange
            var parent = new GameObject("Parent");
            _testObject1.transform.SetParent(parent.transform);
            UnityEditor.Selection.activeGameObject = _testObject1;

            try
            {
                // Act
                ObjectPlacementService.ExecuteObjectSwapper(_testPrefab);

                // Assert
                var createdObjects = GameObject.FindObjectsOfType<GameObject>();
                var replacementObjects = System.Array.FindAll(createdObjects, obj => obj.name.Contains("TestPrefab"));
                
                Assert.AreEqual(1, replacementObjects.Length, "Should create one replacement object");
                
                if (replacementObjects.Length > 0)
                {
                    var replacement = replacementObjects[0];
                    Assert.AreEqual(parent.transform, replacement.transform.parent, "Should preserve parent relationship");
                }
            }
            finally
            {
                // Cleanup
                if (parent != null) UnityEngine.Object.DestroyImmediate(parent);
            }
        }
    }
}