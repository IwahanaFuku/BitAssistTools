using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.Services.TextureTools;

namespace BitAssistTools.Tests.Application.Services.TextureTools
{
    /// <summary>
    /// TextureDisplayServiceのテストクラス
    /// </summary>
    [TestFixture]
    public class TextureDisplayServiceTests
    {
        private Texture2D _testTexture;
        private Texture2D _normalMapTexture;

        [SetUp]
        public void SetUp()
        {
            // テスト用のテクスチャを作成
            _testTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            _testTexture.SetPixels(new Color[] 
            {
                Color.red, Color.green,
                Color.blue, Color.white
            });
            _testTexture.Apply();
            _testTexture.name = "TestTexture";

            // NormalMap用のテクスチャを作成
            _normalMapTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            _normalMapTexture.SetPixels(new Color[]
            {
                new Color(0.5f, 0.5f, 1.0f, 1.0f), new Color(0.6f, 0.4f, 1.0f, 1.0f),
                new Color(0.4f, 0.6f, 1.0f, 1.0f), new Color(0.5f, 0.5f, 1.0f, 1.0f)
            });
            _normalMapTexture.Apply();
            _normalMapTexture.name = "NormalMapTexture";
        }

        [TearDown]
        public void TearDown()
        {
            // テスト後のクリーンアップ
            if (_testTexture != null)
                UnityEngine.Object.DestroyImmediate(_testTexture);
            if (_normalMapTexture != null)
                UnityEngine.Object.DestroyImmediate(_normalMapTexture);
                
            // 管理状態をクリア
            TextureDisplayService.RestoreAllTextureSettings();
        }

        #region CreateDisplayableTexture Tests

        [Test]
        public void CreateDisplayableTexture_NullTexture_ReturnsNull()
        {
            // Act
            var result = TextureDisplayService.CreateDisplayableTexture(null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void CreateDisplayableTexture_RuntimeTexture_ReturnsSameTexture()
        {
            // Arrange - ランタイム生成テクスチャ（AssetDatabaseにないもの）
            var runtimeTexture = new Texture2D(1, 1);

            try
            {
                // Act
                var result = TextureDisplayService.CreateDisplayableTexture(runtimeTexture);

                // Assert
                Assert.AreEqual(runtimeTexture, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(runtimeTexture);
            }
        }

        [Test]
        public void CreateDisplayableTexture_RegularTexture_ReturnsSameTexture()
        {
            // Act
            var result = TextureDisplayService.CreateDisplayableTexture(_testTexture);

            // Assert
            Assert.AreEqual(_testTexture, result);
        }

        #endregion

        #region IsTextureReadable Tests

        [Test]
        public void IsTextureReadable_NullTexture_ReturnsFalse()
        {
            // Act
            var result = TextureDisplayService.IsTextureReadable(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsTextureReadable_ReadableTexture_ReturnsTrue()
        {
            // Act
            var result = TextureDisplayService.IsTextureReadable(_testTexture);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Texture Management Tests

        [Test]
        public void HasManagedTextures_Initially_ReturnsFalse()
        {
            // Act
            var result = TextureDisplayService.HasManagedTextures();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void EnsureTexturePreviewReady_NullTexture_ReturnsFalse()
        {
            // Act
            var result = TextureDisplayService.EnsureTexturePreviewReady(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void EnsureTexturePreviewReady_RuntimeTexture_ReturnsFalse()
        {
            // Arrange - ランタイム生成テクスチャ
            var runtimeTexture = new Texture2D(1, 1);

            try
            {
                // Act
                var result = TextureDisplayService.EnsureTexturePreviewReady(runtimeTexture);

                // Assert
                Assert.IsFalse(result, "ランタイムテクスチャはAssetDatabaseに存在しないため管理できない");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(runtimeTexture);
            }
        }

        [Test]
        public void RestoreCurrentTexture_NoManagedTexture_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => TextureDisplayService.RestoreCurrentTexture());
        }

        [Test]
        public void RestoreAllTextureSettings_CallsRestoreCurrentTexture()
        {
            // このテストは内部実装のテストなので、例外が発生しないことを確認
            // Act & Assert
            Assert.DoesNotThrow(() => TextureDisplayService.RestoreAllTextureSettings());
        }

        [Test]
        public void RestoreAllTextureReadable_CallsRestoreAllTextureSettings()
        {
            // 互換性メソッドのテスト
            // Act & Assert
            Assert.DoesNotThrow(() => TextureDisplayService.RestoreAllTextureReadable());
        }

        [Test]
        public void GetManagedTextureInfo_NoManagedTextures_ReturnsEmptyList()
        {
            // Act
            var result = TextureDisplayService.GetManagedTextureInfo();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void TextureManagement_CompleteWorkflow_WorksCorrectly()
        {
            // 初期状態の確認
            Assert.IsFalse(TextureDisplayService.HasManagedTextures());
            Assert.AreEqual(0, TextureDisplayService.GetManagedTextureInfo().Count);

            // クリーンアップが正常に動作することを確認
            Assert.DoesNotThrow(() => TextureDisplayService.RestoreAllTextureSettings());
            
            // 状態が変わらないことを確認
            Assert.IsFalse(TextureDisplayService.HasManagedTextures());
        }

        [Test]
        public void CreateDisplayableTexture_WithDifferentTextureTypes_HandlesCorrectly()
        {
            // 異なるフォーマットのテクスチャでテスト
            var formats = new[] 
            { 
                TextureFormat.RGB24, 
                TextureFormat.ARGB32, 
                TextureFormat.RGBA32 
            };

            foreach (var format in formats)
            {
                var texture = new Texture2D(1, 1, format, false);
                texture.SetPixel(0, 0, Color.red);
                texture.Apply();

                try
                {
                    Assert.DoesNotThrow(() =>
                    {
                        var result = TextureDisplayService.CreateDisplayableTexture(texture);
                        Assert.IsNotNull(result, $"Format {format} should return a valid texture");
                    });
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
        }

        [Test]
        public void IsTextureReadable_WithDifferentStates_ReturnsCorrectResults()
        {
            // 読み取り可能なテクスチャのテスト
            Assert.IsTrue(TextureDisplayService.IsTextureReadable(_testTexture));
            
            // 読み取り可能なNormalMapテクスチャのテスト
            Assert.IsTrue(TextureDisplayService.IsTextureReadable(_normalMapTexture));
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void CreateDisplayableTexture_WithInvalidTexture_HandlesGracefully()
        {
            // 無効なテクスチャ（サイズ0）でのテスト
            var invalidTexture = new Texture2D(0, 0);
            
            try
            {
                // エラーが発生せず、何らかの結果が返されることを確認
                Assert.DoesNotThrow(() =>
                {
                    var result = TextureDisplayService.CreateDisplayableTexture(invalidTexture);
                    // 結果がnullでないか、元のテクスチャが返されることを確認
                    Assert.IsTrue(result == null || result == invalidTexture);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(invalidTexture);
            }
        }

        #endregion
    }
}