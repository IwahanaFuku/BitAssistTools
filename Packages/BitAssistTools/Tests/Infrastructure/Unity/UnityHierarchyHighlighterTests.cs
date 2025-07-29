using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Infrastructure.Unity;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Tests.Infrastructure.Unity
{
    /// <summary>
    /// UnityHierarchyHighlighterのテストクラス
    /// </summary>
    [TestFixture]
    public class UnityHierarchyHighlighterTests
    {
        private GameObject _baseParent;
        private GameObject[] _targetObjects;
        private HierarchicalSelectionPreview _validPreview;
        private HierarchicalSelectionPreview _invalidPreview;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _baseParent = new GameObject("BaseParent");
            _targetObjects = new GameObject[]
            {
                new GameObject("Target1"),
                new GameObject("Target2")
            };

            // 有効なプレビューを作成
            _validPreview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                new GameObject[] { _baseParent },
                _targetObjects,
                "テスト用の選択理由"
            );

            // 無効なプレビューを作成
            _invalidPreview = HierarchicalSelectionPreview.Invalid("テストエラー");

            // ハイライトが停止状態であることを確認
            UnityHierarchyHighlighter.StopHighlight();
        }

        [TearDown]
        public void TearDown()
        {
            // ハイライトを停止
            UnityHierarchyHighlighter.StopHighlight();

            // テスト用オブジェクトをクリーンアップ
            if (_baseParent != null)
                UnityEngine.Object.DestroyImmediate(_baseParent);

            if (_targetObjects != null)
            {
                foreach (var target in _targetObjects)
                {
                    if (target != null)
                        UnityEngine.Object.DestroyImmediate(target);
                }
            }
        }

        #region Start/Stop Highlight Tests

        [Test]
        public void StartHighlight_WithValidPreview_EnablesHighlight()
        {
            // Arrange
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);

            // Act
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            // Assert
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightEnabled);
        }

        [Test]
        public void StopHighlight_WhenHighlightEnabled_DisablesHighlight()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightEnabled);

            // Act
            UnityHierarchyHighlighter.StopHighlight();

            // Assert
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);
        }

        [Test]
        public void StopHighlight_WhenHighlightDisabled_DoesNotThrow()
        {
            // Arrange
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);

            // Act & Assert
            Assert.DoesNotThrow(() => UnityHierarchyHighlighter.StopHighlight());
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);
        }

        [Test]
        public void StartHighlight_WithInvalidPreview_EnablesHighlightButWithoutObjects()
        {
            // Act
            UnityHierarchyHighlighter.StartHighlight(_invalidPreview);

            // Assert
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightEnabled);
            
            // 無効なプレビューなので、オブジェクトはハイライト対象外
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(_targetObjects[0]));
        }

        #endregion

        #region Update Preview Tests

        [Test]
        public void UpdatePreview_WhenHighlightEnabled_UpdatesPreview()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));

            var newTargetObject = new GameObject("NewTarget");
            var newPreview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                new GameObject[] { _baseParent },
                new GameObject[] { newTargetObject },
                "更新されたプレビュー"
            );

            try
            {
                // Act
                UnityHierarchyHighlighter.UpdatePreview(newPreview);

                // Assert
                Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightEnabled);
                Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
                Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(newTargetObject));
                Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(_targetObjects[0])); // 古いターゲットは除外
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(newTargetObject);
            }
        }

        [Test]
        public void UpdatePreview_WhenHighlightDisabled_DoesNothing()
        {
            // Arrange
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);

            // Act
            UnityHierarchyHighlighter.UpdatePreview(_validPreview);

            // Assert
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
        }

        #endregion

        #region Object Highlight Query Tests

        [Test]
        public void IsHighlightedObject_WithBaseParent_ReturnsTrue()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            // Act & Assert
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
        }

        [Test]
        public void IsHighlightedObject_WithTargetObject_ReturnsTrue()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            // Act & Assert
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_targetObjects[0]));
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_targetObjects[1]));
        }

        [Test]
        public void IsHighlightedObject_WithNonHighlightedObject_ReturnsFalse()
        {
            // Arrange
            var nonHighlightedObject = new GameObject("NonHighlighted");
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            try
            {
                // Act & Assert
                Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(nonHighlightedObject));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(nonHighlightedObject);
            }
        }

        [Test]
        public void IsHighlightedObject_WhenHighlightDisabled_ReturnsFalse()
        {
            // Arrange
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);

            // Act & Assert
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(_targetObjects[0]));
        }

        [Test]
        public void IsHighlightedObject_WithNullObject_ReturnsFalse()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            // Act & Assert
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightedObject(null));
        }

        #endregion

        #region Highlight Type Query Tests

        [Test]
        public void GetHighlightType_WithBaseParent_ReturnsCorrectType()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            // Act
            var highlightType = UnityHierarchyHighlighter.GetHighlightType(_baseParent);

            // Assert
            Assert.AreEqual("基準親", highlightType);
        }

        [Test]
        public void GetHighlightType_WithTargetObject_ReturnsCorrectType()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            // Act
            var highlightType = UnityHierarchyHighlighter.GetHighlightType(_targetObjects[0]);

            // Assert
            Assert.AreEqual("選択対象", highlightType);
        }

        [Test]
        public void GetHighlightType_WithNonHighlightedObject_ReturnsNull()
        {
            // Arrange
            var nonHighlightedObject = new GameObject("NonHighlighted");
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            try
            {
                // Act
                var highlightType = UnityHierarchyHighlighter.GetHighlightType(nonHighlightedObject);

                // Assert
                Assert.IsNull(highlightType);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(nonHighlightedObject);
            }
        }

        [Test]
        public void GetHighlightType_WhenHighlightDisabled_ReturnsNull()
        {
            // Arrange
            Assert.IsFalse(UnityHierarchyHighlighter.IsHighlightEnabled);

            // Act
            var highlightType = UnityHierarchyHighlighter.GetHighlightType(_baseParent);

            // Assert
            Assert.IsNull(highlightType);
        }

        [Test]
        public void GetHighlightType_WithNullObject_ReturnsNull()
        {
            // Arrange
            UnityHierarchyHighlighter.StartHighlight(_validPreview);

            // Act
            var highlightType = UnityHierarchyHighlighter.GetHighlightType(null);

            // Assert
            Assert.IsNull(highlightType);
        }

        #endregion

        #region Multiple Objects Tests

        [Test]
        public void StartHighlight_WithMultipleTargetObjects_HighlightsAllTargets()
        {
            // Arrange
            var additionalTargets = new GameObject[]
            {
                new GameObject("Target3"),
                new GameObject("Target4")
            };

            var allTargets = new GameObject[_targetObjects.Length + additionalTargets.Length];
            _targetObjects.CopyTo(allTargets, 0);
            additionalTargets.CopyTo(allTargets, _targetObjects.Length);

            var multipleTargetPreview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                new GameObject[] { _baseParent },
                allTargets,
                "複数ターゲットテスト"
            );

            try
            {
                // Act
                UnityHierarchyHighlighter.StartHighlight(multipleTargetPreview);

                // Assert
                Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
                foreach (var target in allTargets)
                {
                    Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(target));
                    Assert.AreEqual("選択対象", UnityHierarchyHighlighter.GetHighlightType(target));
                }
            }
            finally
            {
                foreach (var target in additionalTargets)
                {
                    UnityEngine.Object.DestroyImmediate(target);
                }
            }
        }

        [Test]
        public void StartHighlight_WithDuplicateObjects_HandlesCorrectly()
        {
            // Arrange - 基準親がターゲットにも含まれる場合
            var targetsWithDuplicate = new GameObject[] { _baseParent, _targetObjects[0] };
            var duplicatePreview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                new GameObject[] { _baseParent },
                targetsWithDuplicate,
                "重複オブジェクトテスト"
            );

            // Act
            UnityHierarchyHighlighter.StartHighlight(duplicatePreview);

            // Assert
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
            // 基準親として認識される（最初に基準親IDセットに追加されるため）
            Assert.AreEqual("基準親", UnityHierarchyHighlighter.GetHighlightType(_baseParent));
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void StartHighlight_WithEmptyTargetObjects_DoesNotThrow()
        {
            // Arrange
            var emptyPreview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                new GameObject[] { _baseParent },
                new GameObject[0],
                "空のターゲット"
            );

            // Act & Assert
            Assert.DoesNotThrow(() => UnityHierarchyHighlighter.StartHighlight(emptyPreview));
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightEnabled);
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_baseParent));
        }

        [Test]
        public void StartHighlight_WithNullBaseParent_HandlesGracefully()
        {
            // Arrange
            var nullBaseParentPreview = HierarchicalSelectionPreview.Valid(
                null,
                new GameObject[0],
                _targetObjects,
                "基準親なし"
            );

            // Act & Assert
            Assert.DoesNotThrow(() => UnityHierarchyHighlighter.StartHighlight(nullBaseParentPreview));
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightEnabled);
            Assert.IsTrue(UnityHierarchyHighlighter.IsHighlightedObject(_targetObjects[0]));
        }

        #endregion
    }
}