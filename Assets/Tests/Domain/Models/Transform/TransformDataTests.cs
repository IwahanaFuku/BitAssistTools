using System;
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
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void FromUnityTransform_WithNullTransform_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
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
                UnityEngine.Object.DestroyImmediate(gameObject);
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

        [Test]
        public void ApplyToTransform_WithValidTransform_UpdatesTransformValues()
        {
            // Arrange
            var gameObject = new GameObject("TestObject");
            var transform = gameObject.transform;
            var transformData = new TransformData(Vector3.up, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);

            try
            {
                // Act
                transformData.ApplyToTransform(transform);

                // Assert
                Assert.AreEqual(transformData.Position, transform.position);
                Assert.AreEqual(transformData.Rotation, transform.rotation);
                Assert.AreEqual(transformData.LocalScale, transform.localScale);
            }
            finally
            {
                // Cleanup
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ApplyToTransform_WithNullTransform_ThrowsArgumentNullException()
        {
            // Arrange
            var transformData = TransformData.Identity;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => transformData.ApplyToTransform(null));
        }

        [Test]
        public void FromTransform_WithValidTransform_CreatesCorrectTransformData()
        {
            // Arrange
            var gameObject = new GameObject("TestObject");
            var transform = gameObject.transform;
            transform.position = new Vector3(1, 2, 3);
            transform.rotation = Quaternion.Euler(30, 60, 90);
            transform.localScale = new Vector3(0.5f, 1.5f, 2f);

            try
            {
                // Act
                var transformData = TransformData.FromTransform(transform);

                // Assert
                Assert.AreEqual(transform.position, transformData.Position);
                Assert.AreEqual(transform.rotation, transformData.Rotation);
                Assert.AreEqual(transform.localScale, transformData.LocalScale);
            }
            finally
            {
                // Cleanup
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void FromTransform_WithNullTransform_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                TransformData.FromTransform(null));
        }

        [Test]
        public void Add_WithZeroValues_ReturnsOriginalValues()
        {
            // Arrange
            var original = new TransformData(Vector3.one, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);
            var zero = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);

            // Act
            var result = original.Add(zero);

            // Assert
            Assert.AreEqual(original.Position, result.Position);
            Assert.AreEqual(original.Rotation, result.Rotation);
            Assert.AreEqual(original.LocalScale, result.LocalScale);
        }

        [Test]
        public void Add_WithNegativeValues_HandlesCorrectly()
        {
            // Arrange
            var original = new TransformData(Vector3.one, Quaternion.identity, Vector3.one * 2f);
            var negative = new TransformData(-Vector3.one, Quaternion.Euler(180, 0, 0), Vector3.one * 0.5f);

            // Act
            var result = original.Add(negative);

            // Assert
            Assert.AreEqual(Vector3.zero, result.Position);
            Assert.AreEqual(original.Rotation * negative.Rotation, result.Rotation);
            Assert.AreEqual(Vector3.one, result.LocalScale); // 2 * 0.5 = 1
        }

        [Test]
        public void WithPosition_WithSamePosition_ReturnsEquivalentTransformData()
        {
            // Arrange
            var original = new TransformData(Vector3.up, Quaternion.identity, Vector3.one);

            // Act
            var result = original.WithPosition(Vector3.up);

            // Assert
            Assert.AreEqual(original, result);
        }

        [Test]
        public void WithRotation_WithSameRotation_ReturnsEquivalentTransformData()
        {
            // Arrange
            var rotation = Quaternion.Euler(45, 30, 60);
            var original = new TransformData(Vector3.zero, rotation, Vector3.one);

            // Act
            var result = original.WithRotation(rotation);

            // Assert
            Assert.AreEqual(original, result);
        }

        [Test]
        public void WithScale_WithSameScale_ReturnsEquivalentTransformData()
        {
            // Arrange
            var scale = Vector3.one * 1.5f;
            var original = new TransformData(Vector3.zero, Quaternion.identity, scale);

            // Act
            var result = original.WithScale(scale);

            // Assert
            Assert.AreEqual(original, result);
        }

        [Test]
        public void TransformBy_WithIdentityTransform_ReturnsOriginal()
        {
            // Arrange
            var gameObject = new GameObject("IdentityTransform");
            var identityTransform = gameObject.transform;
            var original = new TransformData(Vector3.forward, Quaternion.Euler(45, 0, 0), Vector3.one * 2f);

            try
            {
                // Act
                var result = original.TransformBy(identityTransform);

                // Assert
                Assert.AreEqual(original.Position, result.Position);
                Assert.AreEqual(original.Rotation, result.Rotation);
                Assert.AreEqual(original.LocalScale, result.LocalScale);
            }
            finally
            {
                // Cleanup
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Equals_WithObjectType_HandlesCorrectly()
        {
            // Arrange
            var transformData = new TransformData(Vector3.up, Quaternion.identity, Vector3.one);
            object otherTransformData = new TransformData(Vector3.up, Quaternion.identity, Vector3.one);
            object differentObject = "not a transform data";

            // Act & Assert
            Assert.IsTrue(transformData.Equals(otherTransformData));
            Assert.IsFalse(transformData.Equals(differentObject));
            Assert.IsFalse(transformData.Equals(null));
        }

        [Test]
        public void GetHashCode_WithDifferentValues_ProducesDifferentHashes()
        {
            // Arrange
            var data1 = new TransformData(Vector3.up, Quaternion.identity, Vector3.one);
            var data2 = new TransformData(Vector3.right, Quaternion.identity, Vector3.one);
            var data3 = new TransformData(Vector3.up, Quaternion.Euler(45, 0, 0), Vector3.one);
            var data4 = new TransformData(Vector3.up, Quaternion.identity, Vector3.one * 2f);

            // Act & Assert
            Assert.AreNotEqual(data1.GetHashCode(), data2.GetHashCode());
            Assert.AreNotEqual(data1.GetHashCode(), data3.GetHashCode());
            Assert.AreNotEqual(data1.GetHashCode(), data4.GetHashCode());
        }

        [Test]
        public void Constructor_WithExtremeValues_HandlesCorrectly()
        {
            // Arrange & Act
            var extremeData = new TransformData(
                new Vector3(float.MaxValue, float.MinValue, 0),
                Quaternion.Euler(360, 720, -180),
                new Vector3(0.001f, 1000f, 1f)
            );

            // Assert
            Assert.IsNotNull(extremeData);
            Assert.AreEqual(float.MaxValue, extremeData.Position.x);
            Assert.AreEqual(float.MinValue, extremeData.Position.y);
        }

        [Test]
        public void Identity_IsReadOnly_ReturnsConsistentValues()
        {
            // Act
            var identity1 = TransformData.Identity;
            var identity2 = TransformData.Identity;

            // Assert
            Assert.AreEqual(identity1, identity2);
            Assert.AreEqual(Vector3.zero, identity1.Position);
            Assert.AreEqual(Quaternion.identity, identity1.Rotation);
            Assert.AreEqual(Vector3.one, identity1.LocalScale);
        }
    }
}