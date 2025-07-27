using UnityEngine;

namespace TextureTools.Application.Ports
{
    public interface IGpuTextureOps
    {
        bool IsSupported { get; }
        RenderTexture Invert(Texture src, bool r, bool g, bool b, bool a, Vector2Int outSize);
        RenderTexture MergeMasks(
            Texture rSrc, int rCh,
            Texture gSrc, int gCh,
            Texture bSrc, int bCh,
            Texture aSrc, int aCh,
            Vector2Int outSize,
            float rFill, float gFill, float bFill, float aFill);
        RenderTexture NormalOps(Texture src, bool invertR, bool invertG, float strength, Vector2Int outSize);
    }
}
