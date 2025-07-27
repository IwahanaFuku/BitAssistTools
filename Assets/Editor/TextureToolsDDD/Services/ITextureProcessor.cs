using UnityEngine;
using TextureTools.Domain.Model;

namespace TextureTools.Domain.Services
{
    public interface ITextureProcessor
    {
        void InvertAllChannels(Color32[] pixels);
        void InvertChannels(Color32[] pixels, bool invertR, bool invertG, bool invertB, bool invertA);

        void MergeMasks(
            Color32[] outPixels, int width, int height,
            Color32[] rSrc, Channel rCh,
            Color32[] gSrc, Channel gCh,
            Color32[] bSrc, Channel bCh,
            Color32[] aSrc, Channel aCh);

        void AdjustNormalStrength(Color32[] pixels, float strength);
    }
}
