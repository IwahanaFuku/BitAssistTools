using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Lighting;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class LightingServiceTests
    {
        private GameObject _lightObject1;
        private GameObject _lightObject2;
        private GameObject _nonLightObject;
        private Light _light1;
        private Light _light2;

        [SetUp]
        public void SetUp()
        {
            // テスト用のLightオブジェクトを作成
            _lightObject1 = new GameObject("LightObject1");
            _lightObject2 = new GameObject("LightObject2");
            _nonLightObject = new GameObject("NonLightObject");

            _light1 = _lightObject1.AddComponent<Light>();
            _light2 = _lightObject2.AddComponent<Light>();

            // 初期強度を設定
            _light1.intensity = 1.0f;
            _light2.intensity = 2.0f;
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_lightObject1 != null) UnityEngine.Object.DestroyImmediate(_lightObject1);
            if (_lightObject2 != null) UnityEngine.Object.DestroyImmediate(_lightObject2);
            if (_nonLightObject != null) UnityEngine.Object.DestroyImmediate(_nonLightObject);

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void ApplyLightIntensityOperation_WithValidRule_ChangesLightIntensity()
        {
            // Arrange
            var rule = new LightOperationRule(0.5f, ArithmeticOperation.Addition);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1 };

            // Act
            LightingService.ApplyLightIntensityOperation(rule);

            // Assert
            Assert.AreEqual(1.5f, _light1.intensity, 0.001f);
        }

        [Test]
        public void ApplyLightIntensityOperation_WithMultipleLights_ChangesAllLights()
        {
            // Arrange
            var rule = new LightOperationRule(2.0f, ArithmeticOperation.Multiplication);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1, _lightObject2 };

            // Act
            LightingService.ApplyLightIntensityOperation(rule);

            // Assert
            Assert.AreEqual(2.0f, _light1.intensity, 0.001f); // 1.0 * 2.0
            Assert.AreEqual(4.0f, _light2.intensity, 0.001f); // 2.0 * 2.0
        }

        [Test]
        public void ApplyLightIntensityOperation_WithNegativeResult_ClampsToZero()
        {
            // Arrange
            var rule = new LightOperationRule(-2.0f, ArithmeticOperation.Addition);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1 };

            // Act
            LightingService.ApplyLightIntensityOperation(rule);

            // Assert
            Assert.AreEqual(0.0f, _light1.intensity, 0.001f); // max(1.0 + (-2.0), 0.0) = 0.0
        }

        [Test]
        public void ApplyLightIntensityOperation_WithInvalidRule_LogsError()
        {
            // Arrange
            var rule = new LightOperationRule(float.NaN, ArithmeticOperation.Addition);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1 };

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("無効なライト操作ルールです"));
            LightingService.ApplyLightIntensityOperation(rule);

            // 無効なルールでは強度が変更されないことを確認
            Assert.AreEqual(1.0f, _light1.intensity, 0.001f);
        }

        [Test]
        public void ApplyLightIntensityOperation_WithNoSelection_LogsError()
        {
            // Arrange
            var rule = new LightOperationRule(1.0f, ArithmeticOperation.Addition);
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("ライトコンポーネントを持つGameObjectを選択してください"));
            LightingService.ApplyLightIntensityOperation(rule);
        }

        [Test]
        public void ApplyLightIntensityOperation_WithNonLightObjects_LogsWarning()
        {
            // Arrange
            var rule = new LightOperationRule(1.0f, ArithmeticOperation.Addition);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _nonLightObject };

            // Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("選択されたオブジェクトにLightコンポーネントが見つかりません"));
            LightingService.ApplyLightIntensityOperation(rule);
        }

        [Test]
        public void ApplyLightIntensityOperation_WithMixedObjects_ChangesOnlyLights()
        {
            // Arrange
            var rule = new LightOperationRule(1.0f, ArithmeticOperation.Addition);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1, _nonLightObject, _lightObject2 };

            // Act
            LightingService.ApplyLightIntensityOperation(rule);

            // Assert
            Assert.AreEqual(2.0f, _light1.intensity, 0.001f); // 1.0 + 1.0
            Assert.AreEqual(3.0f, _light2.intensity, 0.001f); // 2.0 + 1.0
        }

        [Test]
        public void CanApplyLightOperation_WithLightObjects_ReturnsTrue()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1 };

            // Act
            bool result = LightingService.CanApplyLightOperation();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanApplyLightOperation_WithNonLightObjects_ReturnsFalse()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _nonLightObject };

            // Act
            bool result = LightingService.CanApplyLightOperation();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CanApplyLightOperation_WithMixedObjects_ReturnsTrue()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1, _nonLightObject };

            // Act
            bool result = LightingService.CanApplyLightOperation();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanApplyLightOperation_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act
            bool result = LightingService.CanApplyLightOperation();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetSelectedLightCount_WithLightObjects_ReturnsCorrectCount()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1, _lightObject2 };

            // Act
            int count = LightingService.GetSelectedLightCount();

            // Assert
            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetSelectedLightCount_WithMixedObjects_ReturnsLightCount()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1, _nonLightObject, _lightObject2 };

            // Act
            int count = LightingService.GetSelectedLightCount();

            // Assert
            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetSelectedLightCount_WithNoLights_ReturnsZero()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _nonLightObject };

            // Act
            int count = LightingService.GetSelectedLightCount();

            // Assert
            Assert.AreEqual(0, count);
        }

        [Test]
        public void GetLightIntensityStats_WithLightObjects_ReturnsCorrectStats()
        {
            // Arrange
            _light1.intensity = 1.0f;
            _light2.intensity = 3.0f;
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _lightObject1, _lightObject2 };

            // Act
            var stats = LightingService.GetLightIntensityStats();

            // Assert
            Assert.AreEqual(2, stats.Count);
            Assert.AreEqual(1.0f, stats.MinIntensity, 0.001f);
            Assert.AreEqual(3.0f, stats.MaxIntensity, 0.001f);
            Assert.AreEqual(2.0f, stats.AverageIntensity, 0.001f);
            Assert.IsTrue(stats.HasLights);
        }

        [Test]
        public void GetLightIntensityStats_WithNoLights_ReturnsEmptyStats()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _nonLightObject };

            // Act
            var stats = LightingService.GetLightIntensityStats();

            // Assert
            Assert.AreEqual(0, stats.Count);
            Assert.AreEqual(0.0f, stats.MinIntensity, 0.001f);
            Assert.AreEqual(0.0f, stats.MaxIntensity, 0.001f);
            Assert.AreEqual(0.0f, stats.AverageIntensity, 0.001f);
            Assert.IsFalse(stats.HasLights);
        }

        [Test]
        public void GetLightIntensityStats_WithNoSelection_ReturnsEmptyStats()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act
            var stats = LightingService.GetLightIntensityStats();

            // Assert
            Assert.AreEqual(0, stats.Count);
            Assert.IsFalse(stats.HasLights);
        }

        [Test]
        public void LightIntensityStats_ToString_WithLights_ReturnsFormattedString()
        {
            // Arrange
            var stats = new LightingService.LightIntensityStats(2, 1.0f, 3.0f, 2.0f);

            // Act
            var result = stats.ToString();

            // Assert
            Assert.IsTrue(result.Contains("2個のライト"));
            Assert.IsTrue(result.Contains("1.00～3.00"));
            Assert.IsTrue(result.Contains("平均: 2.00"));
        }

        [Test]
        public void LightIntensityStats_ToString_WithoutLights_ReturnsNoLightMessage()
        {
            // Arrange
            var stats = new LightingService.LightIntensityStats(0, 0.0f, 0.0f, 0.0f);

            // Act
            var result = stats.ToString();

            // Assert
            Assert.AreEqual("ライトなし", result);
        }

        [Test]
        public void LightIntensityStats_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var stats = new LightingService.LightIntensityStats(3, 0.5f, 2.5f, 1.5f);

            // Assert
            Assert.AreEqual(3, stats.Count);
            Assert.AreEqual(0.5f, stats.MinIntensity, 0.001f);
            Assert.AreEqual(2.5f, stats.MaxIntensity, 0.001f);
            Assert.AreEqual(1.5f, stats.AverageIntensity, 0.001f);
            Assert.IsTrue(stats.HasLights);
        }
    }
}