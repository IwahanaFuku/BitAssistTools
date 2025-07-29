using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class TransformCalculationServiceTests
    {
        private GameObject _testObject1;
        private GameObject _testObject2;
        private GameObject _testObject3;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _testObject1 = new GameObject("TestObject1");
            _testObject2 = new GameObject("TestObject2");
            _testObject3 = new GameObject("TestObject3");

            // 初期位置を設定
            _testObject1.transform.position = new Vector3(1.123f, 2.456f, 3.789f);
            _testObject1.transform.rotation = Quaternion.Euler(10.5f, 20.7f, 30.9f);
            _testObject1.transform.localScale = new Vector3(1.111f, 2.222f, 3.333f);

            _testObject2.transform.position = new Vector3(5f, 0f, 0f);
            _testObject3.transform.position = new Vector3(10f, 0f, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_testObject1 != null) UnityEngine.Object.DestroyImmediate(_testObject1);
            if (_testObject2 != null) UnityEngine.Object.DestroyImmediate(_testObject2);
            if (_testObject3 != null) UnityEngine.Object.DestroyImmediate(_testObject3);

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void ExecuteRounding_WithPositionFlag_RoundsPosition()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            TransformCalculationService.ExecuteRounding(2, TransformFlags.Position);

            // Assert
            Assert.AreEqual(1.12f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(2.46f, _testObject1.transform.position.y, 0.001f);
            Assert.AreEqual(3.79f, _testObject1.transform.position.z, 0.001f);
            
            // 回転とスケールは変化しないことを確認
            Assert.AreEqual(10.5f, _testObject1.transform.rotation.eulerAngles.x, 0.1f);
            Assert.AreEqual(1.111f, _testObject1.transform.localScale.x, 0.001f);
        }

        [Test]
        public void ExecuteRounding_WithRotationFlag_RoundsRotation()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            TransformCalculationService.ExecuteRounding(1, TransformFlags.Rotation);

            // Assert
            var euler = _testObject1.transform.rotation.eulerAngles;
            Assert.AreEqual(10.5f, euler.x, 0.1f);
            Assert.AreEqual(20.7f, euler.y, 0.1f);
            Assert.AreEqual(30.9f, euler.z, 0.1f);
            
            // 位置とスケールは変化しないことを確認
            Assert.AreEqual(1.123f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(1.111f, _testObject1.transform.localScale.x, 0.001f);
        }

        [Test]
        public void ExecuteRounding_WithScaleFlag_RoundsScale()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            TransformCalculationService.ExecuteRounding(2, TransformFlags.Scale);

            // Assert
            Assert.AreEqual(1.11f, _testObject1.transform.localScale.x, 0.001f);
            Assert.AreEqual(2.22f, _testObject1.transform.localScale.y, 0.001f);
            Assert.AreEqual(3.33f, _testObject1.transform.localScale.z, 0.001f);
            
            // 位置と回転は変化しないことを確認
            Assert.AreEqual(1.123f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(10.5f, _testObject1.transform.rotation.eulerAngles.x, 0.1f);
        }

        [Test]
        public void ExecuteRounding_WithAllFlags_RoundsAllProperties()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            TransformCalculationService.ExecuteRounding(1, TransformFlags.All);

            // Assert
            Assert.AreEqual(1.1f, _testObject1.transform.position.x, 0.01f);
            Assert.AreEqual(2.5f, _testObject1.transform.position.y, 0.01f);
            Assert.AreEqual(3.8f, _testObject1.transform.position.z, 0.01f);
            
            Assert.AreEqual(1.1f, _testObject1.transform.localScale.x, 0.01f);
            Assert.AreEqual(2.2f, _testObject1.transform.localScale.y, 0.01f);
            Assert.AreEqual(3.3f, _testObject1.transform.localScale.z, 0.01f);
        }

        [Test]
        public void ExecuteRounding_WithZeroDecimalPlaces_RoundsToInteger()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            TransformCalculationService.ExecuteRounding(0, TransformFlags.Position);

            // Assert
            Assert.AreEqual(1f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(2f, _testObject1.transform.position.y, 0.001f);
            Assert.AreEqual(4f, _testObject1.transform.position.z, 0.001f);
        }

        [Test]
        public void ExecuteTransformChange_WithGlobalCoordinate_ChangesGlobally()
        {
            // Arrange
            var originalPosition = _testObject1.transform.position;
            var change = new TransformData(Vector3.one, Quaternion.identity, Vector3.zero);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            TransformCalculationService.ExecuteTransformChange(CoordinateSystem.Global, change);

            // Assert
            var expectedPosition = originalPosition + Vector3.one;
            Assert.AreEqual(expectedPosition.x, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(expectedPosition.y, _testObject1.transform.position.y, 0.001f);
            Assert.AreEqual(expectedPosition.z, _testObject1.transform.position.z, 0.001f);
        }

        [Test]
        public void ExecuteTransformChange_WithLocalCoordinate_ChangesLocally()
        {
            // Arrange
            var originalPosition = _testObject1.transform.position;
            var change = new TransformData(Vector3.right, Quaternion.identity, Vector3.zero);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            TransformCalculationService.ExecuteTransformChange(CoordinateSystem.Local, change);

            // Assert
            // ローカル座標での変更が適用されていることを確認
            Assert.AreNotEqual(originalPosition, _testObject1.transform.position);
        }

        [Test]
        public void ExecuteDistribute_WithTwoObjects_DoesNotChangePositions()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };
            var originalPos1 = _testObject1.transform.position;
            var originalPos2 = _testObject2.transform.position;

            // Act
            TransformCalculationService.ExecuteDistribute(Axis.X);

            // Assert - 2つのオブジェクトの場合は端点なので変更されない
            Assert.AreEqual(originalPos1, _testObject1.transform.position);
            Assert.AreEqual(originalPos2, _testObject2.transform.position);
        }

        [Test]
        public void ExecuteDistribute_WithThreeObjectsOnXAxis_DistributesEvenly()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(0f, 0f, 0f);
            _testObject2.transform.position = new Vector3(5f, 0f, 0f);
            _testObject3.transform.position = new Vector3(10f, 0f, 0f);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2, _testObject3 };

            // Act
            TransformCalculationService.ExecuteDistribute(Axis.X);

            // Assert - 中央のオブジェクトが等間隔に配置される
            Assert.AreEqual(0f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(5f, _testObject2.transform.position.x, 0.001f);
            Assert.AreEqual(10f, _testObject3.transform.position.x, 0.001f);
        }

        [Test]
        public void ExecuteDistribute_WithOneObject_DoesNothing()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            TransformCalculationService.ExecuteDistribute(Axis.X);

            // Assert
            Assert.AreEqual(originalPosition, _testObject1.transform.position);
        }

        [Test]
        public void ExecuteAlignment_WithCenterAlignment_AlignsToCenter()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(0f, 0f, 0f);
            _testObject2.transform.position = new Vector3(10f, 0f, 0f);
            _testObject3.transform.position = new Vector3(5f, 0f, 0f);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2, _testObject3 };

            // Act
            TransformCalculationService.ExecuteAlignment(Axis.X, AlignmentType.Center);

            // Assert - 全てのオブジェクトが中央値(5)に整列される
            Assert.AreEqual(5f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(5f, _testObject2.transform.position.x, 0.001f);
            Assert.AreEqual(5f, _testObject3.transform.position.x, 0.001f);
        }

        [Test]
        public void ExecuteAlignment_WithMinimumAlignment_AlignsToMinimum()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(0f, 0f, 0f);
            _testObject2.transform.position = new Vector3(10f, 0f, 0f);
            _testObject3.transform.position = new Vector3(5f, 0f, 0f);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2, _testObject3 };

            // Act
            TransformCalculationService.ExecuteAlignment(Axis.X, AlignmentType.Minimum);

            // Assert - 全てのオブジェクトが最小値(0)に整列される
            Assert.AreEqual(0f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(0f, _testObject2.transform.position.x, 0.001f);
            Assert.AreEqual(0f, _testObject3.transform.position.x, 0.001f);
        }

        [Test]
        public void ExecuteAlignment_WithMaximumAlignment_AlignsToMaximum()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(0f, 0f, 0f);
            _testObject2.transform.position = new Vector3(10f, 0f, 0f);
            _testObject3.transform.position = new Vector3(5f, 0f, 0f);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2, _testObject3 };

            // Act
            TransformCalculationService.ExecuteAlignment(Axis.X, AlignmentType.Maximum);

            // Assert - 全てのオブジェクトが最大値(10)に整列される
            Assert.AreEqual(10f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(10f, _testObject2.transform.position.x, 0.001f);
            Assert.AreEqual(10f, _testObject3.transform.position.x, 0.001f);
        }

        [Test]
        public void ExecuteAlignment_WithYAxis_AlignsOnYAxis()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(0f, 1f, 0f);
            _testObject2.transform.position = new Vector3(0f, 5f, 0f);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            TransformCalculationService.ExecuteAlignment(Axis.Y, AlignmentType.Center);

            // Assert - Y軸で中央値(3)に整列される
            Assert.AreEqual(3f, _testObject1.transform.position.y, 0.001f);
            Assert.AreEqual(3f, _testObject2.transform.position.y, 0.001f);
            
            // X,Z軸は変化しない
            Assert.AreEqual(0f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(0f, _testObject2.transform.position.x, 0.001f);
        }

        [Test]
        public void ExecuteAlignment_WithZAxis_AlignsOnZAxis()
        {
            // Arrange
            _testObject1.transform.position = new Vector3(0f, 0f, 2f);
            _testObject2.transform.position = new Vector3(0f, 0f, 8f);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            TransformCalculationService.ExecuteAlignment(Axis.Z, AlignmentType.Center);

            // Assert - Z軸で中央値(5)に整列される
            Assert.AreEqual(5f, _testObject1.transform.position.z, 0.001f);
            Assert.AreEqual(5f, _testObject2.transform.position.z, 0.001f);
            
            // X,Y軸は変化しない
            Assert.AreEqual(0f, _testObject1.transform.position.x, 0.001f);
            Assert.AreEqual(0f, _testObject1.transform.position.y, 0.001f);
        }

        [Test]
        public void ExecuteAlignment_WithOneObject_DoesNothing()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };
            var originalPosition = _testObject1.transform.position;

            // Act
            TransformCalculationService.ExecuteAlignment(Axis.X, AlignmentType.Center);

            // Assert
            Assert.AreEqual(originalPosition, _testObject1.transform.position);
        }

        [Test]
        public void ExecuteRounding_WithNoSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.DoesNotThrow(() => TransformCalculationService.ExecuteRounding(2, TransformFlags.All));
        }

        [Test]
        public void ExecuteTransformChange_WithNoSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
            var change = new TransformData(Vector3.zero, Quaternion.identity, Vector3.zero);

            // Act & Assert
            Assert.DoesNotThrow(() => TransformCalculationService.ExecuteTransformChange(CoordinateSystem.Global, change));
        }

        [Test]
        public void ExecuteRounding_WithNegativeDecimalPlaces_HandlesCorrectly()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act & Assert - 負の小数点桁数でも例外が発生しないことを確認
            Assert.DoesNotThrow(() => TransformCalculationService.ExecuteRounding(-1, TransformFlags.Position));
        }
    }
}