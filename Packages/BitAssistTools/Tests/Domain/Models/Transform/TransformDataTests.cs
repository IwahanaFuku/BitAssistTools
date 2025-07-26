using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Domain.Models.Transform;

namespace BitAssistTools.Tests.Domain.Models.Transform
{
    [TestFixture]
    public class TransformDataTests
    {
        [Test]
        public void Constructor_WithValidParameters_CreatesCorrectTransformData()
        {
            // Arrange
            var position = Vector3.one;
            var rotation = Quaternion.identity;
            var scale = Vector3.one * 2f;

            // Act
            var transformData = new TransformData(position, rotation, scale);

            // Assert
            Assert.AreEqual(position, transformData.Position);
            Assert.AreEqual(rotation, transformData.Rotation);
            Assert.AreEqual(scale, transformData.LocalScale);
        }

        [Test]
        public void Identity_ReturnsDefaultTransformData()
        {
            // Act
            var identity = TransformData.Identity;

            // Assert
            Assert.AreEqual(Vector3.zero, identity.Position);
            Assert.AreEqual(Quaternion.identity, identity.Rotation);
            Assert.AreEqual(Vector3.one, identity.LocalScale);
        }

        [Test]
        public void FromUnityTransform_WithValidTransform_CreatesCorrectTransformData()
        {
            // Arrange
            var gameObject = new GameObject("TestObject");
            var transform = gameObject.transform;
            transform.position = Vector3.up;
            transform.rotation = Quaternion.Euler(45, 0, 0);
            transform.localScale = Vector3.one * 0.5f;

            try
            {
                // Act
                var transformData = TransformData.FromUnityTransform(transform);

                // Assert
                Assert.AreEqual(transform.position, transformData.Position);
                Assert.AreEqual(transform.rotation, transformData.Rotation);
                Assert.AreEqual(transform.localScale, transformData.LocalScale);
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void FromUnityTransform_WithNullTransform_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => 
                TransformData.FromUnityTransform(null));
        }

        [Test]
        public void WithPosition_CreatesNewTransformDataWithUpdatedPosition()
        {
            // Arrange
            var original = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);
            var newPosition = Vector3.up;

            // Act
            var updated = original.WithPosition(newPosition);

            // Assert
            Assert.AreEqual(newPosition, updated.Position);
            Assert.AreEqual(original.Rotation, updated.Rotation);
            Assert.AreEqual(original.LocalScale, updated.LocalScale);
            
            // Verify original is unchanged
            Assert.AreEqual(Vector3.zero, original.Position);
        }

        [Test]
        public void WithRotation_CreatesNewTransformDataWithUpdatedRotation()
        {
            // Arrange
            var original = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);
            var newRotation = Quaternion.Euler(90, 0, 0);

            // Act
            var updated = original.WithRotation(newRotation);

            // Assert
            Assert.AreEqual(original.Position, updated.Position);
            Assert.AreEqual(newRotation, updated.Rotation);
            Assert.AreEqual(original.LocalScale, updated.LocalScale);
        }

        [Test]
        public void WithScale_CreatesNewTransformDataWithUpdatedScale()
        {
            // Arrange
            var original = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);
            var newScale = Vector3.one * 2f;

            // Act
            var updated = original.WithScale(newScale);

            // Assert
            Assert.AreEqual(original.Position, updated.Position);
            Assert.AreEqual(original.Rotation, updated.Rotation);
            Assert.AreEqual(newScale, updated.LocalScale);
        }

        [Test]
        public void Add_CombinesTwoTransformDataCorrectly()
        {
            // Arrange
            var first = new TransformData(Vector3.right, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);
            var second = new TransformData(Vector3.up, Quaternion.Euler(0, 45, 0), Vector3.one * 0.5f);

            // Act
            var combined = first.Add(second);

            // Assert
            Assert.AreEqual(Vector3.right + Vector3.up, combined.Position);
            Assert.AreEqual(first.Rotation * second.Rotation, combined.Rotation);
            Assert.AreEqual(Vector3.Scale(first.LocalScale, second.LocalScale), combined.LocalScale);
        }

        [Test]
        public void TransformBy_WithValidTransform_AppliesTransformation()
        {
            // Arrange
            var gameObject = new GameObject("TransformSpace");
            var transformSpace = gameObject.transform;
            transformSpace.position = Vector3.right;
            transformSpace.rotation = Quaternion.Euler(0, 90, 0);
            transformSpace.localScale = Vector3.one * 2f;

            var transformData = new TransformData(Vector3.forward, Quaternion.identity, Vector3.one);

            try
            {
                // Act
                var transformed = transformData.TransformBy(transformSpace);

                // Assert
                Assert.AreEqual(transformSpace.TransformPoint(transformData.Position), transformed.Position);
                Assert.AreEqual(transformSpace.rotation * transformData.Rotation, transformed.Rotation);
                Assert.AreEqual(Vector3.Scale(transformSpace.lossyScale, transformData.LocalScale), transformed.LocalScale);
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void TransformBy_WithNullTransform_ReturnsOriginalTransformData()
        {
            // Arrange
            var transformData = new TransformData(Vector3.forward, Quaternion.identity, Vector3.one);

            // Act
            var result = transformData.TransformBy(null);

            // Assert
            Assert.AreEqual(transformData, result);
        }

        [Test]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var first = new TransformData(Vector3.up, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);
            var second = new TransformData(Vector3.up, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);

            // Act & Assert
            Assert.IsTrue(first.Equals(second));
            Assert.IsTrue(first == second);
            Assert.IsFalse(first != second);
        }

        [Test]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var first = new TransformData(Vector3.up, Quaternion.identity, Vector3.one);
            var second = new TransformData(Vector3.down, Quaternion.identity, Vector3.one);

            // Act & Assert
            Assert.IsFalse(first.Equals(second));
            Assert.IsFalse(first == second);
            Assert.IsTrue(first != second);
        }

        [Test]
        public void GetHashCode_WithSameValues_ReturnsSameHashCode()
        {
            // Arrange
            var first = new TransformData(Vector3.up, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);
            var second = new TransformData(Vector3.up, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);

            // Act & Assert
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var transformData = new TransformData(Vector3.one, Quaternion.identity, Vector3.one * 2f);

            // Act
            var result = transformData.ToString();

            // Assert
            Assert.IsTrue(result.Contains("TransformData"));
            Assert.IsTrue(result.Contains("Position"));
            Assert.IsTrue(result.Contains("Rotation"));
            Assert.IsTrue(result.Contains("Scale"));
        }
    }
}