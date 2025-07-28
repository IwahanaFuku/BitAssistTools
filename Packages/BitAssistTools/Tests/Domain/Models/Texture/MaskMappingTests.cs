using System;
using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Domain.Models.Texture;

namespace BitAssistTools.Tests.Domain.Models.Texture
{
    /// <summary>
    /// MaskSourceとMaskMappingのテストクラス
    /// </summary>
    [TestFixture]
    public class MaskMappingTests
    {
        private Texture2D _mockTexture1;
        private Texture2D _mockTexture2;

        [SetUp]
        public void SetUp()
        {
            // テスト用のモックテクスチャを作成
            _mockTexture1 = new Texture2D(1, 1);
            _mockTexture1.name = "TestTexture1";
            
            _mockTexture2 = new Texture2D(1, 1);
            _mockTexture2.name = "TestTexture2";
        }

        [TearDown]
        public void TearDown()
        {
            // テスト後のクリーンアップ
            if (_mockTexture1 != null)
                UnityEngine.Object.DestroyImmediate(_mockTexture1);
            if (_mockTexture2 != null)
                UnityEngine.Object.DestroyImmediate(_mockTexture2);
        }

        #region MaskSource Tests

        [Test]
        public void MaskSource_DefaultConstructor_InitializesCorrectly()
        {
            // Act
            var maskSource = new MaskSource();

            // Assert
            Assert.IsNull(maskSource.Source);
            Assert.AreEqual(Channel.R, maskSource.SourceChannel);
            Assert.IsFalse(maskSource.Invert);
            Assert.IsFalse(maskSource.IsValid);
        }

        [Test]
        public void MaskSource_ParameterizedConstructor_InitializesCorrectly()
        {
            // Arrange
            var sourceChannel = Channel.G;
            var invert = true;

            // Act
            var maskSource = new MaskSource(_mockTexture1, sourceChannel, invert);

            // Assert
            Assert.AreEqual(_mockTexture1, maskSource.Source);
            Assert.AreEqual(sourceChannel, maskSource.SourceChannel);
            Assert.AreEqual(invert, maskSource.Invert);
            Assert.IsTrue(maskSource.IsValid);
        }

        [Test]
        public void MaskSource_ParameterizedConstructor_DefaultInvert_InitializesCorrectly()
        {
            // Arrange
            var sourceChannel = Channel.B;

            // Act
            var maskSource = new MaskSource(_mockTexture1, sourceChannel);

            // Assert
            Assert.AreEqual(_mockTexture1, maskSource.Source);
            Assert.AreEqual(sourceChannel, maskSource.SourceChannel);
            Assert.IsFalse(maskSource.Invert);
            Assert.IsTrue(maskSource.IsValid);
        }

        [Test]
        public void MaskSource_IsValid_WithNullTexture_ReturnsFalse()
        {
            // Arrange
            var maskSource = new MaskSource(null, Channel.R, false);

            // Act & Assert
            Assert.IsFalse(maskSource.IsValid);
        }

        [Test]
        public void MaskSource_IsValid_WithValidTexture_ReturnsTrue()
        {
            // Arrange
            var maskSource = new MaskSource(_mockTexture1, Channel.A, true);

            // Act & Assert
            Assert.IsTrue(maskSource.IsValid);
        }

        [Test]
        public void MaskSource_Equals_SameValues_ReturnsTrue()
        {
            // Arrange
            var maskSource1 = new MaskSource(_mockTexture1, Channel.G, true);
            var maskSource2 = new MaskSource(_mockTexture1, Channel.G, true);

            // Act & Assert
            Assert.IsTrue(maskSource1.Equals(maskSource2));
        }

        [Test]
        public void MaskSource_Equals_DifferentTexture_ReturnsFalse()
        {
            // Arrange
            var maskSource1 = new MaskSource(_mockTexture1, Channel.R, false);
            var maskSource2 = new MaskSource(_mockTexture2, Channel.R, false);

            // Act & Assert
            Assert.IsFalse(maskSource1.Equals(maskSource2));
        }

        [Test]
        public void MaskSource_Equals_DifferentChannel_ReturnsFalse()
        {
            // Arrange
            var maskSource1 = new MaskSource(_mockTexture1, Channel.R, false);
            var maskSource2 = new MaskSource(_mockTexture1, Channel.G, false);

            // Act & Assert
            Assert.IsFalse(maskSource1.Equals(maskSource2));
        }

        [Test]
        public void MaskSource_Equals_DifferentInvert_ReturnsFalse()
        {
            // Arrange
            var maskSource1 = new MaskSource(_mockTexture1, Channel.R, false);
            var maskSource2 = new MaskSource(_mockTexture1, Channel.R, true);

            // Act & Assert
            Assert.IsFalse(maskSource1.Equals(maskSource2));
        }

        [Test]
        public void MaskSource_Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var maskSource = new MaskSource(_mockTexture1, Channel.R, false);

            // Act & Assert
            Assert.IsFalse(maskSource.Equals(null));
        }

        [Test]
        public void MaskSource_Equals_WithSameReference_ReturnsTrue()
        {
            // Arrange
            var maskSource = new MaskSource(_mockTexture1, Channel.R, false);

            // Act & Assert
            Assert.IsTrue(maskSource.Equals(maskSource));
        }

        [Test]
        public void MaskSource_GetHashCode_SameValues_ReturnsSameHashCode()
        {
            // Arrange
            var maskSource1 = new MaskSource(_mockTexture1, Channel.B, true);
            var maskSource2 = new MaskSource(_mockTexture1, Channel.B, true);

            // Act
            var hash1 = maskSource1.GetHashCode();
            var hash2 = maskSource2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void MaskSource_ToString_ValidTexture_ReturnsFormattedString()
        {
            // Arrange
            var maskSource = new MaskSource(_mockTexture1, Channel.G, false);

            // Act
            var result = maskSource.ToString();

            // Assert
            Assert.AreEqual("MaskSource(TestTexture1.G)", result);
        }

        [Test]
        public void MaskSource_ToString_ValidTextureInverted_ReturnsFormattedString()
        {
            // Arrange
            var maskSource = new MaskSource(_mockTexture1, Channel.A, true);

            // Act
            var result = maskSource.ToString();

            // Assert
            Assert.AreEqual("MaskSource(TestTexture1.A (Inverted))", result);
        }

        [Test]
        public void MaskSource_ToString_NullTexture_ReturnsFormattedString()
        {
            // Arrange
            var maskSource = new MaskSource(null, Channel.R, false);

            // Act
            var result = maskSource.ToString();

            // Assert
            Assert.AreEqual("MaskSource(None.R)", result);
        }

        #endregion

        #region MaskMapping Tests

        [Test]
        public void MaskMapping_DefaultConstructor_InitializesCorrectly()
        {
            // Act
            var maskMapping = new MaskMapping();

            // Assert
            Assert.IsNull(maskMapping.OutR);
            Assert.IsNull(maskMapping.OutG);
            Assert.IsNull(maskMapping.OutB);
            Assert.IsNull(maskMapping.OutA);
            Assert.IsFalse(maskMapping.HasValidSources);
            Assert.AreEqual(0, maskMapping.ValidSourceCount);
        }

        [Test]
        public void MaskMapping_ParameterizedConstructor_InitializesCorrectly()
        {
            // Arrange
            var outR = new MaskSource(_mockTexture1, Channel.R, false);
            var outG = new MaskSource(_mockTexture1, Channel.G, false);
            var outB = new MaskSource(_mockTexture1, Channel.B, false);
            var outA = new MaskSource(_mockTexture1, Channel.A, false);

            // Act
            var maskMapping = new MaskMapping(outR, outG, outB, outA);

            // Assert
            Assert.AreEqual(outR, maskMapping.OutR);
            Assert.AreEqual(outG, maskMapping.OutG);
            Assert.AreEqual(outB, maskMapping.OutB);
            Assert.AreEqual(outA, maskMapping.OutA);
            Assert.IsTrue(maskMapping.HasValidSources);
            Assert.AreEqual(4, maskMapping.ValidSourceCount);
        }

        [Test]
        public void MaskMapping_HasValidSources_WithOneValidSource_ReturnsTrue()
        {
            // Arrange
            var maskMapping = new MaskMapping();
            maskMapping.OutR = new MaskSource(_mockTexture1, Channel.R, false);

            // Act & Assert
            Assert.IsTrue(maskMapping.HasValidSources);
            Assert.AreEqual(1, maskMapping.ValidSourceCount);
        }

        [Test]
        public void MaskMapping_HasValidSources_WithNoValidSources_ReturnsFalse()
        {
            // Arrange
            var maskMapping = new MaskMapping();
            maskMapping.OutR = new MaskSource(null, Channel.R, false);
            maskMapping.OutG = new MaskSource(null, Channel.G, false);

            // Act & Assert
            Assert.IsFalse(maskMapping.HasValidSources);
            Assert.AreEqual(0, maskMapping.ValidSourceCount);
        }

        [Test]
        public void MaskMapping_ValidSourceCount_CountsOnlyValidSources()
        {
            // Arrange
            var maskMapping = new MaskMapping();
            maskMapping.OutR = new MaskSource(_mockTexture1, Channel.R, false); // Valid
            maskMapping.OutG = new MaskSource(null, Channel.G, false);          // Invalid
            maskMapping.OutB = new MaskSource(_mockTexture2, Channel.B, false); // Valid
            maskMapping.OutA = null;                                            // Invalid

            // Act & Assert
            Assert.IsTrue(maskMapping.HasValidSources);
            Assert.AreEqual(2, maskMapping.ValidSourceCount);
        }

        [Test]
        public void MaskMapping_Equals_SameValues_ReturnsTrue()
        {
            // Arrange
            var outR = new MaskSource(_mockTexture1, Channel.R, false);
            var outG = new MaskSource(_mockTexture1, Channel.G, false);
            
            var maskMapping1 = new MaskMapping(outR, outG, null, null);
            var maskMapping2 = new MaskMapping(outR, outG, null, null);

            // Act & Assert
            Assert.IsTrue(maskMapping1.Equals(maskMapping2));
        }

        [Test]
        public void MaskMapping_Equals_DifferentValues_ReturnsFalse()
        {
            // Arrange
            var outR1 = new MaskSource(_mockTexture1, Channel.R, false);
            var outR2 = new MaskSource(_mockTexture2, Channel.R, false);
            
            var maskMapping1 = new MaskMapping(outR1, null, null, null);
            var maskMapping2 = new MaskMapping(outR2, null, null, null);

            // Act & Assert
            Assert.IsFalse(maskMapping1.Equals(maskMapping2));
        }

        [Test]
        public void MaskMapping_Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var maskMapping = new MaskMapping();

            // Act & Assert
            Assert.IsFalse(maskMapping.Equals(null));
        }

        [Test]
        public void MaskMapping_Equals_WithSameReference_ReturnsTrue()
        {
            // Arrange
            var maskMapping = new MaskMapping();

            // Act & Assert
            Assert.IsTrue(maskMapping.Equals(maskMapping));
        }

        [Test]
        public void MaskMapping_GetHashCode_SameValues_ReturnsSameHashCode()
        {
            // Arrange
            var outR = new MaskSource(_mockTexture1, Channel.R, false);
            var outG = new MaskSource(_mockTexture1, Channel.G, false);
            
            var maskMapping1 = new MaskMapping(outR, outG, null, null);
            var maskMapping2 = new MaskMapping(outR, outG, null, null);

            // Act
            var hash1 = maskMapping1.GetHashCode();
            var hash2 = maskMapping2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void MaskMapping_ToString_ReturnsFormattedString()
        {
            // Arrange
            var outR = new MaskSource(_mockTexture1, Channel.R, false);
            var outG = new MaskSource(_mockTexture1, Channel.G, true);
            var maskMapping = new MaskMapping(outR, outG, null, null);

            // Act
            var result = maskMapping.ToString();

            // Assert
            Assert.AreEqual("MaskMapping(R:MaskSource(TestTexture1.R), G:MaskSource(TestTexture1.G (Inverted)), B:, A:)", result);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void MaskMapping_CompleteWorkflow_WorksCorrectly()
        {
            // テクスチャのR,G,B,Aチャンネルを異なるソースから組み合わせるシナリオ
            // Arrange
            var rSource = new MaskSource(_mockTexture1, Channel.R, false);      // Texture1のRをRに
            var gSource = new MaskSource(_mockTexture1, Channel.G, true);       // Texture1のGを反転してGに
            var bSource = new MaskSource(_mockTexture2, Channel.B, false);      // Texture2のBをBに
            var aSource = new MaskSource(_mockTexture2, Channel.A, false);      // Texture2のAをAに

            // Act
            var maskMapping = new MaskMapping(rSource, gSource, bSource, aSource);

            // Assert
            Assert.IsTrue(maskMapping.HasValidSources);
            Assert.AreEqual(4, maskMapping.ValidSourceCount);
            
            // 個別のソースが正しく設定されていることを確認
            Assert.AreEqual(_mockTexture1, maskMapping.OutR.Source);
            Assert.AreEqual(Channel.R, maskMapping.OutR.SourceChannel);
            Assert.IsFalse(maskMapping.OutR.Invert);
            
            Assert.AreEqual(_mockTexture1, maskMapping.OutG.Source);
            Assert.AreEqual(Channel.G, maskMapping.OutG.SourceChannel);
            Assert.IsTrue(maskMapping.OutG.Invert);
            
            Assert.AreEqual(_mockTexture2, maskMapping.OutB.Source);
            Assert.AreEqual(Channel.B, maskMapping.OutB.SourceChannel);
            Assert.IsFalse(maskMapping.OutB.Invert);
            
            Assert.AreEqual(_mockTexture2, maskMapping.OutA.Source);
            Assert.AreEqual(Channel.A, maskMapping.OutA.SourceChannel);
            Assert.IsFalse(maskMapping.OutA.Invert);
        }

        [Test]
        public void MaskMapping_PartialMapping_WorksCorrectly()
        {
            // 一部のチャンネルのみをマッピングするシナリオ
            // Arrange
            var rSource = new MaskSource(_mockTexture1, Channel.R, false);

            // Act
            var maskMapping = new MaskMapping(rSource, null, null, null);

            // Assert
            Assert.IsTrue(maskMapping.HasValidSources);
            Assert.AreEqual(1, maskMapping.ValidSourceCount);
            Assert.IsNotNull(maskMapping.OutR);
            Assert.IsNull(maskMapping.OutG);
            Assert.IsNull(maskMapping.OutB);
            Assert.IsNull(maskMapping.OutA);
        }

        [Test]
        public void MaskMapping_CanBeUsedInCollections()
        {
            // コレクションでの使用確認
            var mappings = new System.Collections.Generic.List<MaskMapping>
            {
                new MaskMapping(),
                new MaskMapping(new MaskSource(_mockTexture1, Channel.R, false), null, null, null),
                new MaskMapping(null, new MaskSource(_mockTexture2, Channel.G, true), null, null)
            };

            Assert.AreEqual(3, mappings.Count);
            Assert.IsFalse(mappings[0].HasValidSources);
            Assert.IsTrue(mappings[1].HasValidSources);
            Assert.IsTrue(mappings[2].HasValidSources);
        }

        [TestCase(Channel.R)]
        [TestCase(Channel.G)]
        [TestCase(Channel.B)]
        [TestCase(Channel.A)]
        public void MaskSource_AllChannels_WorkCorrectly(Channel channel)
        {
            // すべてのチャンネルでMaskSourceが正しく動作することを確認
            // Act
            var maskSource = new MaskSource(_mockTexture1, channel, false);

            // Assert
            Assert.AreEqual(_mockTexture1, maskSource.Source);
            Assert.AreEqual(channel, maskSource.SourceChannel);
            Assert.IsFalse(maskSource.Invert);
            Assert.IsTrue(maskSource.IsValid);
        }

        #endregion
    }
}