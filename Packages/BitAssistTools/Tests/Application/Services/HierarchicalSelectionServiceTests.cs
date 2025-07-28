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
            // Arrange - ChildAを選択、1階層上の親（Root）を基準とする
            var rule = new HierarchicalSelectionRule(1, false, false); // 1つ上の親から、全兄弟、ターゲット自体を選択
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert - Rootの兄弟は存在しないのでRoot自身が選択される
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(1, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_rootObject));
        }

        [Test]
        public void ExecuteHierarchicalSelection_FromGrandChild_SelectsSiblings()
        {
            // Arrange - GrandChildA1を選択、1階層上の親（ChildA）を基準とする
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.activeGameObject = _grandChildA1;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert - ChildAの兄弟（ChildAとChildB）が選択される
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_childA));
            Assert.IsTrue(selectedObjects.Contains(_childB));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithSameOrderTrue_SelectsSameIndex()
        {
            // Arrange - GrandChildA2を選択（index=1）、1階層上、同じ順番のみ
            var rule = new HierarchicalSelectionRule(1, true, false);
            UnityEditor.Selection.activeGameObject = _grandChildA2; // ChildAの2番目の子（index=1）

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert - ChildAとChildBのうち、index=1のもの（ChildB）が選択される
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(1, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_childB)); // ChildBはRootの子のindex=1
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithChildrenSelection_SelectsChildren()
        {
            // Arrange - ChildAを選択、1階層上（Root）の子オブジェクトを選択（ターゲット自体は除く）
            var rule = new HierarchicalSelectionRule(1, false, true); // 1つ上の親の子
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert - Rootの子オブジェクト（ChildAとChildB）の子が選択される
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.IsTrue(selectedObjects.Contains(_grandChildA1));
            Assert.IsTrue(selectedObjects.Contains(_grandChildA2));
            Assert.IsTrue(selectedObjects.Contains(_grandChildB1));
            // ChildAとChildB自体は選択されない（子選択モードのため）
            Assert.IsFalse(selectedObjects.Contains(_childA));
            Assert.IsFalse(selectedObjects.Contains(_childB));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithOrderFilter_SelectsSameOrder()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, true, false); // 同じ順番のみ
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
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.activeGameObject = null;

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("アクティブなGameObjectを選択してください"));
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithExceedsHierarchyDepth_LogsError()
        {
            // Arrange - 有効な範囲内だが実際の階層より深い値
            var rule = new HierarchicalSelectionRule(5, false, false); // 有効範囲内だが階層構造には存在しない
            UnityEditor.Selection.activeGameObject = _childA; // 実際の階層は2-3レベル程度

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("選択されたオブジェクトに階層選択を適用できません"));
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithValidRule_ReturnsTrue()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void HierarchicalSelectionRule_Constructor_WithInvalidParentDepth_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new HierarchicalSelectionRule(0, false, false));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new HierarchicalSelectionRule(6, false, false));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new HierarchicalSelectionRule(-1, false, false));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new HierarchicalSelectionRule(10, false, false));
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithMaxValidDepth_ReturnsFalse()
        {
            // Arrange - 最大有効深度でも実際の階層構造では実行不可能
            var rule = new HierarchicalSelectionRule(5, false, false); // 最大有効値
            UnityEditor.Selection.activeGameObject = _childA; // 階層が浅いので5深度は不可能

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsFalse(result); // 実際の階層構造で実行不可能
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithNoActiveObject_ReturnsFalse()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, false, false);
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
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.activeGameObject = _childA;

            // Act
            var previewObjects = HierarchicalSelectionService.PreviewHierarchicalSelection(rule);

            // Assert
            Assert.AreEqual(2, previewObjects.Length);
            Assert.IsTrue(previewObjects.Contains(_childA));
            Assert.IsTrue(previewObjects.Contains(_childB));
        }

        [Test]
        public void PreviewHierarchicalSelection_WithExceedsHierarchyDepth_ReturnsEmptyArray()
        {
            // Arrange - 有効な範囲内だが実際の階層より深い値
            var rule = new HierarchicalSelectionRule(5, false, false); // 有効範囲内だが階層構造には存在しない
            UnityEditor.Selection.activeGameObject = _childA; // 実際の階層は2-3レベル程度

            // Act
            var previewObjects = HierarchicalSelectionService.PreviewHierarchicalSelection(rule);

            // Assert
            Assert.AreEqual(0, previewObjects.Length); // 適用不可能なので空の配列
        }

        [Test]
        public void PreviewHierarchicalSelection_WithNoActiveObject_ReturnsEmptyArray()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.activeGameObject = null;

            // Act
            var previewObjects = HierarchicalSelectionService.PreviewHierarchicalSelection(rule);

            // Assert
            Assert.AreEqual(0, previewObjects.Length);
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithComplexHierarchy_SelectsBasedOnImplementation()
        {
            // Arrange - より複雑な階層構造でのテスト
            // Root -> ChildA -> MiddleChild -> DeepChild の階層を作成
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
                
                // DeepChild -> 2階層上 -> ChildA
                // ChildAの兄弟オブジェクト（ChildAとChildB）が選択される
                Assert.AreEqual(2, selectedObjects.Length, "ChildAとChildBが選択されるべきです");
                Assert.IsTrue(selectedObjects.Contains(_childA), "ChildAが選択されるべきです");
                Assert.IsTrue(selectedObjects.Contains(_childB), "ChildBが選択されるべきです");
            }
            finally
            {
                // Cleanup
                if (middleChild != null) UnityEngine.Object.DestroyImmediate(middleChild);
                if (deepChild != null) UnityEngine.Object.DestroyImmediate(deepChild);
            }
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithRootObject_LogsWarning()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.activeGameObject = _rootObject;

            // Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("オブジェクト 'Root' に階層選択を適用できません"));
            LogAssert.Expect(LogType.Warning, "階層選択の条件に一致するオブジェクトがありません。");
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithChildrenFromRoot_LogsWarning()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, false, true); // 1つ上の親の子
            UnityEditor.Selection.activeGameObject = _rootObject;

            // Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("オブジェクト 'Root' に階層選択を適用できません"));
            LogAssert.Expect(LogType.Warning, "階層選択の条件に一致するオブジェクトがありません。");
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithValidHierarchy_SelectsCorrectly()
        {
            // Arrange - grandchildren have enough depth for parentDepth = 1
            var rule = new HierarchicalSelectionRule(1, false, false); // 1つ上の親から
            UnityEditor.Selection.activeGameObject = _grandChildA1;

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_grandChildA1));
            Assert.IsTrue(selectedObjects.Contains(_grandChildA2));
        }

        #region 複数選択対応テスト

        [Test]
        public void ExecuteHierarchicalSelection_WithMultipleValidObjects_SelectsAllCorrespondingObjects()
        {
            // Arrange - 複数の有効なオブジェクトを選択
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _grandChildA1, _grandChildB1 };

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            // _grandChildA1の兄弟: _grandChildA1, _grandChildA2
            // _grandChildB1の兄弟: _grandChildB1
            // 重複除去後: _grandChildA1, _grandChildA2, _grandChildB1
            Assert.AreEqual(3, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_grandChildA1));
            Assert.IsTrue(selectedObjects.Contains(_grandChildA2));
            Assert.IsTrue(selectedObjects.Contains(_grandChildB1));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithMixedValidInvalidObjects_SelectsOnlyValidResults()
        {
            // Arrange - 有効と無効なオブジェクトを混在
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _grandChildA1, _rootObject }; // rootObjectは無効

            // Act
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("オブジェクト 'Root' に階層選択を適用できません"));
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            // _grandChildA1のみ処理され、その兄弟が選択される
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_grandChildA1));
            Assert.IsTrue(selectedObjects.Contains(_grandChildA2));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithMultipleObjectsSameParent_RemovesDuplicates()
        {
            // Arrange - 同じ親を持つ複数オブジェクトを選択
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _grandChildA1, _grandChildA2 };

            // Act
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);

            // Assert
            var selectedObjects = UnityEditor.Selection.gameObjects;
            // 両方とも同じ兄弟セット(_grandChildA1, _grandChildA2)を生成するが重複除去される
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_grandChildA1));
            Assert.IsTrue(selectedObjects.Contains(_grandChildA2));
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithOnlyInvalidObjects_LogsWarningAndDoesNothing()
        {
            // Arrange - 全て無効なオブジェクト
            var rule = new HierarchicalSelectionRule(2, false, false); // 2階層必要だがrootObjectは親がない
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _rootObject };

            // Act
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("オブジェクト 'Root' に階層選択を適用できません"));
            LogAssert.Expect(LogType.Warning, "階層選択の条件に一致するオブジェクトがありません。");
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void ExecuteHierarchicalSelection_WithNoSelection_LogsError()
        {
            // Arrange - 何も選択されていない状態
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            LogAssert.Expect(LogType.Error, "GameObjectを選択してください。");
            HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithMultipleValidObjects_ReturnsTrue()
        {
            // Arrange - 複数の有効なオブジェクト
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _grandChildA1, _grandChildB1 };

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithMixedValidInvalidObjects_ReturnsTrue()
        {
            // Arrange - 一部有効なオブジェクト
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _grandChildA1, _rootObject };

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsTrue(result); // 1つでも有効なら実行可能
        }

        [Test]
        public void CanExecuteHierarchicalSelection_WithOnlyInvalidObjects_ReturnsFalse()
        {
            // Arrange - 全て無効なオブジェクト
            var rule = new HierarchicalSelectionRule(2, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _rootObject };

            // Act
            bool result = HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetDetailedPreview_WithMultipleValidObjects_GeneratesCorrectPreview()
        {
            // Arrange - 複数の有効なオブジェクト
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _grandChildA1, _grandChildB1 };

            // Act
            var preview = HierarchicalSelectionService.GetDetailedPreview(rule);

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.AreEqual(3, preview.TargetCount); // _grandChildA1, _grandChildA2, _grandChildB1
            Assert.IsTrue(preview.SelectionReason.Contains("2個のオブジェクト選択中"));
            Assert.IsTrue(preview.SelectionReason.Contains("全て適用可能"));
            Assert.IsTrue(preview.SelectionReason.Contains("合計3個のオブジェクト"));
        }

        [Test]
        public void GetDetailedPreview_WithMixedValidInvalidObjects_GeneratesPartialPreview()
        {
            // Arrange - 一部有効なオブジェクト
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _grandChildA1, _rootObject };

            // Act
            var preview = HierarchicalSelectionService.GetDetailedPreview(rule);

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.IsTrue(preview.SelectionReason.Contains("2個のオブジェクト選択中"));
            Assert.IsTrue(preview.SelectionReason.Contains("1個が適用可能"));
            Assert.IsTrue(preview.SelectionReason.Contains("1個のオブジェクトは親階層不足のためスキップ"));
        }

        [Test]
        public void GetDetailedPreview_WithOnlyInvalidObjects_ReturnsInvalidPreview()
        {
            // Arrange - 全て無効なオブジェクト
            var rule = new HierarchicalSelectionRule(2, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _rootObject };

            // Act
            var preview = HierarchicalSelectionService.GetDetailedPreview(rule);

            // Assert
            Assert.IsFalse(preview.IsValid);
            Assert.IsTrue(preview.ErrorMessage.Contains("階層選択を適用できません"));
        }

        [Test]  
        public void GetDetailedPreview_WithNoSelection_ReturnsInvalidPreview()
        {
            // Arrange - 何も選択されていない
            var rule = new HierarchicalSelectionRule(1, false, false);
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act
            var preview = HierarchicalSelectionService.GetDetailedPreview(rule);

            // Assert
            Assert.IsFalse(preview.IsValid);
            Assert.AreEqual("GameObjectを選択してください。", preview.ErrorMessage);
        }

        #endregion
    }
}