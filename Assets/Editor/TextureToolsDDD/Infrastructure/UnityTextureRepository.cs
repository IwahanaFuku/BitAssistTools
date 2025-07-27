#if UNITY_EDITOR
using System;                          // ← Action / Func 用
using System.IO;
using TextureTools.Application.Ports;
using UnityEditor;
using UnityEngine;

namespace TextureTools.Infrastructure
{
    public sealed class UnityTextureRepository : ITextureRepository
    {
        // --- ITextureRepository 実装 ---

        public string GetAssetPath(UnityEngine.Object obj) => AssetDatabase.GetAssetPath(obj);

        public Color32[] LoadPixels(Texture2D tex, out int width, out int height)
        {
            EnsureReadable(tex, out var importer, out var originalReadable);
            width = tex.width; height = tex.height;

            var tmp = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            Graphics.CopyTexture(tex, tmp);
            var pixels = tmp.GetPixels32();
            UnityEngine.Object.DestroyImmediate(tmp);

            RestoreReadable(importer, originalReadable);
            return pixels;
        }

        public Texture2D CreateEmpty(int w, int h, TextureFormat fmt = TextureFormat.RGBA32, bool linear = true)
            => new Texture2D(w, h, fmt, false, linear);

        public Texture2D SavePixelsAsPng(Color32[] pixels, int w, int h, string basePathNoExt)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            tex.SetPixels32(pixels);
            tex.Apply(false, false);

            var path = basePathNoExt + ".png";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public Texture2D SaveRenderTextureAsPng(RenderTexture rt, string basePathNoExt)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            tex.Apply(false, false);

            RenderTexture.active = prev;

            var path = basePathNoExt + ".png";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public bool DeleteAsset(string assetPath) => AssetDatabase.DeleteAsset(assetPath);

        // ★ 追加: 処理中だけ TextureType=Default（必要なら isReadable=true）にして、終了時に元へ戻す
        public void WithTextureTypeDefault(Texture2D tex, bool needReadable, Action action)
        {
            if (tex == null) { action?.Invoke(); return; }

            var path = AssetDatabase.GetAssetPath(tex);
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
                    importer.SaveAndReimport();
            }

            try
            {
                action?.Invoke();
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
                        importer.SaveAndReimport();
                }
            }
        }

        public T WithTextureTypeDefault<T>(Texture2D tex, bool needReadable, Func<T> func)
        {
            T result = default(T);
            WithTextureTypeDefault(tex, needReadable, () => { result = (func != null) ? func() : default(T); });
            return result;
        }

        // --- 内部ユーティリティ（既存） ---

        private static void EnsureReadable(Texture2D tex, out TextureImporter importer, out bool originalReadable)
        {
            importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex)) as TextureImporter;
            originalReadable = importer != null && importer.isReadable;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        private static void RestoreReadable(TextureImporter importer, bool originalReadable)
        {
            if (importer != null && importer.isReadable != originalReadable)
            {
                importer.isReadable = originalReadable;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
