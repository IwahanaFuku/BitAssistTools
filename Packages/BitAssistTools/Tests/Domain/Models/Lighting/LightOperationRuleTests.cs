
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
    }
}
