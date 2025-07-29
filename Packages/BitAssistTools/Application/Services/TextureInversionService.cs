using System;
using BitAssistTools.Domain.Models.Shared;
using BitAssistTools.Domain.Repositories;
using BitAssistTools.Domain.Services;
using UnityEngine;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// テクスチャ反転のアプリケーションサービス
    /// </summary>
    public static class TextureInversionService
    {
        /// <summary>
        /// テクスチャのチャンネルを反転
        /// </summary>
        /// <param name="sourceTexture">反転対象のテクスチャ</param>
        /// <param name="invertR">Rチャンネルを反転するか</param>
        /// <param name="invertG">Gチャンネルを反転するか</param>
        /// <param name="invertB">Bチャンネルを反転するか</param>
        /// <param name="invertA">Aチャンネルを反転するか</param>
        /// <param name="saveBasePathNoExt">保存先ベースパス（拡張子なし）</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <param name="cpuProcessor">CPU処理サービス</param>
        /// <param name="gpuProcessor">GPU処理サービス（オプション）</param>
        /// <returns>反転処理結果</returns>
        public static Result<Texture2D> InvertTexture(
            Texture2D sourceTexture, 
            bool invertR, bool invertG, bool invertB, bool invertA, 
            string saveBasePathNoExt,
            ITextureRepository repository,
            ITextureProcessingService cpuProcessor,
            IGpuTextureOperationService gpuProcessor = null)
        {
            try
            {
                if (sourceTexture == null) 
                    return Result<Texture2D>.Fail("反転対象のテクスチャが選択されていません。");

                if (repository == null)
                    return Result<Texture2D>.Fail("テクスチャリポジトリが設定されていません。");

                if (cpuProcessor == null)
                    return Result<Texture2D>.Fail("CPU処理サービスが設定されていません。");

                if (string.IsNullOrEmpty(saveBasePathNoExt))
                    return Result<Texture2D>.Fail("保存先パスが指定されていません。");

                // 反転するチャンネルが一つもない場合
                if (!invertR && !invertG && !invertB && !invertA)
                    return Result<Texture2D>.Fail("反転するチャンネルが選択されていません。");

                Texture2D savedTexture = null;

                // GPU処理が可能な場合はGPUを使用、そうでなければCPU処理
                repository.WithTextureTypeDefault(sourceTexture, needReadable: gpuProcessor == null, action: () =>
                {
                    if (gpuProcessor != null && gpuProcessor.IsSupported)
                    {
                        // GPU処理
                        var renderTexture = gpuProcessor.InvertChannels(
                            sourceTexture, invertR, invertG, invertB, invertA,
                            new Vector2Int(sourceTexture.width, sourceTexture.height));
                        
                        savedTexture = repository.SaveRenderTextureAsPng(renderTexture, saveBasePathNoExt);
                        renderTexture.Release();
                    }
                    else
                    {
                        // CPU処理
                        var pixels = repository.LoadPixels(sourceTexture, out var width, out var height);
                        cpuProcessor.InvertChannels(pixels, invertR, invertG, invertB, invertA);
                        savedTexture = repository.SavePixelsAsPng(pixels, width, height, saveBasePathNoExt);
                    }
                });

                return savedTexture != null 
                    ? Result<Texture2D>.Ok(savedTexture) 
                    : Result<Texture2D>.Fail("テクスチャの反転処理に失敗しました。");
            }
            catch (Exception ex)
            {
                return Result<Texture2D>.Fail($"テクスチャ反転中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// テクスチャの全チャンネルを反転
        /// </summary>
        /// <param name="sourceTexture">反転対象のテクスチャ</param>
        /// <param name="saveBasePathNoExt">保存先ベースパス（拡張子なし）</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <param name="cpuProcessor">CPU処理サービス</param>
        /// <param name="gpuProcessor">GPU処理サービス（オプション）</param>
        /// <returns>反転処理結果</returns>
        public static Result<Texture2D> InvertAllChannels(
            Texture2D sourceTexture,
            string saveBasePathNoExt,
            ITextureRepository repository,
            ITextureProcessingService cpuProcessor,
            IGpuTextureOperationService gpuProcessor = null)
        {
            return InvertTexture(sourceTexture, true, true, true, true, saveBasePathNoExt, repository, cpuProcessor, gpuProcessor);
        }

        /// <summary>
        /// テクスチャの反転が可能かどうかを判定
        /// </summary>
        /// <param name="texture">判定対象のテクスチャ</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <returns>反転可能な場合true</returns>
        public static bool CanInvertTexture(Texture2D texture, ITextureRepository repository)
        {
            if (texture == null || repository == null) return false;
            
            var assetPath = repository.GetAssetPath(texture);
            return !string.IsNullOrEmpty(assetPath);
        }
    }
}