using System;
using UnityEngine;
using BitAssistTools.Domain.Models.TextureTools;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Application.ViewModels.TextureTools;
using static BitAssistTools.Application.ViewModels.TextureTools.DetailFunctionViewModel;

namespace BitAssistTools.Application.Services.TextureTools
{
    /// <summary>
    /// テクスチャプレビュー生成サービス
    /// </summary>
    public static class TexturePreviewService
    {
        /// <summary>
        /// レシピのプレビューを生成
        /// </summary>
        public static Texture2D GenerateRecipePreview(Texture2D sourceTexture, RecipeType recipe)
        {
            if (sourceTexture == null) return null;
            
            // プレビュー用にテクスチャを準備（Read/Write Enabled + Texture Type調整）
            if (!TextureDisplayService.EnsureTexturePreviewReady(sourceTexture))
            {
                // 自動設定に失敗した場合は表示用テクスチャを作成
                return TextureDisplayService.CreateDisplayableTexture(sourceTexture);
            }

            try
            {
                switch (recipe)
                {
                    case RecipeType.None:
                        return null;
                    case RecipeType.PngConversion:
                        return TextureDisplayService.CreateDisplayableTexture(sourceTexture);
                    case RecipeType.InvertAll:
                        return GenerateInvertAllPreview(sourceTexture);
                    case RecipeType.NormalGInvert:
                        return GenerateNormalGInvertPreview(sourceTexture);
                    case RecipeType.NormalStrength15:
                        return GenerateNormalStrengthPreview(sourceTexture, 1.5f);
                    case RecipeType.RMAToUnity:
                        return GenerateRMAToUnityPreview(sourceTexture);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TexturePreviewService] レシピプレビュー生成に失敗: {ex.Message}");
                return TextureDisplayService.CreateDisplayableTexture(sourceTexture);
            }
        }

        /// <summary>
        /// 詳細機能のプレビューを生成
        /// </summary>
        public static Texture2D GenerateDetailPreview(Texture2D sourceTexture, DetailFunction function, object parameters = null)
        {
            if (sourceTexture == null) return null;
            
            // プレビュー用にテクスチャを準備（Read/Write Enabled + Texture Type調整）
            if (!TextureDisplayService.EnsureTexturePreviewReady(sourceTexture))
            {
                // 自動設定に失敗した場合は表示用テクスチャを作成
                return TextureDisplayService.CreateDisplayableTexture(sourceTexture);
            }

            try
            {
                switch (function)
                {
                    case DetailFunction.None:
                        return null;
                    case DetailFunction.PngConvert:
                        return TextureDisplayService.CreateDisplayableTexture(sourceTexture);
                    case DetailFunction.ChannelInvert:
                        return GenerateChannelInvertPreview(sourceTexture, parameters);
                    case DetailFunction.NormalMap:
                        return GenerateNormalMapPreview(sourceTexture, parameters);
                    case DetailFunction.MaskMerge:
                        return GenerateMaskMergePreview(sourceTexture, parameters);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TexturePreviewService] 詳細機能プレビュー生成に失敗: {ex.Message}");
                return TextureDisplayService.CreateDisplayableTexture(sourceTexture);
            }
        }

        #region Private Preview Generation Methods

        private static Texture2D GenerateInvertAllPreview(Texture2D source)
        {
            var colors = source.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(1f - colors[i].r, 1f - colors[i].g, 1f - colors[i].b, colors[i].a);
            }
            
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        private static Texture2D GenerateNormalGInvertPreview(Texture2D source)
        {
            var colors = source.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(colors[i].r, 1f - colors[i].g, colors[i].b, colors[i].a);
            }
            
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        private static Texture2D GenerateNormalStrengthPreview(Texture2D source, float strength)
        {
            var colors = source.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                var r = colors[i].r * 2f - 1f;
                var g = colors[i].g * 2f - 1f;
                
                r *= strength;
                g *= strength;
                
                r = Mathf.Clamp(r * 0.5f + 0.5f, 0f, 1f);
                g = Mathf.Clamp(g * 0.5f + 0.5f, 0f, 1f);
                
                colors[i] = new Color(r, g, colors[i].b, colors[i].a);
            }
            
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        private static Texture2D GenerateRMAToUnityPreview(Texture2D source)
        {
            var colors = source.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                // UE4形式のRMAテクスチャ: R=AO, G=Roughness, B=Metallic
                // Unity Standard用に変換: R=Metallic, G=AO, B=0, Alpha=Smoothness (1 - Roughness)
                var ao = colors[i].r;        // R → G
                var roughness = colors[i].g; // G → Alpha (反転してSmoothness)
                var metallic = colors[i].b;  // B → R
                var smoothness = 1f - roughness; // RoughnessをSmoothnessに変換
                
                colors[i] = new Color(metallic, ao, 0f, smoothness);
            }
            
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        private static Texture2D GenerateChannelInvertPreview(Texture2D source, object parameters)
        {
            var colors = source.GetPixels();
            
            if (parameters is ChannelInvertParameters invertParams)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    var r = invertParams.InvertR ? 1f - colors[i].r : colors[i].r;
                    var g = invertParams.InvertG ? 1f - colors[i].g : colors[i].g;
                    var b = invertParams.InvertB ? 1f - colors[i].b : colors[i].b;
                    var a = invertParams.InvertA ? 1f - colors[i].a : colors[i].a;
                    colors[i] = new Color(r, g, b, a);
                }
            }
            
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        private static Texture2D GenerateNormalMapPreview(Texture2D source, object parameters)
        {
            var colors = source.GetPixels();
            
            if (parameters is NormalMapParameters normalParams)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    var r = normalParams.InvertR ? 1f - colors[i].r : colors[i].r;
                    var g = normalParams.InvertG ? 1f - colors[i].g : colors[i].g;
                    
                    // 強度調整
                    if (normalParams.Strength != 1.0f)
                    {
                        r = (r * 2f - 1f) * normalParams.Strength;
                        g = (g * 2f - 1f) * normalParams.Strength;
                        r = Mathf.Clamp(r * 0.5f + 0.5f, 0f, 1f);
                        g = Mathf.Clamp(g * 0.5f + 0.5f, 0f, 1f);
                    }
                    
                    colors[i] = new Color(r, g, colors[i].b, colors[i].a);
                }
            }
            
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        private static Texture2D GenerateMaskMergePreview(Texture2D source, object parameters)
        {
            // パラメータからマスク統合設定を取得
            if (parameters is DetailFunctionViewModel detailViewModel)
            {
                // マスクチャンネルが設定されているかチェック
                bool hasAnyMaskChannel = false;
                for (int i = 0; i < detailViewModel.MaskChannels.Length; i++)
                {
                    if (detailViewModel.MaskChannels[i].Source != null)
                    {
                        hasAnyMaskChannel = true;
                        break;
                    }
                }
                
                if (hasAnyMaskChannel)
                {
                    return MaskMergeService.GenerateMergePreview(
                        detailViewModel.MaskChannels, 
                        detailViewModel.OutputWidth, 
                        detailViewModel.OutputHeight,
                        detailViewModel.PreviewSize);
                }
            }
            
            // フォールバック - 何も設定されていない場合はデフォルトテクスチャを表示
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            var pixels = new Color[source.width * source.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 1f); // RGB=0, A=1
            }
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        #endregion
    }
}