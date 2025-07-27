using UnityEngine;
using TextureTools.Domain.Model;

namespace TextureTools.Domain.Services
{
    public interface ITextureProcessor
    {
        void InvertAllChannels(Color32[] pixels);
        void InvertChannels(Color32[] pixels, bool invertR, bool invertG, bool invertB, bool invertA);

        // 既存
        void MergeMasks(
            Color32[] outPixels, int width, int height,
            Color32[] rSrc, Channel rCh,
            Color32[] gSrc, Channel gCh,
            Color32[] bSrc, Channel bCh,
            Color32[] aSrc, Channel aCh);

        // ★ 追加：入力ごとの反転をサポート
        void MergeMasks(
            Color32[] outPixels, int width, int height,
            Color32[] rSrc, Channel rCh, bool rInv,
            Color32[] gSrc, Channel gCh, bool gInv,
            Color32[] bSrc, Channel bCh, bool bInv,
            Color32[] aSrc, Channel aCh, bool aInv);

        void AdjustNormalStrength(Color32[] pixels, float strength);
    }
}
