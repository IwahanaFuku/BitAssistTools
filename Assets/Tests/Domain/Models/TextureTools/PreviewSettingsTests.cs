using System;
using NUnit.Framework;
using BitAssistTools.Domain.Models.TextureTools;

namespace BitAssistTools.Tests.Domain.Models.TextureTools
{
    /// <summary>
    /// PreviewSettingsのテストクラス
    /// </summary>
    [TestFixture]
    public class PreviewSettingsTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var mode = PreviewMode.RGB;
            var showBeforeAfter = true;

            // Act
            var settings = new PreviewSettings(mode, showBeforeAfter);

            // Assert
            Assert.AreEqual(mode, settings.Mode);
            Assert.AreEqual(showBeforeAfter, settings.ShowBeforeAfter);
        }

        [Test]
        public void Constructor_AllPreviewModes_CreatesInstance()
        {
            // すべてのPreviewModeでインスタンスが作成できることを確認
            var modes = Enum.GetValues(typeof(PreviewMode));
            
            foreach (PreviewMode mode in modes)
            {
                Assert.DoesNotThrow(() =>
                {
                    var settings1 = new PreviewSettings(mode, true);
                    var settings2 = new PreviewSettings(mode, false);
                    
                    Assert.AreEqual(mode, settings1.Mode);
                    Assert.AreEqual(mode, settings2.Mode);
                    Assert.IsTrue(settings1.ShowBeforeAfter);
                    Assert.IsFalse(settings2.ShowBeforeAfter);
                });
            }
        }

        #endregion

        #region Equality Tests

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.Alpha, true);
            var settings2 = new PreviewSettings(PreviewMode.Alpha, true);

            // Act & Assert
            Assert.IsTrue(settings1.Equals(settings2));
            Assert.IsTrue(settings1 == settings2);
            Assert.IsFalse(settings1 != settings2);
        }

        [Test]
        public void Equals_DifferentMode_ReturnsFalse()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.RGB, false);
            var settings2 = new PreviewSettings(PreviewMode.Alpha, false);

            // Act & Assert
            Assert.IsFalse(settings1.Equals(settings2));
            Assert.IsFalse(settings1 == settings2);
            Assert.IsTrue(settings1 != settings2);
        }

        [Test]
        public void Equals_DifferentShowBeforeAfter_ReturnsFalse()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.Normal, true);
            var settings2 = new PreviewSettings(PreviewMode.Normal, false);

            // Act & Assert
            Assert.IsFalse(settings1.Equals(settings2));
            Assert.IsFalse(settings1 == settings2);
            Assert.IsTrue(settings1 != settings2);
        }

        [Test]
        public void Equals_BothDifferent_ReturnsFalse()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.R, true);
            var settings2 = new PreviewSettings(PreviewMode.G, false);

            // Act & Assert
            Assert.IsFalse(settings1.Equals(settings2));
            Assert.IsFalse(settings1 == settings2);
            Assert.IsTrue(settings1 != settings2);
        }

        [Test]
        public void Equals_WithObject_SameValues_ReturnsTrue()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.B, false);
            object settings2 = new PreviewSettings(PreviewMode.B, false);

            // Act & Assert
            Assert.IsTrue(settings1.Equals(settings2));
        }

        [Test]
        public void Equals_WithObject_DifferentType_ReturnsFalse()
        {
            // Arrange
            var settings = new PreviewSettings(PreviewMode.RGB, true);
            object other = "文字列";

            // Act & Assert
            Assert.IsFalse(settings.Equals(other));
        }

        [Test]
        public void Equals_WithObject_Null_ReturnsFalse()
        {
            // Arrange
            var settings = new PreviewSettings(PreviewMode.RGB, true);
            object other = null;

            // Act & Assert
            Assert.IsFalse(settings.Equals(other));
        }

        #endregion

        #region GetHashCode Tests

        [Test]
        public void GetHashCode_SameValues_ReturnsSameHashCode()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.Normal, true);
            var settings2 = new PreviewSettings(PreviewMode.Normal, true);

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_DifferentValues_ReturnsDifferentHashCode()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.RGB, false);
            var settings2 = new PreviewSettings(PreviewMode.Alpha, false);

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_DifferentShowBeforeAfter_ReturnsDifferentHashCode()
        {
            // Arrange
            var settings1 = new PreviewSettings(PreviewMode.R, true);
            var settings2 = new PreviewSettings(PreviewMode.R, false);

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        #endregion

        #region PreviewMode Enum Tests

        [Test]
        public void PreviewMode_AllValues_HaveValidNumbers()
        {
            // すべてのPreviewMode値が定義されていることを確認
            var values = Enum.GetValues(typeof(PreviewMode));
            
            Assert.Contains(PreviewMode.RGB, values);
            Assert.Contains(PreviewMode.Alpha, values);
            Assert.Contains(PreviewMode.Normal, values);
            Assert.Contains(PreviewMode.R, values);
            Assert.Contains(PreviewMode.G, values);
            Assert.Contains(PreviewMode.B, values);
        }

        [Test]
        public void PreviewMode_RGB_IsZero()
        {
            // RGB値が0であることを確認（デフォルト値として重要）
            Assert.AreEqual(0, (int)PreviewMode.RGB);
        }

        [Test]
        public void PreviewMode_ChannelModes_HaveCorrectOrder()
        {
            // チャンネルモードが適切な順序であることを確認
            Assert.IsTrue((int)PreviewMode.R > (int)PreviewMode.Normal);
            Assert.IsTrue((int)PreviewMode.G > (int)PreviewMode.R);
            Assert.IsTrue((int)PreviewMode.B > (int)PreviewMode.G);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PreviewSettings_DefaultConstructor_HasDefaultValues()
        {
            // デフォルトコンストラクタの動作確認
            var settings = default(PreviewSettings);
            
            Assert.AreEqual(PreviewMode.RGB, settings.Mode);
            Assert.IsFalse(settings.ShowBeforeAfter);
        }

        [Test]
        public void PreviewSettings_AllCombinations_CanBeCreated()
        {
            // すべてのモードとShowBeforeAfterの組み合わせでインスタンスが作成できることを確認
            var modes = Enum.GetValues(typeof(PreviewMode));
            var boolValues = new[] { true, false };
            
            foreach (PreviewMode mode in modes)
            {
                foreach (var showBeforeAfter in boolValues)
                {
                    Assert.DoesNotThrow(() =>
                    {
                        var settings = new PreviewSettings(mode, showBeforeAfter);
                        Assert.AreEqual(mode, settings.Mode);
                        Assert.AreEqual(showBeforeAfter, settings.ShowBeforeAfter);
                    });
                }
            }
        }

        #endregion
    }
}