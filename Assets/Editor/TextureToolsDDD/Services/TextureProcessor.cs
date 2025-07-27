using UnityEngine;
using TextureTools.Domain.Model;

namespace TextureTools.Domain.Services
{
    public sealed class TextureProcessor : ITextureProcessor
    {
        private static byte GetCh(Color32 c, Channel ch)
        {
            switch (ch)
            {
                case Channel.R: return c.r;
                case Channel.G: return c.g;
                case Channel.B: return c.b;
                case Channel.A: return c.a;
                default:        return c.r;
            }
        }

        public void InvertAllChannels(Color32[] px)
        {
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                c.r = (byte)(255 - c.r);
                c.g = (byte)(255 - c.g);
                c.b = (byte)(255 - c.b);
                c.a = (byte)(255 - c.a);
                px[i] = c;
            }
        }

        public void InvertChannels(Color32[] px, bool r, bool g, bool b, bool a)
        {
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                if (r) c.r = (byte)(255 - c.r);
                if (g) c.g = (byte)(255 - c.g);
                if (b) c.b = (byte)(255 - c.b);
                if (a) c.a = (byte)(255 - c.a);
                px[i] = c;
            }
        }

        public void MergeMasks(
            Color32[] outPx, int w, int h,
            Color32[] rSrc, Channel rCh,
            Color32[] gSrc, Channel gCh,
            Color32[] bSrc, Channel bCh,
            Color32[] aSrc, Channel aCh)
        {
            int len = w * h;
            for (int i = 0; i < len; i++)
            {
                byte r = rSrc != null ? GetCh(rSrc[i], rCh) : (byte)0;
                byte g = gSrc != null ? GetCh(gSrc[i], gCh) : (byte)0;
                byte b = bSrc != null ? GetCh(bSrc[i], bCh) : (byte)0;
                byte a = aSrc != null ? GetCh(aSrc[i], aCh) : (byte)255;
                outPx[i] = new Color32(r, g, b, a);
            }
        }

        public void AdjustNormalStrength(Color32[] px, float strength)
        {
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                float nx = c.r / 255f * 2f - 1f;
                float ny = c.g / 255f * 2f - 1f;

                nx *= strength;
                ny *= strength;

                float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - nx * nx - ny * ny));
                float invLen = 1f / Mathf.Max(1e-6f, Mathf.Sqrt(nx * nx + ny * ny + nz * nz));
                nx *= invLen; ny *= invLen; nz *= invLen;

                c.r = (byte)Mathf.RoundToInt((nx * 0.5f + 0.5f) * 255f);
                c.g = (byte)Mathf.RoundToInt((ny * 0.5f + 0.5f) * 255f);
                c.b = (byte)Mathf.RoundToInt((nz * 0.5f + 0.5f) * 255f);
                px[i] = c;
            }
        }
    }
}
