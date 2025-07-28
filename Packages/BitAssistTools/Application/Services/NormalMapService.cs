using System;
using BitAssistTools.Domain.Models.Shared;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Domain.Repositories;
using BitAssistTools.Domain.Services;
using UnityEngine;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// 法線マップ処理のアプリケーションサービス
    /// </summary>
    public static class NormalMapService
    {
        /// <summary>
        /// 法線マップの調整（反転と強度変更）
        /// </summary>
        /// <param name="normalTexture">法線マップテクスチャ</param>
        /// <param name="invertR">Rチャンネルを反転するか</param>
        /// <param name="invertG">Gチャンネルを反転するか</param>
        /// <param name="strength">法線の強度</param>
        /// <param name="saveBasePathNoExt">保存先ベースパス（拡張子なし）</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <param name="cpuProcessor">CPU処理サービス</param>
        /// <param name="gpuProcessor">GPU処理サービス（オプション）</param>
        /// <returns>調整処理結果</returns>
        public static Result<Texture2D> AdjustNormalMap(
            Texture2D normalTexture,
            bool invertR, bool invertG,
            NormalStrength strength,
            string saveBasePathNoExt,
            ITextureRepository repository,
            ITextureProcessingService cpuProcessor,
            IGpuTextureOperationService gpuProcessor = null)
        {
            try
            {
                if (normalTexture == null)
                    return Result<Texture2D>.Fail("法線マップが選択されていません。");

                if (repository == null)
                    return Result<Texture2D>.Fail("テクスチャリポジトリが設定されていません。");

                if (cpuProcessor == null)
                    return Result<Texture2D>.Fail("CPU処理サービスが設定されていません。");

                if (string.IsNullOrEmpty(saveBasePathNoExt))
                    return Result<Texture2D>.Fail("保存先パスが指定されていません。");

                Texture2D savedTexture = null;

                repository.WithTextureTypeDefault(normalTexture, needReadable: gpuProcessor == null, action: () =>
                {
                    if (gpuProcessor != null && gpuProcessor.IsSupported)
                    {
                        // GPU処理
                        var renderTexture = gpuProcessor.ProcessNormalMap(
                            normalTexture, invertR, invertG, strength.Value,
                            new Vector2Int(normalTexture.width, normalTexture.height));
                        
                        savedTexture = repository.SaveRenderTextureAsPng(renderTexture, saveBasePathNoExt);
                        renderTexture.Release();
                    }
                    else
                    {
                        // CPU処理
                        var pixels = repository.LoadPixels(normalTexture, out var width, out var height);
                        
                        // チャンネル反転
                        cpuProcessor.InvertChannels(pixels, invertR, invertG, false, false);
                        
                        // 強度調整
                        cpuProcessor.AdjustNormalStrength(pixels, strength.Value);
                        
                        savedTexture = repository.SavePixelsAsPng(pixels, width, height, saveBasePathNoExt);
                    }
                });

                return savedTexture != null
                    ? Result<Texture2D>.Ok(savedTexture)
                    : Result<Texture2D>.Fail("法線マップの調整に失敗しました。");
            }
            catch (Exception ex)
            {
                return Result<Texture2D>.Fail($"法線マップ編集中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 法線マップの調整（float値の強度指定版）
        /// </summary>
        /// <param name="normalTexture">法線マップテクスチャ</param>
        /// <param name="invertR">Rチャンネルを反転するか</param>
        /// <param name="invertG">Gチャンネルを反転するか</param>
        /// <param name="strengthValue">法線の強度値</param>
        /// <param name="saveBasePathNoExt">保存先ベースパス（拡張子なし）</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <param name="cpuProcessor">CPU処理サービス</param>
        /// <param name="gpuProcessor">GPU処理サービス（オプション）</param>
        /// <returns>調整処理結果</returns>
        public static Result<Texture2D> AdjustNormalMap(
            Texture2D normalTexture,
            bool invertR, bool invertG,
            float strengthValue,
            string saveBasePathNoExt,
            ITextureRepository repository,
            ITextureProcessingService cpuProcessor,
            IGpuTextureOperationService gpuProcessor = null)
        {
            try
            {
                var strength = new NormalStrength(strengthValue);
                return AdjustNormalMap(normalTexture, invertR, invertG, strength, saveBasePathNoExt, repository, cpuProcessor, gpuProcessor);
            }
            catch (ArgumentException ex)
            {
                return Result<Texture2D>.Fail($"無効な強度値です: {ex.Message}");
            }
        }

        /// <summary>
        /// 法線マップの処理が可能かどうかを判定
        /// </summary>
        /// <param name="texture">判定対象のテクスチャ</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <returns>処理可能な場合true</returns>
        public static bool CanProcessNormalMap(Texture2D texture, ITextureRepository repository)
        {
            if (texture == null || repository == null) return false;
            
            var assetPath = repository.GetAssetPath(texture);
            return !string.IsNullOrEmpty(assetPath);
        }

        /// <summary>
        /// 法線マップの情報を取得
        /// </summary>
        /// <param name="texture">法線マップテクスチャ</param>
        /// <returns>法線マップ情報</returns>
        public static string GetNormalMapInfo(Texture2D texture)
        {
            if (texture == null) return "テクスチャが選択されていません";
            
            return $"法線マップ: {texture.name} ({texture.width}x{texture.height}, {texture.format})";
        }
    }
}