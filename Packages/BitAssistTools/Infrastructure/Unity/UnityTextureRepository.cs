#if UNITY_EDITOR
using System;
using System.IO;
using BitAssistTools.Domain.Repositories;
using UnityEditor;
using UnityEngine;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// Unityエディタ用テクスチャリポジトリ実装
    /// </summary>
    public sealed class UnityTextureRepository : ITextureRepository
    {
        /// <summary>
        /// オブジェクトのアセットパスを取得
        /// </summary>
        public string GetAssetPath(UnityEngine.Object obj)
        {
            return AssetDatabase.GetAssetPath(obj);
        }

        /// <summary>
        /// テクスチャからピクセルデータを読み込み
        /// </summary>
        public Color32[] LoadPixels(Texture2D texture, out int width, out int height)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            EnsureReadable(texture, out var importer, out var originalReadable);
            width = texture.width;
            height = texture.height;

            try
            {
                // 元のテクスチャのピクセルを読み取り（sRGB考慮）
                var colors = texture.GetPixels();
                var pixels = new Color32[colors.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    pixels[i] = colors[i];
                }
                return pixels;
            }
            finally
            {
                RestoreReadable(importer, originalReadable);
            }
        }

        /// <summary>
        /// 空のテクスチャを作成
        /// </summary>
        public Texture2D CreateEmpty(int width, int height, TextureFormat format = TextureFormat.RGBA32, bool linear = true)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Texture size must be positive");

            return new Texture2D(width, height, format, false, linear);
        }

        /// <summary>
        /// ピクセルデータをPNGとして保存
        /// </summary>
        public Texture2D SavePixelsAsPng(Color32[] pixels, int width, int height, string basePathNoExt)
        {
            return SavePixelsAsPngWithSettings(pixels, width, height, basePathNoExt, null);
        }
        
        /// <summary>
        /// ピクセルデータをPNGとして保存（元テクスチャの設定を継承）
        /// </summary>
        public Texture2D SavePixelsAsPngWithSettings(Color32[] pixels, int width, int height, string basePathNoExt, Texture2D originalTexture)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));
            if (pixels.Length != width * height)
                throw new ArgumentException("Pixel array size does not match width * height");
            if (string.IsNullOrEmpty(basePathNoExt))
                throw new ArgumentException("Base path cannot be null or empty", nameof(basePathNoExt));

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            try
            {
                // Color32からColorに変換してsRGB色空間を適切に処理
                var colors = new Color[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    colors[i] = pixels[i];
                }
                texture.SetPixels(colors);
                texture.Apply(false, false);

                var path = basePathNoExt + ".png";
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                
                // インポート後にテクスチャ設定を適切に設定
                ConfigureTextureImporter(path, originalTexture);
                
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// RenderTextureをPNGとして保存
        /// </summary>
        public Texture2D SaveRenderTextureAsPng(RenderTexture renderTexture, string basePathNoExt)
        {
            if (renderTexture == null)
                throw new ArgumentNullException(nameof(renderTexture));
            if (string.IsNullOrEmpty(basePathNoExt))
                throw new ArgumentException("Base path cannot be null or empty", nameof(basePathNoExt));

            var previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, false);
            try
            {
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
                texture.Apply(false, false);

                var path = basePathNoExt + ".png";
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                
                // インポート後にテクスチャ設定を適切に設定
                ConfigureTextureImporter(path, null);
                
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            finally
            {
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// アセットを削除
        /// </summary>
        public bool DeleteAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;

            return AssetDatabase.DeleteAsset(assetPath);
        }

        /// <summary>
        /// テクスチャの設定を一時的にデフォルトに変更して処理を実行
        /// </summary>
        public void WithTextureTypeDefault(Texture2D texture, bool needReadable, Action action)
        {
            if (texture == null)
            {
                action?.Invoke();
                return;
            }

            var path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            TextureImporterType originalType = TextureImporterType.Default;
            bool originalReadable = false;
            bool changedType = false;
            bool changedReadable = false;

            if (importer != null)
            {
                originalType = importer.textureType;
                originalReadable = importer.isReadable;

                if (importer.textureType != TextureImporterType.Default)
                {
                    importer.textureType = TextureImporterType.Default;
                    changedType = true;
                }
                if (needReadable && !importer.isReadable)
                {
                    importer.isReadable = true;
                    changedReadable = true;
                }
                if (changedType || changedReadable)
                {
                    importer.SaveAndReimport();
                }
            }

            try
            {
                action?.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (importer != null)
                {
                    bool needReimportBack = false;
                    if (changedType && importer.textureType != originalType)
                    {
                        importer.textureType = originalType;
                        needReimportBack = true;
                    }
                    if (changedReadable && importer.isReadable != originalReadable)
                    {
                        importer.isReadable = originalReadable;
                        needReimportBack = true;
                    }
                    if (needReimportBack)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        /// <summary>
        /// テクスチャの設定を一時的にデフォルトに変更して処理を実行（戻り値あり）
        /// </summary>
        public T WithTextureTypeDefault<T>(Texture2D texture, bool needReadable, Func<T> func)
        {
            T result = default(T);
            WithTextureTypeDefault(texture, needReadable, () => { result = func != null ? func() : default(T); });
            return result;
        }

        /// <summary>
        /// テクスチャインポーターを適切に設定
        /// </summary>
        private static void ConfigureTextureImporter(string texturePath, Texture2D originalTexture = null)
        {
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                // 元のテクスチャの設定を取得（可能な場合）
                TextureImporter originalImporter = null;
                if (originalTexture != null)
                {
                    var originalPath = AssetDatabase.GetAssetPath(originalTexture);
                    if (!string.IsNullOrEmpty(originalPath))
                    {
                        originalImporter = AssetImporter.GetAtPath(originalPath) as TextureImporter;
                    }
                }
                
                // 基本設定（元の設定を優先）
                importer.textureType = originalImporter?.textureType ?? TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = originalImporter?.alphaIsTransparency ?? true;
                
                // sRGB設定を最初に設定（重要）
                if (originalImporter != null)
                {
                    importer.sRGBTexture = originalImporter.sRGBTexture;
                }
                else
                {
                    // デフォルトは通常のカラーテクスチャとしてsRGB=trueに設定
                    importer.sRGBTexture = true;
                }
                
                // 圧縮設定（元の設定を優先、ただし品質は維持）
                importer.textureCompression = originalImporter?.textureCompression ?? TextureImporterCompression.Compressed;
                
                // プラットフォーム設定
                var platformSettings = importer.GetDefaultPlatformTextureSettings();
                if (originalImporter != null)
                {
                    var originalPlatformSettings = originalImporter.GetDefaultPlatformTextureSettings();
                    platformSettings.format = originalPlatformSettings.format;
                    platformSettings.textureCompression = originalPlatformSettings.textureCompression;
                    platformSettings.compressionQuality = Math.Max(originalPlatformSettings.compressionQuality, 90); // 最低90%品質
                }
                else
                {
                    platformSettings.format = TextureImporterFormat.Automatic;
                    platformSettings.textureCompression = TextureImporterCompression.Compressed;
                    platformSettings.compressionQuality = 100; // 最高品質
                }
                importer.SetPlatformTextureSettings(platformSettings);
                
                // ミップマップ設定（元の設定を優先）
                importer.mipmapEnabled = originalImporter?.mipmapEnabled ?? true;
                importer.streamingMipmaps = originalImporter?.streamingMipmaps ?? false;
                
                // フィルタリング設定（元の設定を優先）
                importer.filterMode = originalImporter?.filterMode ?? FilterMode.Bilinear;
                importer.anisoLevel = originalImporter?.anisoLevel ?? 1;
                
                // 変更を適用
                importer.SaveAndReimport();
            }
        }
        
        #region Private Helper Methods

        /// <summary>
        /// テクスチャを読み取り可能にする
        /// </summary>
        private static void EnsureReadable(Texture2D texture, out TextureImporter importer, out bool originalReadable)
        {
            importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            originalReadable = importer != null && importer.isReadable;
            
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// テクスチャの読み取り可能設定を元に戻す
        /// </summary>
        private static void RestoreReadable(TextureImporter importer, bool originalReadable)
        {
            if (importer != null && importer.isReadable != originalReadable)
            {
                importer.isReadable = originalReadable;
                importer.SaveAndReimport();
            }
        }

        #endregion
    }
}
#endif