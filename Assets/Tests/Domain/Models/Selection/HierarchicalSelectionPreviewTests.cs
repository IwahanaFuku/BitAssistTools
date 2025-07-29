using System;
using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Tests.Domain.Models.Selection
{
    /// <summary>
    /// HierarchicalSelectionPreviewのテストクラス
    /// </summary>
    [TestFixture]
    public class HierarchicalSelectionPreviewTests
    {
        private GameObject _baseParent;
        private GameObject[] _parentSiblings;
        private GameObject[] _targetObjects;
        private string _selectionReason;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _baseParent = new GameObject("BaseParent");
            
            _parentSiblings = new GameObject[]
            {
                new GameObject("Sibling1"),
                new GameObject("Sibling2"),
                new GameObject("Sibling3")
            };

            _targetObjects = new GameObject[]
            {
                new GameObject("Target1"),
                new GameObject("Target2")
            };

            _selectionReason = "テスト用の選択理由";
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトをクリーンアップ
            if (_baseParent != null)
                UnityEngine.Object.DestroyImmediate(_baseParent);

            if (_parentSiblings != null)
            {
                foreach (var sibling in _parentSiblings)
                {
                    if (sibling != null)
                        UnityEngine.Object.DestroyImmediate(sibling);
                }
            }

            if (_targetObjects != null)
            {
                foreach (var target in _targetObjects)
                {
                    if (target != null)
                        UnityEngine.Object.DestroyImmediate(target);
                }
            }
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidParameters_CreatesValidPreview()
        {
            // Act
            var preview = new HierarchicalSelectionPreview(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason,
                true,
                null
            );

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.AreEqual(_baseParent, preview.BaseParent);
            Assert.AreEqual(_parentSiblings, preview.ParentSiblings);
            Assert.AreEqual(_targetObjects, preview.TargetObjects);
            Assert.AreEqual(_selectionReason, preview.SelectionReason);
            Assert.AreEqual(string.Empty, preview.ErrorMessage);
        }

        [Test]
        public void Constructor_WithNullArrays_HandlesGracefully()
        {
            // Act
            var preview = new HierarchicalSelectionPreview(
                _baseParent,
                null,
                null,
                _selectionReason,
                true,
                null
            );

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.AreEqual(0, preview.ParentSiblingCount);
            Assert.AreEqual(0, preview.TargetCount);
            Assert.IsNotNull(preview.ParentSiblings);
            Assert.IsNotNull(preview.TargetObjects);
        }

        [Test]
        public void Constructor_WithNullStrings_HandlesGracefully()
        {
            // Act
            var preview = new HierarchicalSelectionPreview(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                null,
                true,
                null
            );

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.AreEqual(string.Empty, preview.SelectionReason);
            Assert.AreEqual(string.Empty, preview.ErrorMessage);
        }

        #endregion

        #region Static Factory Methods Tests

        [Test]
        public void Invalid_WithErrorMessage_CreatesInvalidPreview()
        {
            // Arrange
            var errorMessage = "テストエラーメッセージ";

            // Act
            var preview = HierarchicalSelectionPreview.Invalid(errorMessage);

            // Assert
            Assert.IsFalse(preview.IsValid);
            Assert.AreEqual(errorMessage, preview.ErrorMessage);
            Assert.IsNull(preview.BaseParent);
            Assert.AreEqual(0, preview.ParentSiblingCount);
            Assert.AreEqual(0, preview.TargetCount);
            Assert.AreEqual(string.Empty, preview.SelectionReason);
        }

        [Test]
        public void Valid_WithValidParameters_CreatesValidPreview()
        {
            // Act
            var preview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.AreEqual(_baseParent, preview.BaseParent);
            Assert.AreEqual(_parentSiblings, preview.ParentSiblings);
            Assert.AreEqual(_targetObjects, preview.TargetObjects);
            Assert.AreEqual(_selectionReason, preview.SelectionReason);
            Assert.AreEqual(string.Empty, preview.ErrorMessage);
        }

        #endregion

        #region Property Tests

        [Test]
        public void TargetCount_WithTargetObjects_ReturnsCorrectCount()
        {
            // Arrange
            var preview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Act & Assert
            Assert.AreEqual(2, preview.TargetCount);
        }

        [Test]
        public void ParentSiblingCount_WithSiblings_ReturnsCorrectCount()
        {
            // Arrange
            var preview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Act & Assert
            Assert.AreEqual(3, preview.ParentSiblingCount);
        }

        [Test]
        public void TargetCount_WithNullTargetObjects_ReturnsZero()
        {
            // Arrange
            var preview = new HierarchicalSelectionPreview(
                _baseParent,
                _parentSiblings,
                null,
                _selectionReason,
                true,
                null
            );

            // Act & Assert
            Assert.AreEqual(0, preview.TargetCount);
        }

        #endregion

        #region Text Generation Tests

        [Test]
        public void GetSummaryText_ValidPreview_ReturnsCorrectSummary()
        {
            // Arrange
            var preview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Act
            var summary = preview.GetSummaryText();

            // Assert
            Assert.IsTrue(summary.Contains("BaseParent"));
            Assert.IsTrue(summary.Contains("兄弟: 3個"));
            Assert.IsTrue(summary.Contains("選択対象: 2個"));
        }

        [Test]
        public void GetSummaryText_InvalidPreview_ReturnsErrorMessage()
        {
            // Arrange
            var errorMessage = "テストエラー";
            var preview = HierarchicalSelectionPreview.Invalid(errorMessage);

            // Act
            var summary = preview.GetSummaryText();

            // Assert
            Assert.AreEqual($"エラー: {errorMessage}", summary);
        }

        [Test]
        public void GetDetailedText_ValidPreview_ReturnsDetailedInformation()
        {
            // Arrange
            var preview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Act
            var detailed = preview.GetDetailedText();

            // Assert
            Assert.IsTrue(detailed.Contains("基準親: BaseParent"));
            Assert.IsTrue(detailed.Contains("兄弟オブジェクト: Sibling1, Sibling2, Sibling3"));
            Assert.IsTrue(detailed.Contains("選択対象: Target1, Target2"));
            Assert.IsTrue(detailed.Contains($"選択理由: {_selectionReason}"));
        }

        [Test]
        public void GetDetailedText_InvalidPreview_ReturnsErrorMessage()
        {
            // Arrange
            var errorMessage = "テストエラー";
            var preview = HierarchicalSelectionPreview.Invalid(errorMessage);

            // Act
            var detailed = preview.GetDetailedText();

            // Assert
            Assert.AreEqual($"エラー: {errorMessage}", detailed);
        }

        #endregion

        #region Equality Tests

        [Test]
        public void Equals_SamePreviewData_ReturnsTrue()
        {
            // Arrange
            var preview1 = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            var preview2 = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Act & Assert
            Assert.IsTrue(preview1.Equals(preview2));
            Assert.IsTrue(preview1 == preview2);
            Assert.IsFalse(preview1 != preview2);
        }

        [Test]
        public void Equals_DifferentBaseParent_ReturnsFalse()
        {
            // Arrange
            var otherBaseParent = new GameObject("OtherBaseParent");
            try
            {
                var preview1 = HierarchicalSelectionPreview.Valid(
                    _baseParent,
                    _parentSiblings,
                    _targetObjects,
                    _selectionReason
                );

                var preview2 = HierarchicalSelectionPreview.Valid(
                    otherBaseParent,
                    _parentSiblings,
                    _targetObjects,
                    _selectionReason
                );

                // Act & Assert
                Assert.IsFalse(preview1.Equals(preview2));
                Assert.IsFalse(preview1 == preview2);
                Assert.IsTrue(preview1 != preview2);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(otherBaseParent);
            }
        }

        [Test]
        public void Equals_DifferentSelectionReason_ReturnsFalse()
        {
            // Arrange
            var preview1 = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            var preview2 = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                "異なる選択理由"
            );

            // Act & Assert
            Assert.IsFalse(preview1.Equals(preview2));
        }

        [Test]
        public void GetHashCode_SamePreviewData_ReturnsSameHashCode()
        {
            // Arrange
            var preview1 = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            var preview2 = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Act & Assert
            Assert.AreEqual(preview1.GetHashCode(), preview2.GetHashCode());
        }

        #endregion

        #region ToString Tests

        [Test]
        public void ToString_ValidPreview_ReturnsValidString()
        {
            // Arrange
            var preview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Act
            var result = preview.ToString();

            // Assert
            Assert.IsTrue(result.Contains("HierarchicalSelectionPreview(Valid:"));
            Assert.IsTrue(result.Contains("BaseParent"));
        }

        [Test]
        public void ToString_InvalidPreview_ReturnsInvalidString()
        {
            // Arrange
            var errorMessage = "テストエラー";
            var preview = HierarchicalSelectionPreview.Invalid(errorMessage);

            // Act
            var result = preview.ToString();

            // Assert
            Assert.IsTrue(result.Contains("HierarchicalSelectionPreview(Invalid:"));
            Assert.IsTrue(result.Contains(errorMessage));
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void Constructor_WithEmptyArrays_HandlesCorrectly()
        {
            // Arrange
            var emptyParentSiblings = new GameObject[0];
            var emptyTargetObjects = new GameObject[0];

            // Act
            var preview = HierarchicalSelectionPreview.Valid(
                _baseParent,
                emptyParentSiblings,
                emptyTargetObjects,
                _selectionReason
            );

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.AreEqual(0, preview.ParentSiblingCount);
            Assert.AreEqual(0, preview.TargetCount);
        }

        [Test]
        public void Constructor_WithNullBaseParent_HandlesCorrectly()
        {
            // Act
            var preview = HierarchicalSelectionPreview.Valid(
                null,
                _parentSiblings,
                _targetObjects,
                _selectionReason
            );

            // Assert
            Assert.IsTrue(preview.IsValid);
            Assert.IsNull(preview.BaseParent);
        }

        #endregion
    }
}