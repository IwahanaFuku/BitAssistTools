using System;
using NUnit.Framework;
using BitAssistTools.Domain.Models.Texture;

namespace BitAssistTools.Tests.Domain.Models.Texture
{
    /// <summary>
    /// NormalStrengthのテストクラス
    /// </summary>
    [TestFixture]
    public class NormalStrengthTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_ValidValue_CreatesInstance()
        {
            // Arrange
            float value = 1.5f;

            // Act
            var normalStrength = new NormalStrength(value);

            // Assert
            Assert.AreEqual(value, normalStrength.Value);
        }

        [Test]
        public void Constructor_ZeroValue_CreatesInstance()
        {
            // Arrange
            float value = 0.0f;

            // Act
            var normalStrength = new NormalStrength(value);

            // Assert
            Assert.AreEqual(value, normalStrength.Value);
        }

        [Test]
        public void Constructor_LargeValue_CreatesInstance()
        {
            // Arrange
            float value = 10.0f;

            // Act
            var normalStrength = new NormalStrength(value);

            // Assert
            Assert.AreEqual(value, normalStrength.Value);
        }

        [Test]
        public void Constructor_NegativeValue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            float value = -1.0f;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new NormalStrength(value));
        }

        [Test]
        public void Constructor_NaNValue_ThrowsArgumentException()
        {
            // Arrange
            float value = float.NaN;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new NormalStrength(value));
        }

        [Test]
        public void Constructor_PositiveInfinityValue_ThrowsArgumentException()
        {
            // Arrange
            float value = float.PositiveInfinity;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new NormalStrength(value));
        }

        [Test]
        public void Constructor_NegativeInfinityValue_ThrowsArgumentException()
        {
            // Arrange
            float value = float.NegativeInfinity;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new NormalStrength(value));
        }

        #endregion

        #region Static Properties Tests

        [Test]
        public void Default_ReturnsOnePointZero()
        {
            // Act
            var defaultStrength = NormalStrength.Default;

            // Assert
            Assert.AreEqual(1.0f, defaultStrength.Value);
        }

        [Test]
        public void None_ReturnsZero()
        {
            // Act
            var noneStrength = NormalStrength.None;

            // Assert
            Assert.AreEqual(0.0f, noneStrength.Value);
        }

        [Test]
        public void StaticProperties_AreNotEqual()
        {
            // Act
            var defaultStrength = NormalStrength.Default;
            var noneStrength = NormalStrength.None;

            // Assert
            Assert.AreNotEqual(defaultStrength, noneStrength);
            Assert.IsTrue(defaultStrength != noneStrength);
        }

        #endregion

        #region Equality Tests

        [Test]
        public void Equals_SameValue_ReturnsTrue()
        {
            // Arrange
            var strength1 = new NormalStrength(2.0f);
            var strength2 = new NormalStrength(2.0f);

            // Act & Assert
            Assert.IsTrue(strength1.Equals(strength2));
            Assert.IsTrue(strength1 == strength2);
            Assert.IsFalse(strength1 != strength2);
        }

        [Test]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            // Arrange
            var strength1 = new NormalStrength(1.0f);
            var strength2 = new NormalStrength(2.0f);

            // Act & Assert
            Assert.IsFalse(strength1.Equals(strength2));
            Assert.IsFalse(strength1 == strength2);
            Assert.IsTrue(strength1 != strength2);
        }

        [Test]
        public void Equals_VeryCloseValues_ReturnsTrue()
        {
            // float.Epsilonよりも小さい差の値で等価性を確認
            // Arrange
            var strength1 = new NormalStrength(1.0f);
            var strength2 = new NormalStrength(1.0f + float.Epsilon * 0.5f);

            // Act & Assert
            Assert.IsTrue(strength1.Equals(strength2));
        }

        [Test]
        public void Equals_SlightlyDifferentValues_ReturnsFalse()
        {
            // float.Epsilonよりも明らかに大きい差の値で非等価性を確認
            // Arrange
            var strength1 = new NormalStrength(1.0f);
            var strength2 = new NormalStrength(1.0f + 0.0001f); // 0.0001fはfloat.Epsilonより十分大きい

            // Act & Assert
            Assert.IsFalse(strength1.Equals(strength2));
        }

        [Test]
        public void Equals_WithObject_SameValue_ReturnsTrue()
        {
            // Arrange
            var strength1 = new NormalStrength(1.5f);
            object strength2 = new NormalStrength(1.5f);

            // Act & Assert
            Assert.IsTrue(strength1.Equals(strength2));
        }

        [Test]
        public void Equals_WithObject_DifferentType_ReturnsFalse()
        {
            // Arrange
            var strength = new NormalStrength(1.0f);
            object other = 1.0f;

            // Act & Assert
            Assert.IsFalse(strength.Equals(other));
        }

        [Test]
        public void Equals_WithObject_Null_ReturnsFalse()
        {
            // Arrange
            var strength = new NormalStrength(1.0f);
            object other = null;

            // Act & Assert
            Assert.IsFalse(strength.Equals(other));
        }

        #endregion

        #region GetHashCode Tests

        [Test]
        public void GetHashCode_SameValue_ReturnsSameHashCode()
        {
            // Arrange
            var strength1 = new NormalStrength(2.5f);
            var strength2 = new NormalStrength(2.5f);

            // Act
            var hash1 = strength1.GetHashCode();
            var hash2 = strength2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_DifferentValue_ReturnsDifferentHashCode()
        {
            // Arrange
            var strength1 = new NormalStrength(1.0f);
            var strength2 = new NormalStrength(2.0f);

            // Act
            var hash1 = strength1.GetHashCode();
            var hash2 = strength2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_StaticProperties_ReturnsDifferentHashCodes()
        {
            // Act
            var defaultHash = NormalStrength.Default.GetHashCode();
            var noneHash = NormalStrength.None.GetHashCode();

            // Assert
            Assert.AreNotEqual(defaultHash, noneHash);
        }

        #endregion

        #region ToString Tests

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var strength = new NormalStrength(1.5f);

            // Act
            var result = strength.ToString();

            // Assert
            Assert.AreEqual("NormalStrength(1.50)", result);
        }

        [Test]
        public void ToString_ZeroValue_ReturnsFormattedString()
        {
            // Arrange
            var strength = new NormalStrength(0.0f);

            // Act
            var result = strength.ToString();

            // Assert
            Assert.AreEqual("NormalStrength(0.00)", result);
        }

        [Test]
        public void ToString_SmallValue_ReturnsFormattedString()
        {
            // Arrange
            var strength = new NormalStrength(0.1f);

            // Act
            var result = strength.ToString();

            // Assert
            Assert.AreEqual("NormalStrength(0.10)", result);
        }

        [Test]
        public void ToString_LargeValue_ReturnsFormattedString()
        {
            // Arrange
            var strength = new NormalStrength(10.25f);

            // Act
            var result = strength.ToString();

            // Assert
            Assert.AreEqual("NormalStrength(10.25)", result);
        }

        #endregion

        #region Validation Tests

        [TestCase(0.0f)]
        [TestCase(0.5f)]
        [TestCase(1.0f)]
        [TestCase(1.5f)]
        [TestCase(2.0f)]
        [TestCase(10.0f)]
        [TestCase(100.0f)]
        public void Constructor_ValidPositiveValues_CreatesInstance(float value)
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var strength = new NormalStrength(value);
                Assert.AreEqual(value, strength.Value);
            });
        }

        [TestCase(-0.1f)]
        [TestCase(-1.0f)]
        [TestCase(-10.0f)]
        public void Constructor_NegativeValues_ThrowsException(float value)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new NormalStrength(value));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void NormalStrength_DefaultConstructor_HasZeroValue()
        {
            // デフォルトコンストラクタの動作確認
            var strength = default(NormalStrength);
            
            Assert.AreEqual(0.0f, strength.Value);
        }

        [Test]
        public void NormalStrength_CanBeUsedInCollections()
        {
            // コレクションでの使用確認
            var strengths = new System.Collections.Generic.List<NormalStrength>
            {
                NormalStrength.None,
                new NormalStrength(0.5f),
                NormalStrength.Default,
                new NormalStrength(1.5f),
                new NormalStrength(2.0f)
            };

            Assert.AreEqual(5, strengths.Count);
            Assert.Contains(NormalStrength.None, strengths);
            Assert.Contains(NormalStrength.Default, strengths);
        }

        [Test]
        public void NormalStrength_CanBeUsedAsDictionaryKey()
        {
            // ディクショナリのキーとしての使用確認
            var strengthNames = new System.Collections.Generic.Dictionary<NormalStrength, string>
            {
                { NormalStrength.None, "なし" },
                { NormalStrength.Default, "標準" },
                { new NormalStrength(1.5f), "強め" },
                { new NormalStrength(2.0f), "とても強い" }
            };

            Assert.AreEqual("なし", strengthNames[NormalStrength.None]);
            Assert.AreEqual("標準", strengthNames[NormalStrength.Default]);
            Assert.AreEqual("強め", strengthNames[new NormalStrength(1.5f)]);
        }

        [Test]
        public void NormalStrength_CommonPresetValues_WorkCorrectly()
        {
            // 一般的なプリセット値のテスト
            var presets = new[]
            {
                new NormalStrength(0.5f),
                NormalStrength.Default,  // 1.0f
                new NormalStrength(1.5f),
                new NormalStrength(2.0f)
            };

            foreach (var preset in presets)
            {
                Assert.DoesNotThrow(() =>
                {
                    var displayString = preset.ToString();
                    var hashCode = preset.GetHashCode();
                    var isDefault = preset.Equals(NormalStrength.Default);
                    
                    Assert.IsNotNull(displayString);
                    Assert.IsNotNull(hashCode);
                });
            }
        }

        #endregion
    }
}