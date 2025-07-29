
using System;
using NUnit.Framework;
using BitAssistTools.Domain.Models.Lighting;

namespace BitAssistTools.Tests.Domain.Models.Lighting
{
    [TestFixture]
    public class LightOperationRuleTests
    {
        [Test]
        public void ApplyTo_WithAddition_ReturnsCorrectValue()
        {
            // Arrange
            var rule = new LightOperationRule(1.5f, ArithmeticOperation.Addition);
            var currentIntensity = 2.0f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(3.5f, newIntensity);
        }

        [Test]
        public void ApplyTo_WithMultiplication_ReturnsCorrectValue()
        {
            // Arrange
            var rule = new LightOperationRule(2.0f, ArithmeticOperation.Multiplication);
            var currentIntensity = 3.0f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(6.0f, newIntensity);
        }

        [Test]
        public void IsValid_WithValidNumber_ReturnsTrue()
        {
            // Arrange
            var rule = new LightOperationRule(1.0f, ArithmeticOperation.Addition);

            // Act & Assert
            Assert.IsTrue(rule.IsValid());
        }

        [Test]
        public void IsValid_WithZeroForMultiplication_ReturnsTrue()
        {
            // Arrange
            var rule = new LightOperationRule(0.0f, ArithmeticOperation.Multiplication);

            // Act & Assert
            Assert.IsTrue(rule.IsValid());
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesCorrectRule()
        {
            // Arrange & Act
            var rule = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);

            // Assert
            Assert.AreEqual(2.5f, rule.IntensityValue);
            Assert.AreEqual(ArithmeticOperation.Multiplication, rule.Operation);
        }

        [Test]
        public void ApplyTo_WithNegativeAddition_ReturnsCorrectValue()
        {
            // Arrange (負の値を加算することで減算と同等の効果)
            var rule = new LightOperationRule(-1.5f, ArithmeticOperation.Addition);
            var currentIntensity = 3.0f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(1.5f, newIntensity);
        }

        [Test]
        public void ApplyTo_WithFractionalMultiplication_ReturnsCorrectValue()
        {
            // Arrange (0.5を掛けることで2で割ることと同等の効果)
            var rule = new LightOperationRule(0.5f, ArithmeticOperation.Multiplication);
            var currentIntensity = 6.0f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(3.0f, newIntensity);
        }

        [Test]
        public void ApplyTo_WithZeroMultiplication_ReturnsZero()
        {
            // Arrange
            var rule = new LightOperationRule(0.0f, ArithmeticOperation.Multiplication);
            var currentIntensity = 5.0f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(0.0f, newIntensity);
        }

        [Test]
        public void ApplyTo_WithNegativeValues_HandlesCorrectly()
        {
            // Arrange
            var rule = new LightOperationRule(-2.0f, ArithmeticOperation.Addition);
            var currentIntensity = 5.0f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(3.0f, newIntensity);
        }

        [Test]
        public void ApplyTo_WithFloatingPointPrecision_HandlesCorrectly()
        {
            // Arrange
            var rule = new LightOperationRule(0.1f, ArithmeticOperation.Addition);
            var currentIntensity = 0.2f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(0.3f, newIntensity, 0.0001f);
        }

        [Test]
        public void ApplyTo_WithExtremeValues_HandlesCorrectly()
        {
            // Arrange
            var rule = new LightOperationRule(float.MaxValue, ArithmeticOperation.Addition);
            var currentIntensity = 1.0f;

            // Act
            var newIntensity = rule.ApplyTo(currentIntensity);

            // Assert
            Assert.AreEqual(float.MaxValue, newIntensity);
        }

        [Test]
        public void ApplyTo_WithInvalidOperation_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var rule = new LightOperationRule(2.0f, (ArithmeticOperation)999);
            var currentIntensity = 5.0f;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => rule.ApplyTo(currentIntensity));
        }

        [Test]
        public void Constructor_WithNaNValue_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                new LightOperationRule(float.NaN, ArithmeticOperation.Addition));
        }

        [Test]
        public void Constructor_WithInfinityValue_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                new LightOperationRule(float.PositiveInfinity, ArithmeticOperation.Addition));
            Assert.Throws<ArgumentException>(() => 
                new LightOperationRule(float.NegativeInfinity, ArithmeticOperation.Addition));
        }

        [Test]
        public void Default_ReturnsRuleWithOneAddition()
        {
            // Act
            var defaultRule = LightOperationRule.Default;

            // Assert
            Assert.AreEqual(1.0f, defaultRule.IntensityValue);
            Assert.AreEqual(ArithmeticOperation.Addition, defaultRule.Operation);
            Assert.IsTrue(defaultRule.IsValid());
        }

        [Test]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var rule1 = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);
            var rule2 = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);

            // Act & Assert
            Assert.IsTrue(rule1.Equals(rule2));
            Assert.IsTrue(rule1 == rule2);
            Assert.IsFalse(rule1 != rule2);
        }

        [Test]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var rule1 = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);
            var rule2 = new LightOperationRule(3.0f, ArithmeticOperation.Multiplication);

            // Act & Assert
            Assert.IsFalse(rule1.Equals(rule2));
            Assert.IsFalse(rule1 == rule2);
            Assert.IsTrue(rule1 != rule2);
        }

        [Test]
        public void Equals_WithObjectType_HandlesCorrectly()
        {
            // Arrange
            var rule = new LightOperationRule(2.5f, ArithmeticOperation.Addition);
            object otherRule = new LightOperationRule(2.5f, ArithmeticOperation.Addition);
            object differentObject = "not a light operation rule";

            // Act & Assert
            Assert.IsTrue(rule.Equals(otherRule));
            Assert.IsFalse(rule.Equals(differentObject));
            Assert.IsFalse(rule.Equals(null));
        }

        [Test]
        public void GetHashCode_WithSameValues_ReturnsSameHashCode()
        {
            // Arrange
            var rule1 = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);
            var rule2 = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);

            // Act & Assert
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_WithDifferentValues_ReturnsDifferentHashCodes()
        {
            // Arrange
            var rule1 = new LightOperationRule(2.5f, ArithmeticOperation.Addition);
            var rule2 = new LightOperationRule(3.0f, ArithmeticOperation.Addition);
            var rule3 = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
            Assert.AreNotEqual(rule1.GetHashCode(), rule3.GetHashCode());
        }

        [Test]
        public void ToString_WithMultiplication_ReturnsFormattedString()
        {
            // Arrange
            var rule = new LightOperationRule(2.5f, ArithmeticOperation.Multiplication);

            // Act
            var result = rule.ToString();

            // Assert
            Assert.IsTrue(result.Contains("LightOperation"));
            Assert.IsTrue(result.Contains("2.5"));
            Assert.IsTrue(result.Contains("×")); // 乗算記号
        }

        [Test]
        public void ToString_WithAddition_ReturnsFormattedString()
        {
            // Arrange
            var rule = new LightOperationRule(1.5f, ArithmeticOperation.Addition);

            // Act
            var result = rule.ToString();

            // Assert
            Assert.IsTrue(result.Contains("LightOperation"));
            Assert.IsTrue(result.Contains("1.5"));
            Assert.IsTrue(result.Contains("+")); // 加算記号
        }

        [TestCase(ArithmeticOperation.Addition, 2.0f, 3.0f, 5.0f)]
        [TestCase(ArithmeticOperation.Addition, -2.0f, 5.0f, 3.0f)]
        [TestCase(ArithmeticOperation.Multiplication, 3.0f, 4.0f, 12.0f)]
        [TestCase(ArithmeticOperation.Multiplication, 0.5f, 8.0f, 4.0f)]
        public void ApplyTo_WithVariousOperations_ReturnsExpectedResults(ArithmeticOperation operation, float value, float current, float expected)
        {
            // Arrange
            var rule = new LightOperationRule(value, operation);

            // Act
            var result = rule.ApplyTo(current);

            // Assert
            Assert.AreEqual(expected, result, 0.0001f);
        }
    }
}
