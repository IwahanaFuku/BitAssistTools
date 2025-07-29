
using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Domain.Models.Transform;

namespace BitAssistTools.Tests.Domain.Models.Placement
{
    [TestFixture]
    public class DuplicationStrategyTests
    {
        [Test]
        public void CalculateCumulativeTransform_WithIndex_ReturnsCorrectTransform()
        {
            // Arrange
            var delta = new TransformData(Vector3.right, Quaternion.identity, Vector3.one);
            var strategy = new DuplicationStrategy(5, delta);

            // Act
            var result = strategy.CalculateCumulativeTransform(2);

            // Assert
            Assert.AreEqual(Vector3.right * 2, result.Position);
        }

        [Test]
        public void IsValid_WithPositiveCopyCount_ReturnsTrue()
        {
            // Arrange
            var strategy = new DuplicationStrategy(1, TransformData.Identity);

            // Act & Assert
            Assert.IsTrue(strategy.IsValid);
        }

        [Test]
        public void IsValid_WithZeroCopyCount_ReturnsFalse()
        {
            // Arrange
            var strategy = new DuplicationStrategy(0, TransformData.Identity);

            // Act & Assert
            Assert.IsFalse(strategy.IsValid);
        }
    }
}
