using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Infrastructure.Unity;

namespace BitAssistTools.Tests.Infrastructure.Unity
{
    [TestFixture]
    public class UnityObjectGroupingServiceTests
    {
        private UnityObjectGroupingService _service;
        private GameObject _testObject1;
        private GameObject _testObject2;
        private GameObject _testObject3;
        private GameObject _parentObject;

        [SetUp]
        public void SetUp()
        {
            _service = new UnityObjectGroupingService();
            
            // テスト用のGameObjectを作成
            _parentObject = new GameObject("Parent");
            _testObject1 = new GameObject("TestObject1");
            _testObject2 = new GameObject("TestObject2");
            _testObject3 = new GameObject("TestObject3");

            // 階層構造を設定
            _testObject1.transform.SetParent(_parentObject.transform);
            _testObject2.transform.SetParent(_parentObject.transform);
            _testObject3.transform.SetParent(_parentObject.transform);

            // 位置を設定
            _testObject1.transform.position = new Vector3(0, 0, 0);
            _testObject2.transform.position = new Vector3(2, 0, 0);
            _testObject3.transform.position = new Vector3(4, 0, 0);
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_testObject1 != null) UnityEngine.Object.DestroyImmediate(_testObject1);
            if (_testObject2 != null) UnityEngine.Object.DestroyImmediate(_testObject2);
            if (_testObject3 != null) UnityEngine.Object.DestroyImmediate(_testObject3);
            if (_parentObject != null) UnityEngine.Object.DestroyImmediate(_parentObject);

            // 動的に作成されたGroupオブジェクトを削除
            var groupObjects = GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name == "Group")
                .ToArray();
            foreach (var group in groupObjects)
            {
                UnityEngine.Object.DestroyImmediate(group);
            }

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void CreateParentCenterGroup_WithValidTransforms_CreatesGroupAtCenter()
        {
            // Arrange
            var transforms = new Transform[] { _testObject1.transform, _testObject2.transform, _testObject3.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform; // 親の参照のため

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject, "Group object should be created");
            
            // 中央位置の確認（グローバル座標での中央: (0+2+4)/3 = 2）
            var expectedCenter = new Vector3(2, 0, 0);
            Assert.AreEqual(expectedCenter.x, groupObject.transform.position.x, 0.01f);
            Assert.AreEqual(expectedCenter.y, groupObject.transform.position.y, 0.01f);
            Assert.AreEqual(expectedCenter.z, groupObject.transform.position.z, 0.01f);
        }

        [Test]
        public void CreateParentCenterGroup_WithValidTransforms_SetsCorrectParent()
        {
            // Arrange
            var transforms = new Transform[] { _testObject1.transform, _testObject2.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform;

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject);
            Assert.AreEqual(_parentObject.transform, groupObject.transform.parent);
        }

        [Test]
        public void CreateParentCenterGroup_WithValidTransforms_GroupsObjectsAsChildren()
        {
            // Arrange
            var transforms = new Transform[] { _testObject1.transform, _testObject2.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform;

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject);
            
            // 子オブジェクトの確認
            Assert.AreEqual(groupObject.transform, _testObject1.transform.parent);
            Assert.AreEqual(groupObject.transform, _testObject2.transform.parent);
            Assert.AreEqual(2, groupObject.transform.childCount);
        }

        [Test]
        public void CreateParentCenterGroup_WithValidTransforms_SetsActiveSelection()
        {
            // Arrange
            var transforms = new Transform[] { _testObject1.transform, _testObject2.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform;

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject);
            Assert.AreEqual(groupObject, UnityEditor.Selection.activeGameObject);
        }

        [Test]
        public void CreateParentCenterGroup_WithSingleTransform_CreatesGroupCorrectly()
        {
            // Arrange
            var transforms = new Transform[] { _testObject1.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform;

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject);
            Assert.AreEqual(_testObject1.transform.position, groupObject.transform.position);
            Assert.AreEqual(groupObject.transform, _testObject1.transform.parent);
        }

        [Test]
        public void CreateParentCenterGroup_WithNullArray_DoesNothing()
        {
            // Arrange
            Transform[] transforms = null;

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNull(groupObject, "No group should be created for null input");
        }

        [Test]
        public void CreateParentCenterGroup_WithEmptyArray_DoesNothing()
        {
            // Arrange
            var transforms = new Transform[0];

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNull(groupObject, "No group should be created for empty input");
        }

        [Test]
        public void CreateParentCenterGroup_WithoutParent_CreatesGroupAtWorldOrigin()
        {
            // Arrange
            // ルートレベルのオブジェクトを作成
            var rootObject1 = new GameObject("Root1");
            var rootObject2 = new GameObject("Root2");
            rootObject1.transform.position = Vector3.zero;
            rootObject2.transform.position = new Vector3(2, 0, 0);
            
            var transforms = new Transform[] { rootObject1.transform, rootObject2.transform };
            UnityEditor.Selection.activeTransform = rootObject1.transform;

            try
            {
                // Act
                _service.CreateParentCenterGroup(transforms);

                // Assert
                var groupObject = GameObject.Find("Group");
                Assert.IsNotNull(groupObject);
                Assert.IsNull(groupObject.transform.parent, "Group should be at root level");
                
                // 中央位置の確認
                var expectedCenter = new Vector3(1, 0, 0);
                Assert.AreEqual(expectedCenter.x, groupObject.transform.position.x, 0.01f);
            }
            finally
            {
                // Cleanup
                if (rootObject1 != null) UnityEngine.Object.DestroyImmediate(rootObject1);
                if (rootObject2 != null) UnityEngine.Object.DestroyImmediate(rootObject2);
            }
        }

        [Test]
        public void CreateParentCenterGroup_PreservesSiblingOrder()
        {
            // Arrange
            _testObject1.transform.SetSiblingIndex(1);
            _testObject2.transform.SetSiblingIndex(0);
            _testObject3.transform.SetSiblingIndex(2);
            
            var transforms = new Transform[] { _testObject1.transform, _testObject2.transform, _testObject3.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform;

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject);
            
            // グループは最小のSiblingIndex（0）の位置に配置される
            Assert.AreEqual(0, groupObject.transform.GetSiblingIndex());
        }

        [Test]
        public void CreateParentCenterGroup_CalculatesCenterCorrectly()
        {
            // Arrange - 特定の位置に配置
            _testObject1.transform.position = new Vector3(1, 2, 3);
            _testObject2.transform.position = new Vector3(4, 5, 6);
            _testObject3.transform.position = new Vector3(7, 8, 9);
            
            var transforms = new Transform[] { _testObject1.transform, _testObject2.transform, _testObject3.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform;

            // Act
            _service.CreateParentCenterGroup(transforms);

            // Assert
            var groupObject = GameObject.Find("Group");
            Assert.IsNotNull(groupObject);
            
            // 中央位置の計算: (1+4+7)/3=4, (2+5+8)/3=5, (3+6+9)/3=6
            var expectedCenter = new Vector3(4, 5, 6);
            Assert.AreEqual(expectedCenter.x, groupObject.transform.position.x, 0.01f);
            Assert.AreEqual(expectedCenter.y, groupObject.transform.position.y, 0.01f);
            Assert.AreEqual(expectedCenter.z, groupObject.transform.position.z, 0.01f);
        }

        [Test]
        public void CreateParentCenterGroup_HandlesNullTransformsInArray()
        {
            // Arrange
            var transforms = new Transform[] { _testObject1.transform, null, _testObject2.transform };
            UnityEditor.Selection.activeTransform = _testObject1.transform;

            // Act & Assert - 例外が発生せず、有効なTransformのみで処理される
            Assert.DoesNotThrow(() => _service.CreateParentCenterGroup(transforms));
            
            var groupObject = GameObject.Find("Group");
            // nullがある場合の動作は実装依存だが、少なくとも例外は発生しない
        }
    }
}