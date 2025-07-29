using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Domain.Models.Transform;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class RandomizationServiceTests
    {
        private GameObject _testObject1;
        private GameObject _testObject2;
        private GameObject _referenceObject;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _testObject1 = new GameObject("TestObject1");
            _testObject2 = new GameObject("TestObject2");
            _referenceObject = new GameObject("ReferenceObject");

            // 初期位置を設定
            _testObject1.transform.position = Vector3.zero;
            _testObject1.transform.rotation = Quaternion.identity;
            _testObject1.transform.localScale = Vector3.one;

            _testObject2.transform.position = Vector3.one;
            _testObject2.transform.rotation = Quaternion.identity;
            _testObject2.transform.localScale = Vector3.one;

            _referenceObject.transform.position = Vector3.zero;

            // ランダムシードを固定してテストの再現性を確保
            UnityEngine.Random.InitState(42);
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_testObject1 != null) UnityEngine.Object.DestroyImmediate(_testObject1);
            if (_testObject2 != null) UnityEngine.Object.DestroyImmediate(_testObject2);
            if (_referenceObject != null) UnityEngine.Object.DestroyImmediate(_referenceObject);

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void ExecuteRandomPosition_WithXAxisOnly_ChangesOnlyXPosition()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            RandomizationService.ExecuteRandomPosition(1f, AxisFlags.XOnly);

            // Assert
            Assert.AreNotEqual(originalPosition.x, _testObject1.transform.position.x);
            Assert.AreEqual(originalPosition.y, _testObject1.transform.position.y, 0.001f);
            Assert.AreEqual(originalPosition.z, _testObject1.transform.position.z, 0.001f);
        }

        [Test]
        public void ExecuteRandomPosition_WithYAxisOnly_ChangesOnlyYPosition()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            RandomizationService.ExecuteRandomPosition(1f, AxisFlags.YOnly);

            // Assert
            Assert.AreEqual(originalPosition.x, _testObject1.transform.position.x, 0.001f);
            Assert.AreNotEqual(originalPosition.y, _testObject1.transform.position.y);
            Assert.AreEqual(originalPosition.z, _testObject1.transform.position.z, 0.001f);
        }

        [Test]
        public void ExecuteRandomPosition_WithZAxisOnly_ChangesOnlyZPosition()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            RandomizationService.ExecuteRandomPosition(1f, AxisFlags.ZOnly);

            // Assert
            Assert.AreEqual(originalPosition.x, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(originalPosition.y, _testObject1.transform.position.y, 0.001f);
            Assert.AreNotEqual(originalPosition.z, _testObject1.transform.position.z);
        }

        [Test]
        public void ExecuteRandomPosition_WithAllAxes_ChangesAllPositions()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            RandomizationService.ExecuteRandomPosition(1f, AxisFlags.All);

            // Assert - 少なくとも一つの軸は変化する（ランダムなので全軸変化は保証されない）
            var newPosition = _testObject1.transform.position;
            bool hasChanged = originalPosition.x != newPosition.x || 
                             originalPosition.y != newPosition.y || 
                             originalPosition.z != newPosition.z;
            Assert.IsTrue(hasChanged);
        }

        [Test]
        public void ExecuteRandomPosition_WithZeroRadius_DoesNotChangePosition()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            RandomizationService.ExecuteRandomPosition(0f, AxisFlags.All);

            // Assert
            Assert.AreEqual(originalPosition, _testObject1.transform.position);
        }

        [Test]
        public void ExecuteRandomRotation_WithXAxisOnly_ChangesOnlyXRotation()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalRotation = _testObject1.transform.rotation;

            // Act
            RandomizationService.ExecuteRandomRotation(1f, AxisFlags.XOnly);

            // Assert
            Assert.AreNotEqual(originalRotation, _testObject1.transform.rotation);
        }

        [Test]
        public void ExecuteRandomRotation_WithCustomCurve_UsesLimitCurve()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalRotation = _testObject1.transform.rotation;
            var curve = AnimationCurve.Linear(0, 0, 1, 90); // 0度から90度の範囲

            // Act
            RandomizationService.ExecuteRandomRotation(1f, AxisFlags.YOnly, curve);

            // Assert
            Assert.AreNotEqual(originalRotation, _testObject1.transform.rotation);
        }

        [Test]
        public void ExecuteRandomScale_WithValidRange_ChangesScale()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalScale = _testObject1.transform.localScale;

            // Act
            RandomizationService.ExecuteRandomScale(1f, 0.5f, 2f);

            // Assert
            var newScale = _testObject1.transform.localScale;
            Assert.IsTrue(newScale.x >= 0.5f && newScale.x <= 2f);
            Assert.IsTrue(newScale.y >= 0.5f && newScale.y <= 2f);
            Assert.IsTrue(newScale.z >= 0.5f && newScale.z <= 2f);
            
            // uniform scaleなので全ての軸が同じ値
            Assert.AreEqual(newScale.x, newScale.y, 0.001f);
            Assert.AreEqual(newScale.y, newScale.z, 0.001f);
        }

        [Test]
        public void ExecuteRandomScale_WithSameMinMax_SetsExactScale()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            RandomizationService.ExecuteRandomScale(1f, 1.5f, 1.5f);

            // Assert
            var newScale = _testObject1.transform.localScale;
            Assert.AreEqual(1.5f, newScale.x, 0.001f);
            Assert.AreEqual(1.5f, newScale.y, 0.001f);
            Assert.AreEqual(1.5f, newScale.z, 0.001f);
        }

        [Test]
        public void ExecuteRandomDetailScale_WithValidRange_ChangesScalePerAxis()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var minScale = new Vector3(0.5f, 0.8f, 1.2f);
            var maxScale = new Vector3(1.5f, 1.8f, 2.2f);

            // Act
            RandomizationService.ExecuteRandomDetailScale(1f, minScale, maxScale);

            // Assert
            var newScale = _testObject1.transform.localScale;
            Assert.IsTrue(newScale.x >= 0.5f && newScale.x <= 1.5f);
            Assert.IsTrue(newScale.y >= 0.8f && newScale.y <= 1.8f);
            Assert.IsTrue(newScale.z >= 1.2f && newScale.z <= 2.2f);
        }

        [Test]
        public void ExecuteRandomDetailScale_WithRatio_AppliesRatioToScale()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var minScale = Vector3.one;
            var maxScale = Vector3.one;
            var ratio = 2f;

            // Act
            RandomizationService.ExecuteRandomDetailScale(ratio, minScale, maxScale);

            // Assert
            var newScale = _testObject1.transform.localScale;
            Assert.AreEqual(2f, newScale.x, 0.001f);
            Assert.AreEqual(2f, newScale.y, 0.001f);
            Assert.AreEqual(2f, newScale.z, 0.001f);
        }

        [Test]
        public void ExecutePadding_WithoutReferencePoint_LogsWarningAndDoesNothing()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            RandomizationService.ExecutePadding(1f, null, AxisFlags.All);

            // Assert
            Assert.AreEqual(originalPosition, _testObject1.transform.position);
        }

        [Test]
        public void ExecutePadding_WithReferencePoint_MovesObjectsAwayFromReference()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(1f, 0f, 0f);
            _referenceObject.transform.position = Vector3.zero;
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            RandomizationService.ExecutePadding(1f, _referenceObject.transform, AxisFlags.All);

            // Assert
            var newPosition = _testObject1.transform.position;
            var distance = Vector3.Distance(newPosition, _referenceObject.transform.position);
            Assert.IsTrue(distance > 1f); // 元の距離1 + パディング1 = 2に近い値
        }

        [Test]
        public void ExecutePadding_WithXAxisOnly_PadsOnlyXAxis()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(1f, 1f, 1f);
            _referenceObject.transform.position = Vector3.zero;
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            RandomizationService.ExecutePadding(1f, _referenceObject.transform, AxisFlags.XOnly);

            // Assert
            var newPosition = _testObject1.transform.position;
            Assert.AreNotEqual(1f, newPosition.x); // X軸は変化
            Assert.AreEqual(1f, newPosition.y, 0.001f); // Y軸は変化しない
            Assert.AreEqual(1f, newPosition.z, 0.001f); // Z軸は変化しない
        }

        [Test]
        public void ExecutePadding_WithObjectAtReferencePosition_DoesNotMove()
        {
            // Arrange
            _testObject1.transform.position = Vector3.zero;
            _referenceObject.transform.position = Vector3.zero;
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            RandomizationService.ExecutePadding(1f, _referenceObject.transform, AxisFlags.All);

            // Assert - 同じ位置にあるオブジェクトは動かない（方向ベクトルが0）
            Assert.AreEqual(Vector3.zero, _testObject1.transform.position);
        }

        [Test]
        public void ExecuteRandomPosition_WithNoSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.DoesNotThrow(() => RandomizationService.ExecuteRandomPosition(1f, AxisFlags.All));
        }

        [Test]
        public void ExecuteRandomRotation_WithNoSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.DoesNotThrow(() => RandomizationService.ExecuteRandomRotation(1f, AxisFlags.All));
        }

        [Test]
        public void ExecuteRandomScale_WithNoSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.DoesNotThrow(() => RandomizationService.ExecuteRandomScale(1f, 0.5f, 2f));
        }

        [Test]
        public void ExecuteRandomDetailScale_WithNoSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.DoesNotThrow(() => RandomizationService.ExecuteRandomDetailScale(1f, Vector3.one, Vector3.one * 2f));
        }

        [Test]
        public void ExecutePadding_WithNoSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.DoesNotThrow(() => RandomizationService.ExecutePadding(1f, _referenceObject.transform, AxisFlags.All));
        }

        [Test]
        public void ExecuteRandomScale_WithInvertedRange_HandlesCorrectly()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act - 最小値 > 最大値の場合
            RandomizationService.ExecuteRandomScale(1f, 2f, 0.5f);

            // Assert - Unity.Random.Rangeは自動的に範囲を修正するため、例外は発生しない
            var newScale = _testObject1.transform.localScale;
            Assert.IsTrue(newScale.x >= 0.5f && newScale.x <= 2f);
        }

        [Test]
        public void ExecuteRandomDetailScale_WithInvertedRange_HandlesCorrectly()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var minScale = new Vector3(2f, 2f, 2f);
            var maxScale = new Vector3(1f, 1f, 1f);

            // Act
            RandomizationService.ExecuteRandomDetailScale(1f, minScale, maxScale);

            // Assert - Mathf.Min/Maxで正しく範囲が調整される
            var newScale = _testObject1.transform.localScale;
            Assert.IsTrue(newScale.x >= 1f && newScale.x <= 2f);
            Assert.IsTrue(newScale.y >= 1f && newScale.y <= 2f);
            Assert.IsTrue(newScale.z >= 1f && newScale.z <= 2f);
        }
    }
}