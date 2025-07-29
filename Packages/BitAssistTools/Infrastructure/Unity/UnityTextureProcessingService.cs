using System;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Domain.Services;
using UnityEngine;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// Unity用テクスチャ処理サービス実装（CPU処理）
    /// </summary>
    public sealed class UnityTextureProcessingService : ITextureProcessingService
    {
        /// <summary>
        /// 指定されたチャンネルの値を取得
        /// </summary>
        private static byte GetChannelValue(Color32 color, Channel channel)
        {
            switch (channel)
            {
                case Channel.R: return color.r;
                case Channel.G: return color.g;
                case Channel.B: return color.b;
                case Channel.A: return color.a;
                default: return color.r;
            }
        }

        /// <summary>
        /// 全チャンネルを反転
        /// </summary>
        public void InvertAllChannels(Color32[] pixels)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));

            for (int i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                color.r = (byte)(255 - color.r);
                color.g = (byte)(255 - color.g);
                color.b = (byte)(255 - color.b);
                color.a = (byte)(255 - color.a);
                pixels[i] = color;
            }
        }

        /// <summary>
        /// 指定されたチャンネルを反転
        /// </summary>
        public void InvertChannels(Color32[] pixels, bool invertR, bool invertG, bool invertB, bool invertA)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));

            for (int i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                if (invertR) color.r = (byte)(255 - color.r);
                if (invertG) color.g = (byte)(255 - color.g);
                if (invertB) color.b = (byte)(255 - color.b);
                if (invertA) color.a = (byte)(255 - color.a);
                pixels[i] = color;
            }
        }

        /// <summary>
        /// 複数のマスクを統合（基本版）
        /// </summary>
        public void MergeMasks(
            Color32[] outputPixels, int width, int height,
            Color32[] rSource, Channel rChannel,
            Color32[] gSource, Channel gChannel,
            Color32[] bSource, Channel bChannel,
            Color32[] aSource, Channel aChannel)
        {
            // 反転フラグはすべて false で拡張版に委譲
            MergeMasks(outputPixels, width, height,
                rSource, rChannel, false,
                gSource, gChannel, false,
                bSource, bChannel, false,
                aSource, aChannel, false);
        }

        /// <summary>
        /// 複数のマスクを統合（反転サポート版）
        /// </summary>
        public void MergeMasks(
            Color32[] outputPixels, int width, int height,
            Color32[] rSource, Channel rChannel, bool rInvert,
            Color32[] gSource, Channel gChannel, bool gInvert,
            Color32[] bSource, Channel bChannel, bool bInvert,
            Color32[] aSource, Channel aChannel, bool aInvert)
        {
            if (outputPixels == null)
                throw new ArgumentNullException(nameof(outputPixels));
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive");
            if (outputPixels.Length != width * height)
                throw new ArgumentException("Output pixel array size does not match width * height");

            int pixelCount = width * height;
            for (int i = 0; i < pixelCount; i++)
            {
                byte r = rSource != null && i < rSource.Length ? GetChannelValue(rSource[i], rChannel) : (byte)0;
                byte g = gSource != null && i < gSource.Length ? GetChannelValue(gSource[i], gChannel) : (byte)0;
                byte b = bSource != null && i < bSource.Length ? GetChannelValue(bSource[i], bChannel) : (byte)0;
                byte a = aSource != null && i < aSource.Length ? GetChannelValue(aSource[i], aChannel) : (byte)255;

                if (rInvert) r = (byte)(255 - r);
                if (gInvert) g = (byte)(255 - g);
                if (bInvert) b = (byte)(255 - b);
                if (aInvert) a = (byte)(255 - a);

                outputPixels[i] = new Color32(r, g, b, a);
            }
        }

        /// <summary>
        /// 法線マップの強度を調整
        /// </summary>
        public void AdjustNormalStrength(Color32[] pixels, float strength)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));
            if (strength < 0f)
                throw new ArgumentOutOfRangeException(nameof(strength), "Strength must be non-negative");

            for (int i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                
                // RGから法線ベクトルのXY成分を取得（-1～1の範囲）
                float normalX = color.r / 255f * 2f - 1f;
                float normalY = color.g / 255f * 2f - 1f;

                // 強度を適用
                normalX *= strength;
                normalY *= strength;

                // Z成分を計算（法線ベクトルの長さが1になるように）
                float normalZ = Mathf.Sqrt(Mathf.Max(0f, 1f - normalX * normalX - normalY * normalY));

                // 法線ベクトルを正規化
                float length = Mathf.Max(1e-6f, Mathf.Sqrt(normalX * normalX + normalY * normalY + normalZ * normalZ));
                float invLength = 1f / length;
                normalX *= invLength;
                normalY *= invLength; 
                normalZ *= invLength;

                // 0～255の範囲に戻す
                color.r = (byte)Mathf.RoundToInt((normalX * 0.5f + 0.5f) * 255f);
                color.g = (byte)Mathf.RoundToInt((normalY * 0.5f + 0.5f) * 255f);
                color.b = (byte)Mathf.RoundToInt((normalZ * 0.5f + 0.5f) * 255f);
                // Aチャンネルはそのまま維持

                pixels[i] = color;
            }
        }
    }
}