using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services.TextureTools;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Application.ViewModels.TextureTools;

namespace BitAssistTools.Tests.Application.Services.TextureTools
{
    /// <summary>
    /// MaskMergeServiceのテストクラス
    /// </summary>
    [TestFixture]
    public class MaskMergeServiceTests
    {
        private Texture2D _redTexture;
        private Texture2D _greenTexture;
        private Texture2D _blueTexture;
        private Texture2D _alphaTexture;
        private MaskChannelData[] _testChannels;

        [SetUp]
        public void SetUp()
        {
            // テスト用のテクスチャを作成
            _redTexture = CreateSolidColorTexture(2, 2, Color.red);
            _redTexture.name = "RedTexture";

            _greenTexture = CreateSolidColorTexture(2, 2, Color.green);
            _greenTexture.name = "GreenTexture";

            _blueTexture = CreateSolidColorTexture(2, 2, Color.blue);
            _blueTexture.name = "BlueTexture";

            _alphaTexture = CreateSolidColorTexture(2, 2, new Color(1f, 1f, 1f, 0.5f));
            _alphaTexture.name = "AlphaTexture";

            // テスト用のチャンネルデータを作成
            _testChannels = new MaskChannelData[4]
            {
                new MaskChannelData { Source = _redTexture, SourceChannel = Channel.R, Invert = false },
                new MaskChannelData { Source = _greenTexture, SourceChannel = Channel.G, Invert = false },
                new MaskChannelData { Source = _blueTexture, SourceChannel = Channel.B, Invert = false },
                new MaskChannelData { Source = _alphaTexture, SourceChannel = Channel.A, Invert = false }
            };
        }

        [TearDown]
        public void TearDown()
        {
            // テスト後のクリーンアップ
            if (_redTexture != null) UnityEngine.Object.DestroyImmediate(_redTexture);
            if (_greenTexture != null) UnityEngine.Object.DestroyImmediate(_greenTexture);
            if (_blueTexture != null) UnityEngine.Object.DestroyImmediate(_blueTexture);
            if (_alphaTexture != null) UnityEngine.Object.DestroyImmediate(_alphaTexture);
        }

        private Texture2D CreateSolidColorTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        #region MergeChannels Tests

        [Test]
        public void MergeChannels_ValidChannels_MergesCorrectly()
        {
            // Act
            var result = MaskMergeService.MergeChannels(_testChannels, 2, 2);

            try
            {
                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.width);
                Assert.AreEqual(2, result.height);

                // 最初のピクセルをチェック
                var pixel = result.GetPixel(0, 0);
                Assert.AreEqual(1f, pixel.r, 0.01f, "Rチャンネル（赤テクスチャのR）");
                Assert.AreEqual(1f, pixel.g, 0.01f, "Gチャンネル（緑テクスチャのG）");
                Assert.AreEqual(1f, pixel.b, 0.01f, "Bチャンネル（青テクスチャのB）");
                Assert.AreEqual(0.5f, pixel.a, 0.01f, "Aチャンネル（アルファテクスチャのA）");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void MergeChannels_NullChannels_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                MaskMergeService.MergeChannels(null, 2, 2));
        }

        [Test]
        public void MergeChannels_InvalidChannelCount_ThrowsArgumentException()
        {
            // Arrange
            var invalidChannels = new MaskChannelData[3];

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                MaskMergeService.MergeChannels(invalidChannels, 2, 2));
        }

        [Test]
        public void MergeChannels_WithNullSources_UsesDefaultValues()
        {
            // Arrange
            var channelsWithNulls = new MaskChannelData[4]
            {
                new MaskChannelData { Source = null, SourceChannel = Channel.R, Invert = false },
                new MaskChannelData { Source = null, SourceChannel = Channel.G, Invert = false },
                new MaskChannelData { Source = null, SourceChannel = Channel.B, Invert = false },
                new MaskChannelData { Source = null, SourceChannel = Channel.A, Invert = false }
            };

            // Act
            var result = MaskMergeService.MergeChannels(channelsWithNulls, 2, 2);

            try
            {
                // Assert
                Assert.IsNotNull(result);
                
                var pixel = result.GetPixel(0, 0);
                Assert.AreEqual(0f, pixel.r, 0.01f, "RGBチャンネルのデフォルト値は0");
                Assert.AreEqual(0f, pixel.g, 0.01f, "RGBチャンネルのデフォルト値は0");
                Assert.AreEqual(0f, pixel.b, 0.01f, "RGBチャンネルのデフォルト値は0");
                Assert.AreEqual(1f, pixel.a, 0.01f, "Aチャンネルのデフォルト値は1");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void MergeChannels_WithInvertedChannels_InvertsValues()
        {
            // Arrange
            var invertedChannels = new MaskChannelData[4]
            {
                new MaskChannelData { Source = _redTexture, SourceChannel = Channel.R, Invert = true },
                new MaskChannelData { Source = _greenTexture, SourceChannel = Channel.G, Invert = true },
                new MaskChannelData { Source = _blueTexture, SourceChannel = Channel.B, Invert = true },
                new MaskChannelData { Source = _alphaTexture, SourceChannel = Channel.A, Invert = true }
            };

            // Act
            var result = MaskMergeService.MergeChannels(invertedChannels, 2, 2);

            try
            {
                // Assert
                var pixel = result.GetPixel(0, 0);
                Assert.AreEqual(0f, pixel.r, 0.01f, "反転されたRチャンネル（1-1=0）");
                Assert.AreEqual(0f, pixel.g, 0.01f, "反転されたGチャンネル（1-1=0）");
                Assert.AreEqual(0f, pixel.b, 0.01f, "反転されたBチャンネル（1-1=0）");
                Assert.AreEqual(0.5f, pixel.a, 0.01f, "反転されたAチャンネル（1-0.5=0.5）");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void MergeChannels_DifferentOutputSize_ScalesCorrectly()
        {
            // Act - 元のテクスチャ（2x2）を4x4にスケール
            var result = MaskMergeService.MergeChannels(_testChannels, 4, 4);

            try
            {
                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.width);
                Assert.AreEqual(4, result.height);

                // スケールされたテクスチャでも同じ色が取得できることを確認
                var pixel = result.GetPixel(0, 0);
                Assert.AreEqual(1f, pixel.r, 0.01f);
                Assert.AreEqual(1f, pixel.g, 0.01f);
                Assert.AreEqual(1f, pixel.b, 0.01f);
                Assert.AreEqual(0.5f, pixel.a, 0.01f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        #endregion

        #region GenerateMergePreview Tests

        [Test]
        public void GenerateMergePreview_ValidChannels_ReturnsPreview()
        {
            // Act
            var result = MaskMergeService.GenerateMergePreview(_testChannels, 2, 2);

            try
            {
                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.width);
                Assert.AreEqual(2, result.height);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void GenerateMergePreview_WithException_ReturnsDefaultTexture()
        {
            // Arrange - 無効なチャンネル数でエラーを発生させる
            var invalidChannels = new MaskChannelData[2];

            // 期待されるエラーログメッセージ（部分一致でより確実に）
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[MaskMergeService\] マスク統合プレビュー生成に失敗"));

            // Act
            var result = MaskMergeService.GenerateMergePreview(invalidChannels, 2, 2);

            try
            {
                // Assert - エラーが発生してもデフォルトテクスチャが返される
                Assert.IsNotNull(result, "エラーが発生してもテクスチャが返されるべき");
                Assert.AreEqual(2, result.width);
                Assert.AreEqual(2, result.height);

                // デフォルトテクスチャは黒（RGB=0, A=1）
                var pixel = result.GetPixel(0, 0);
                Assert.AreEqual(0f, pixel.r, 0.01f);
                Assert.AreEqual(0f, pixel.g, 0.01f);
                Assert.AreEqual(0f, pixel.b, 0.01f);
                Assert.AreEqual(1f, pixel.a, 0.01f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        #endregion

        #region Preset Application Tests

        [Test]
        public void ApplyORMPreset_ValidChannels_SetsCorrectMappings()
        {
            // Arrange
            var channels = new MaskChannelData[4];
            for (int i = 0; i < 4; i++)
            {
                channels[i] = new MaskChannelData();
            }
            var aoTexture = _redTexture;
            var roughnessTexture = _greenTexture;
            var metallicTexture = _blueTexture;

            // Act
            MaskMergeService.ApplyORMPreset(channels, aoTexture, roughnessTexture, metallicTexture);

            // Assert
            Assert.AreEqual(aoTexture, channels[0].Source, "AO → R");
            Assert.AreEqual(Channel.R, channels[0].SourceChannel);
            Assert.IsFalse(channels[0].Invert);

            Assert.AreEqual(roughnessTexture, channels[1].Source, "Roughness → G");
            Assert.AreEqual(Channel.G, channels[1].SourceChannel);
            Assert.IsFalse(channels[1].Invert);

            Assert.AreEqual(metallicTexture, channels[2].Source, "Metallic → B");
            Assert.AreEqual(Channel.B, channels[2].SourceChannel);
            Assert.IsFalse(channels[2].Invert);

            Assert.IsNull(channels[3].Source, "Alpha → null（デフォルト値）");
            Assert.AreEqual(Channel.A, channels[3].SourceChannel);
            Assert.IsFalse(channels[3].Invert);
        }

        [Test]
        public void ApplyMRAPreset_ValidChannels_SetsCorrectMappings()
        {
            // Arrange
            var channels = new MaskChannelData[4];
            for (int i = 0; i < 4; i++)
            {
                channels[i] = new MaskChannelData();
            }
            var metallicTexture = _redTexture;
            var roughnessTexture = _greenTexture;
            var aoTexture = _blueTexture;

            // Act
            MaskMergeService.ApplyMRAPreset(channels, metallicTexture, roughnessTexture, aoTexture);

            // Assert
            Assert.AreEqual(metallicTexture, channels[0].Source, "Metallic → R");
            Assert.AreEqual(Channel.R, channels[0].SourceChannel);
            Assert.IsFalse(channels[0].Invert);

            Assert.AreEqual(roughnessTexture, channels[1].Source, "Roughness → G");
            Assert.AreEqual(Channel.G, channels[1].SourceChannel);
            Assert.IsFalse(channels[1].Invert);

            Assert.AreEqual(aoTexture, channels[2].Source, "AO → B");
            Assert.AreEqual(Channel.B, channels[2].SourceChannel);
            Assert.IsFalse(channels[2].Invert);

            Assert.IsNull(channels[3].Source, "Alpha → null（デフォルト値）");
            Assert.AreEqual(Channel.A, channels[3].SourceChannel);
            Assert.IsFalse(channels[3].Invert);
        }

        [Test]
        public void ApplyORMPreset_InsufficientChannels_DoesNotThrow()
        {
            // Arrange
            var channels = new MaskChannelData[2]; // 4つ未満

            // Act & Assert
            Assert.DoesNotThrow(() =>
                MaskMergeService.ApplyORMPreset(channels, _redTexture, _greenTexture, _blueTexture));
        }

        [Test]
        public void ApplyMRAPreset_InsufficientChannels_DoesNotThrow()
        {
            // Arrange
            var channels = new MaskChannelData[3]; // 4つ未満

            // Act & Assert
            Assert.DoesNotThrow(() =>
                MaskMergeService.ApplyMRAPreset(channels, _redTexture, _greenTexture, _blueTexture));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void MaskMergeService_CompleteWorkflow_WorksCorrectly()
        {
            // Arrange - 完全なワークフローをテスト
            var channels = new MaskChannelData[4];
            for (int i = 0; i < 4; i++)
            {
                channels[i] = new MaskChannelData();
            }
            
            // ORMプリセット適用
            MaskMergeService.ApplyORMPreset(channels, _redTexture, _greenTexture, _blueTexture);
            
            // マスク統合実行
            var result = MaskMergeService.MergeChannels(channels, 2, 2);

            try
            {
                // Assert
                Assert.IsNotNull(result);
                
                var pixel = result.GetPixel(0, 0);
                Assert.AreEqual(1f, pixel.r, 0.01f, "AOテクスチャのRチャンネル");
                Assert.AreEqual(1f, pixel.g, 0.01f, "RoughnessテクスチャのGチャンネル");
                Assert.AreEqual(1f, pixel.b, 0.01f, "MetallicテクスチャのBチャンネル");
                Assert.AreEqual(1f, pixel.a, 0.01f, "デフォルトのAlpha値");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void MaskMergeService_WithMixedSourceChannels_ExtractsCorrectChannels()
        {
            // Arrange - 異なるソースチャンネルを使用
            var mixedTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            mixedTexture.SetPixel(0, 0, new Color(0.2f, 0.4f, 0.6f, 0.8f));
            mixedTexture.Apply();

            var channels = new MaskChannelData[4]
            {
                new MaskChannelData { Source = mixedTexture, SourceChannel = Channel.A, Invert = false }, // 0.8 → R
                new MaskChannelData { Source = mixedTexture, SourceChannel = Channel.B, Invert = false }, // 0.6 → G
                new MaskChannelData { Source = mixedTexture, SourceChannel = Channel.G, Invert = false }, // 0.4 → B
                new MaskChannelData { Source = mixedTexture, SourceChannel = Channel.R, Invert = false }  // 0.2 → A
            };

            try
            {
                // Act
                var result = MaskMergeService.MergeChannels(channels, 1, 1);

                try
                {
                    // Assert
                    var pixel = result.GetPixel(0, 0);
                    Assert.AreEqual(0.8f, pixel.r, 0.01f, "A → R");
                    Assert.AreEqual(0.6f, pixel.g, 0.01f, "B → G");
                    Assert.AreEqual(0.4f, pixel.b, 0.01f, "G → B");
                    Assert.AreEqual(0.2f, pixel.a, 0.01f, "R → A");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(result);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mixedTexture);
            }
        }

        [Test]
        public void MaskMergeService_WithUnreadableTexture_UsesDefaultValues()
        {
            // このテストは、読み取り不可能なテクスチャの処理を確認します
            // 実際のUnity環境では読み取り不可能なテクスチャを作成するのが困難なため、
            // nullテクスチャで代用してデフォルト値の動作を確認します

            // Arrange
            var channelsWithUnreadable = new MaskChannelData[4]
            {
                new MaskChannelData { Source = null, SourceChannel = Channel.R, Invert = false },
                new MaskChannelData { Source = null, SourceChannel = Channel.G, Invert = false },
                new MaskChannelData { Source = null, SourceChannel = Channel.B, Invert = false },
                new MaskChannelData { Source = null, SourceChannel = Channel.A, Invert = false }
            };

            // Act
            var result = MaskMergeService.MergeChannels(channelsWithUnreadable, 1, 1);

            try
            {
                // Assert - デフォルト値が使用される
                var pixel = result.GetPixel(0, 0);
                Assert.AreEqual(0f, pixel.r, 0.01f, "RGB = 0（デフォルト）");
                Assert.AreEqual(0f, pixel.g, 0.01f, "RGB = 0（デフォルト）");
                Assert.AreEqual(0f, pixel.b, 0.01f, "RGB = 0（デフォルト）");
                Assert.AreEqual(1f, pixel.a, 0.01f, "A = 1（デフォルト）");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        #endregion
    }
}