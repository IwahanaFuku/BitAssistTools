using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class HierarchicalSelectionServiceTests
    {
        private GameObject _rootObject;
        private GameObject _childA;
        private GameObject _childB;
        private GameObject _grandChildA1;
        private GameObject _grandChildA2;
        private GameObject _grandChildB1;

        [SetUp]
        public void SetUp()
        {
            // 階層構造を作成
            // Root
            //   ├─ ChildA
            //   │    ├─ GrandChildA1
            //   │    └─ GrandChildA2
            //   └─ ChildB
            //        └─ GrandChildB1

            _rootObject = new GameObject("Root");
            _childA = new GameObject("ChildA");
            _childB = new GameObject("ChildB");
            _grandChildA1 = new GameObject("GrandChildA1");
            _grandChildA2 = new GameObject("GrandChildA2");
            _grandChildB1 = new GameObject("GrandChildB1");

            // 親子関係を設定
            _childA.transform.SetParent(_rootObject.transform);
            _childB.transform.SetParent(_rootObject.transform);
            _grandChildA1.transform.SetParent(_childA.transform);
            _grandChildA2.transform.SetParent(_childA.transform);
            _grandChildB1.transform.SetParent(_childB.transform);
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_rootObject != null) UnityEngine.Object.DestroyImmediate(_rootObject);
            if (_childA != null) UnityEngine.Object.DestroyImmediate(_childA);
            if (_childB != null) UnityEngine.Object.DestroyImmediate(_childB);
            if (_grandChildA1 != null) UnityEngine.Object.DestroyImmediate(_grandChildA1);
            if (_grandChildA2 != null) UnityEngine.Object.DestroyImmediate(_grandChildA2);
            if (_grandChildB1 != null) UnityEngine.Object.DestroyImmediate(_grandChildB1);

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithValidRule_SelectsCorrectObjects()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, false); // 同じ階層
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_childA));
            Assert.IsTrue(selectedObjects.Contains(_childB));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithChildrenSelection_SelectsChildren()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, true); // 同じ階層の子
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.IsTrue(selectedObjects.Contains(_grandChildA1));
            Assert.IsTrue(selectedObjects.Contains(_grandChildA2));
            Assert.IsTrue(selectedObjects.Contains(_grandChildB1));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithOrderFilter_SelectsSameOrder()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, true, false); // 同じ順番のみ
            UnityEditor.Selection.activeGameObject = _childA; // 0番目の子

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(1, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_childA));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithParentDepth_SelectsFromParent()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, false, false); // 1つ上の親から
            UnityEditor.Selection.activeGameObject = _grandChildA1;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_childA));
            Assert.IsTrue(selectedObjects.Contains(_childB));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithNoActiveObject_LogsError()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, false);
            UnityEditor.Selection.activeGameObject = null;

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("アクティブなGameObjectを選択してください"));
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithInvalidRule_LogsError()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(10, false, false); // 存在しない親階層
            UnityEditor.Selection.activeGameObject = _childA;

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("選択されたオブジェクトに階層選択を適用できません"));
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithValidRule_ReturnsTrue()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, false);
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithInvalidRule_ReturnsFalse()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(10, false, false);
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithNoActiveObject_ReturnsFalse()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, false);
            UnityEditor.Selection.activeGameObject = null;

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void PreviewHierarchicalSelection_WithValidRule_ReturnsCorrectObjects()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, false);
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            var previewObjects = HierarchicalSelectionService.PreviewHierarchicalSelection(rule);

            // Assert
            Assert.AreEqual(2, previewObjects.Length);
            Assert.IsTrue(previewObjects.Contains(_childA));
            Assert.IsTrue(previewObjects.Contains(_childB));
        }

        [Test]
        public void PreviewHierarchicalSelection_WithInvalidRule_ReturnsEmptyArray()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(10, false, false);
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            var previewObjects = HierarchicalSelectionService.PreviewHierarchicalSelection(rule);

            // Assert
            Assert.AreEqual(0, previewObjects.Length);
        }

        [Test]
        public void PreviewHierarchicalSelection_WithNoActiveObject_ReturnsEmptyArray()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, false);
            UnityEditor.Selection.activeGameObject = null;

            // Act
            var previewObjects = HierarchicalSelectionService.PreviewHierarchicalSelection(rule);

            // Assert
            Assert.AreEqual(0, previewObjects.Length);
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithComplexHierarchy_SelectsCorrectly()
        {
            // Arrange - より複雑な階層構造でのテスト
            var middleChild = new GameObject("MiddleChild");
            middleChild.transform.SetParent(_childA.transform);
            var deepChild = new GameObject("DeepChild");
            deepChild.transform.SetParent(middleChild.transform);

            var rule = new HierarchicalSelectionRule(2, false, false); // 2階層上から
            UnityEditor.Selection.activeGameObject = deepChild;

            try
            {
                // Act
                HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

                // Assert
                var selectedObjects = UnityEditor.Selection.gameObjects;
                Assert.AreEqual(2, selectedObjects.Length);
                Assert.IsTrue(selectedObjects.Contains(_childA));
                Assert.IsTrue(selectedObjects.Contains(_childB));
            }
            finally
            {
                // Cleanup
                if (middleChild != null) UnityEngine.Object.DestroyImmediate(middleChild);
                if (deepChild != null) UnityEngine.Object.DestroyImmediate(deepChild);
            }
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithRootObject_SelectsRootOnly()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, false);
            UnityEditor.Selection.activeGameObject = _rootObject;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(1, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_rootObject));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithChildrenFromRoot_SelectsAllChildren()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(0, false, true); // ルートレベルの子
            UnityEditor.Selection.activeGameObject = _rootObject;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_childA));
            Assert.IsTrue(selectedObjects.Contains(_childB));
        }
    }
}