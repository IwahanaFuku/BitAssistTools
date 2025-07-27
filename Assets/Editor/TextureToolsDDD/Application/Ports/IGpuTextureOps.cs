using UnityEngine;

namespace TextureTools.Application.Ports
{
    public interface IGpuTextureOps
    {
        bool IsSupported { get; }
        RenderTexture Invert(Texture src, bool r, bool g, bool b, bool a, Vector2Int outSize);

        // ★ 反転フラグを追加
        RenderTexture MergeMasks(
            Texture rSrc, int rCh, bool rInv,
            Texture gSrc, int gCh, bool gInv,
            Texture bSrc, int bCh, bool bInv,
            Texture aSrc, int aCh, bool aInv,
            Vector2Int outSize,
            float rFill, float gFill, float bFill, float aFill);

        RenderTexture NormalOps(Texture src, bool invertR, bool invertG, float strength, Vector2Int outSize);
    }
}
