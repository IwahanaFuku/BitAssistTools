using UnityEngine;
using TextureTools.Application.Ports;

namespace TextureTools.Infrastructure.Gpu
{
    public sealed class UnityComputeTextureOps : IGpuTextureOps
    {
        private readonly ComputeShader _cs;
        private readonly int _kInvert, _kMerge, _kNormal;

        public bool IsSupported => SystemInfo.supportsComputeShaders;

        public UnityComputeTextureOps(ComputeShader cs)
        {
            _cs = cs;
            _kInvert = _cs.FindKernel("Invert");
            _kMerge  = _cs.FindKernel("MergeMasks");
            _kNormal = _cs.FindKernel("NormalOps");
        }

        static RenderTexture CreateRt(Vector2Int sz)
        {
            var rt = new RenderTexture(sz.x, sz.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            rt.enableRandomWrite = true;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.filterMode = FilterMode.Bilinear;
            rt.Create();
            return rt;
        }

        static void SetSize(ComputeShader cs, int kernel, Vector2Int sz)
        {
            cs.SetInts("_Size", sz.x, sz.y);
        }

        public RenderTexture Invert(Texture src, bool r, bool g, bool b, bool a, Vector2Int outSize)
        {
            var rt = CreateRt(outSize);
            _cs.SetTexture(_kInvert, "_Src", src);
            _cs.SetTexture(_kInvert, "_Dst", rt);
            SetSize(_cs, _kInvert, outSize);
            _cs.SetInt("_InvR", r ? 1 : 0);
            _cs.SetInt("_InvG", g ? 1 : 0);
            _cs.SetInt("_InvB", b ? 1 : 0);
            _cs.SetInt("_InvA", a ? 1 : 0);
            _cs.Dispatch(_kInvert, Mathf.CeilToInt(outSize.x / 8f), Mathf.CeilToInt(outSize.y / 8f), 1);
            return rt;
        }

        public RenderTexture MergeMasks(
            Texture rSrc, int rCh,
            Texture gSrc, int gCh,
            Texture bSrc, int bCh,
            Texture aSrc, int aCh,
            Vector2Int outSize,
            float rFill, float gFill, float bFill, float aFill)
        {
            var rt = CreateRt(outSize);
            _cs.SetTexture(_kMerge, "_Rsrc", rSrc);
            _cs.SetTexture(_kMerge, "_Gsrc", gSrc);
            _cs.SetTexture(_kMerge, "_Bsrc", bSrc);
            _cs.SetTexture(_kMerge, "_Asrc", aSrc);
            _cs.SetTexture(_kMerge, "_Dst",  rt);

            SetSize(_cs, _kMerge, outSize);
            _cs.SetInt("_Rch", rCh); _cs.SetInt("_Gch", gCh);
            _cs.SetInt("_Bch", bCh); _cs.SetInt("_Ach", aCh);

            _cs.SetFloat("_Rfill", rFill); _cs.SetFloat("_Gfill", gFill);
            _cs.SetFloat("_Bfill", bFill); _cs.SetFloat("_Afill", aFill);

            _cs.SetInt("_HasR", rSrc ? 1 : 0);
            _cs.SetInt("_HasG", gSrc ? 1 : 0);
            _cs.SetInt("_HasB", bSrc ? 1 : 0);
            _cs.SetInt("_HasA", aSrc ? 1 : 0);

            _cs.Dispatch(_kMerge, Mathf.CeilToInt(outSize.x / 8f), Mathf.CeilToInt(outSize.y / 8f), 1);
            return rt;
        }

        public RenderTexture NormalOps(Texture src, bool invertR, bool invertG, float strength, Vector2Int outSize)
        {
            var rt = CreateRt(outSize);
            _cs.SetTexture(_kNormal, "_Src", src);
            _cs.SetTexture(_kNormal, "_Dst", rt);
            SetSize(_cs, _kNormal, outSize);
            _cs.SetInt("_NInvR", invertR ? 1 : 0);
            _cs.SetInt("_NInvG", invertG ? 1 : 0);
            _cs.SetFloat("_Strength", Mathf.Max(0f, strength));
            _cs.Dispatch(_kNormal, Mathf.CeilToInt(outSize.x / 8f), Mathf.CeilToInt(outSize.y / 8f), 1);
            return rt;
        }
    }
}
