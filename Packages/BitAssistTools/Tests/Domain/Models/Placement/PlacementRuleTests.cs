using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Domain.Models.Transform;

namespace BitAssistTools.Tests.Domain.Models.Placement
{
    [TestFixture]
    public class PlacementRuleTests
    {
        private GameObject[] _testObjects;
        private UnityEngine.Transform[] _testTransforms;

        [SetUp]
        public void SetUp()
        {
            // テスト用オブジェクトを作成
            _testObjects = new GameObject[]
            {
                new GameObject("Object1"),
                new GameObject("Object2"),
                new GameObject("Object3")
            };

            // 異なる位置に配置
            _testObjects[0].transform.position = new Vector3(0, 0, 0);
            _testObjects[1].transform.position = new Vector3(5, 3, 2);
            _testObjects[2].transform.position = new Vector3(-2, 8, -1);

            _testTransforms = new UnityEngine.Transform[]
            {
                _testObjects[0].transform,
                _testObjects[1].transform,
                _testObjects[2].transform
            };
        }

        [TearDown]
        public void TearDown()
        {
            // テストオブジェクトを削除
            foreach (var obj in _testObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesCorrectPlacementRule()
        {
            // Act
            var rule = new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global, true);

            // Assert
            Assert.AreEqual(AlignType.Center, rule.AlignType);
            Assert.AreEqual(Axis.Y, rule.TargetAxis);
            Assert.AreEqual(CoordinateReference.Global, rule.CoordinateReference);
            Assert.IsTrue(rule.PreserveOriginalValues);
        }

        [Test]
        public void Default_ReturnsCorrectDefaultRule()
        {
            // Act
            var defaultRule = PlacementRule.Default;

            // Assert
            Assert.AreEqual(AlignType.Center, defaultRule.AlignType);
            Assert.AreEqual(Axis.Y, defaultRule.TargetAxis);
            Assert.AreEqual(CoordinateReference.Global, defaultRule.CoordinateReference);
            Assert.IsFalse(defaultRule.PreserveOriginalValues);
        }

        [Test]
        public void CalculateBounds_WithValidTransforms_ReturnsCorrectBounds()
        {
            // Act
            var bounds = PlacementRule.CalculateBounds(_testTransforms, CoordinateReference.Global);

            // Assert
            // 期待値: min(-2, 0, -1), max(5, 8, 2), center(1.5, 4, 0.5)
            Assert.AreEqual(new Vector3(-2, 0, -1), bounds.min);
            Assert.AreEqual(new Vector3(5, 8, 2), bounds.max);
            Assert.AreEqual(new Vector3(1.5f, 4f, 0.5f), bounds.center);
        }

        [Test]
        public void CalculateBounds_WithEmptyTransforms_ReturnsEmptyBounds()
        {
            // Act
            var bounds = PlacementRule.CalculateBounds(new UnityEngine.Transform[0], CoordinateReference.Global);

            // Assert
            Assert.AreEqual(new Bounds(), bounds);
        }

        [Test]
        public void CalculateBounds_WithNullTransforms_ReturnsEmptyBounds()
        {
            // Act
            var bounds = PlacementRule.CalculateBounds(null, CoordinateReference.Global);

            // Assert
            Assert.AreEqual(new Bounds(), bounds);
        }

        [Test]
        public void CalculateTargetPosition_WithMaximumAlign_ReturnsMaxPosition()
        {
            // Arrange
            var rule = new PlacementRule(AlignType.Maximum, Axis.Y, CoordinateReference.Global);
            var bounds = PlacementRule.CalculateBounds(_testTransforms, CoordinateReference.Global);

            // Act
            var targetPosition = rule.CalculateTargetPosition(bounds);

            // Assert
            Assert.AreEqual(new Vector3(0, 8, 0), targetPosition); // Y軸の最大値のみ
        }

        [Test]
        public void CalculateTargetPosition_WithCenterAlign_ReturnsCenterPosition()
        {
            // Arrange
            var rule = new PlacementRule(AlignType.Center, Axis.X, CoordinateReference.Global);
            var bounds = PlacementRule.CalculateBounds(_testTransforms, CoordinateReference.Global);

            // Act
            var targetPosition = rule.CalculateTargetPosition(bounds);

            // Assert
            Assert.AreEqual(new Vector3(1.5f, 0, 0), targetPosition); // X軸の中央値のみ
        }

        [Test]
        public void CalculateTargetPosition_WithMinimumAlign_ReturnsMinPosition()
        {
            // Arrange
            var rule = new PlacementRule(AlignType.Minimum, Axis.Z, CoordinateReference.Global);
            var bounds = PlacementRule.CalculateBounds(_testTransforms, CoordinateReference.Global);

            // Act
            var targetPosition = rule.CalculateTargetPosition(bounds);

            // Assert
            Assert.AreEqual(new Vector3(0, 0, -1), targetPosition); // Z軸の最小値のみ
        }

        [Test]
        public void GetAxisValue_ReturnsCorrectAxisValue()
        {
            // Arrange
            var rule = PlacementRule.Default;
            var vector = new Vector3(1, 2, 3);

            // Act & Assert
            Assert.AreEqual(1f, rule.GetAxisValue(vector, Axis.X));
            Assert.AreEqual(2f, rule.GetAxisValue(vector, Axis.Y));
            Assert.AreEqual(3f, rule.GetAxisValue(vector, Axis.Z));
        }

        [Test]
        public void SetAxisValue_SetsCorrectAxisValue()
        {
            // Arrange
            var rule = PlacementRule.Default;
            var original = new Vector3(1, 2, 3);

            // Act & Assert
            Assert.AreEqual(new Vector3(10, 2, 3), rule.SetAxisValue(original, Axis.X, 10));
            Assert.AreEqual(new Vector3(1, 20, 3), rule.SetAxisValue(original, Axis.Y, 20));
            Assert.AreEqual(new Vector3(1, 2, 30), rule.SetAxisValue(original, Axis.Z, 30));
        }

        [Test]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var rule1 = new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global, true);
            var rule2 = new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global, true);

            // Act & Assert
            Assert.IsTrue(rule1.Equals(rule2));
            Assert.IsTrue(rule1 == rule2);
            Assert.IsFalse(rule1 != rule2);
        }

        [Test]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var rule1 = new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global, true);
            var rule2 = new PlacementRule(AlignType.Maximum, Axis.Y, CoordinateReference.Global, true);

            // Act & Assert
            Assert.IsFalse(rule1.Equals(rule2));
            Assert.IsFalse(rule1 == rule2);
            Assert.IsTrue(rule1 != rule2);
        }

        [Test]
        public void GetHashCode_WithSameValues_ReturnsSameHashCode()
        {
            // Arrange
            var rule1 = new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global, true);
            var rule2 = new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global, true);

            // Act & Assert
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var rule = new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global, true);

            // Act
            var result = rule.ToString();

            // Assert
            Assert.IsTrue(result.Contains("PlacementRule"));
            Assert.IsTrue(result.Contains("Center"));
            Assert.IsTrue(result.Contains("Y"));
            Assert.IsTrue(result.Contains("Global"));
            Assert.IsTrue(result.Contains("True"));
        }
    }
}