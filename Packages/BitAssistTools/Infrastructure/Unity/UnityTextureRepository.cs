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
                // 一時的なテクスチャを作成してピクセルをコピー
                var tempTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
                Graphics.CopyTexture(texture, tempTexture);
                var pixels = tempTexture.GetPixels32();
                UnityEngine.Object.DestroyImmediate(tempTexture);
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
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));
            if (pixels.Length != width * height)
                throw new ArgumentException("Pixel array size does not match width * height");
            if (string.IsNullOrEmpty(basePathNoExt))
                throw new ArgumentException("Base path cannot be null or empty", nameof(basePathNoExt));

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);

                var path = basePathNoExt + ".png";
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
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

            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, true);
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