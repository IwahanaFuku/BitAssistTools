using System;
using UnityEngine;

namespace TextureTools.Application.Ports
{
    public interface ITextureRepository
    {
        Color32[] LoadPixels(Texture2D tex, out int width, out int height);
        Texture2D SavePixelsAsPng(Color32[] pixels, int width, int height, string basePathNoExt);
        Texture2D SaveRenderTextureAsPng(RenderTexture rt, string basePathNoExt);
        bool DeleteAsset(string assetPath);
        // ▼ ここを UnityEngine.Object に
        string GetAssetPath(UnityEngine.Object obj);
        Texture2D CreateEmpty(int width, int height, TextureFormat fmt = TextureFormat.RGBA32, bool linear = true);

        void WithTextureTypeDefault(Texture2D tex, bool needReadable, Action action);
        T WithTextureTypeDefault<T>(Texture2D tex, bool needReadable, Func<T> func);
    }
}
