using System;
using UnityEngine;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Application.ViewModels.TextureTools;

namespace BitAssistTools.Application.Services.TextureTools
{
    /// <summary>
    /// マスク統合サービス
    /// </summary>
    public static class MaskMergeService
    {
        /// <summary>
        /// マスクチャンネルを統合して新しいテクスチャを生成
        /// </summary>
        public static Texture2D MergeChannels(MaskChannelData[] channels, int outputWidth, int outputHeight)
        {
            if (channels == null || channels.Length != 4)
                throw new ArgumentException("チャンネルデータは4つ必要です", nameof(channels));

            var resultTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);
            var pixels = new Color[outputWidth * outputHeight];

            // 各ピクセルを処理
            for (int y = 0; y < outputHeight; y++)
            {
                for (int x = 0; x < outputWidth; x++)
                {
                    int pixelIndex = y * outputWidth + x;
                    
                    // 各チャンネル（RGBA）の値を取得
                    float r = GetChannelValue(channels[0], x, y, outputWidth, outputHeight);
                    float g = GetChannelValue(channels[1], x, y, outputWidth, outputHeight);
                    float b = GetChannelValue(channels[2], x, y, outputWidth, outputHeight);
                    float a = GetChannelValue(channels[3], x, y, outputWidth, outputHeight);
                    
                    pixels[pixelIndex] = new Color(r, g, b, a);
                }
            }

            resultTexture.SetPixels(pixels);
            resultTexture.Apply();
            return resultTexture;
        }

        /// <summary>
        /// マスク統合のプレビューを生成
        /// </summary>
        public static Texture2D GenerateMergePreview(MaskChannelData[] channels, int outputWidth, int outputHeight)
        {
            try
            {
                return MergeChannels(channels, outputWidth, outputHeight);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MaskMergeService] マスク統合プレビュー生成に失敗: {ex.Message}");
                return CreateDefaultTexture(outputWidth, outputHeight);
            }
        }

        /// <summary>
        /// ORMプリセットを適用
        /// </summary>
        public static void ApplyORMPreset(MaskChannelData[] channels, Texture2D aoTexture, Texture2D roughnessTexture, Texture2D metallicTexture)
        {
            if (channels.Length < 4) return;

            // AO(R) + Roughness(G) + Metallic(B)
            channels[0].Source = aoTexture;
            channels[0].SourceChannel = Channel.R;
            channels[0].Invert = false;

            channels[1].Source = roughnessTexture;
            channels[1].SourceChannel = Channel.G;
            channels[1].Invert = false;

            channels[2].Source = metallicTexture;
            channels[2].SourceChannel = Channel.B;
            channels[2].Invert = false;

            // Alphaはデフォルト値（1.0）
            channels[3].Source = null;
            channels[3].SourceChannel = Channel.A;
            channels[3].Invert = false;
        }

        /// <summary>
        /// MRAプリセットを適用
        /// </summary>
        public static void ApplyMRAPreset(MaskChannelData[] channels, Texture2D metallicTexture, Texture2D roughnessTexture, Texture2D aoTexture)
        {
            if (channels.Length < 4) return;

            // Metallic(R) + Roughness(G) + AO(B)
            channels[0].Source = metallicTexture;
            channels[0].SourceChannel = Channel.R;
            channels[0].Invert = false;

            channels[1].Source = roughnessTexture;
            channels[1].SourceChannel = Channel.G;
            channels[1].Invert = false;

            channels[2].Source = aoTexture;
            channels[2].SourceChannel = Channel.B;
            channels[2].Invert = false;

            // Alphaはデフォルト値（1.0）
            channels[3].Source = null;
            channels[3].SourceChannel = Channel.A;
            channels[3].Invert = false;
        }

        #region Private Methods

        /// <summary>
        /// 指定座標のチャンネル値を取得
        /// </summary>
        private static float GetChannelValue(MaskChannelData channelData, int x, int y, int outputWidth, int outputHeight)
        {
            if (channelData.Source == null)
            {
                // デフォルト値を返す（RGB=0, A=1）
                return channelData.SourceChannel == Channel.A ? 1.0f : 0.0f;
            }

            // テクスチャが読み取り可能かチェック
            if (!TextureDisplayService.IsTextureReadable(channelData.Source))
            {
                return channelData.SourceChannel == Channel.A ? 1.0f : 0.0f;
            }

            try
            {
                // 座標を入力テクスチャのサイズに正規化
                float normalizedX = (float)x / outputWidth;
                float normalizedY = (float)y / outputHeight;
                
                int sourceX = Mathf.FloorToInt(normalizedX * channelData.Source.width);
                int sourceY = Mathf.FloorToInt(normalizedY * channelData.Source.height);
                
                // 範囲チェック
                sourceX = Mathf.Clamp(sourceX, 0, channelData.Source.width - 1);
                sourceY = Mathf.Clamp(sourceY, 0, channelData.Source.height - 1);
                
                Color pixel = channelData.Source.GetPixel(sourceX, sourceY);
                
                // 指定チャンネルの値を取得
                float value;
                switch (channelData.SourceChannel)
                {
                    case Channel.R:
                        value = pixel.r;
                        break;
                    case Channel.G:
                        value = pixel.g;
                        break;
                    case Channel.B:
                        value = pixel.b;
                        break;
                    case Channel.A:
                        value = pixel.a;
                        break;
                    default:
                        value = 0.0f;
                        break;
                }
                
                // 反転処理
                if (channelData.Invert)
                {
                    value = 1.0f - value;
                }
                
                return value;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MaskMergeService] チャンネル値取得に失敗: {ex.Message}");
                return channelData.SourceChannel == Channel.A ? 1.0f : 0.0f;
            }
        }

        /// <summary>
        /// デフォルトテクスチャを作成
        /// </summary>
        private static Texture2D CreateDefaultTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            
            // デフォルト値で埋める（RGB=0, A=1）
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 1f);
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        #endregion
    }
}