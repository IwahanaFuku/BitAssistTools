using System;
using System.Collections.Generic;
using BitAssistTools.Domain.Models.Shared;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Domain.Repositories;
using BitAssistTools.Domain.Services;
using UnityEngine;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// マスク統合のアプリケーションサービス
    /// </summary>
    public static class MaskMergeService
    {
        /// <summary>
        /// 複数のマスクテクスチャを統合してRGBAパッキング
        /// </summary>
        /// <param name="maskMapping">マスクマッピング設定</param>
        /// <param name="outputWidth">出力テクスチャ幅</param>
        /// <param name="outputHeight">出力テクスチャ高さ</param>
        /// <param name="saveBasePathNoExt">保存先ベースパス（拡張子なし）</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <param name="cpuProcessor">CPU処理サービス</param>
        /// <param name="gpuProcessor">GPU処理サービス（オプション）</param>
        /// <returns>統合処理結果</returns>
        public static Result<Texture2D> MergeMasks(
            MaskMapping maskMapping,
            int outputWidth, int outputHeight,
            string saveBasePathNoExt,
            ITextureRepository repository,
            ITextureProcessingService cpuProcessor,
            IGpuTextureOperationService gpuProcessor = null)
        {
            try
            {
                if (maskMapping == null)
                    return Result<Texture2D>.Fail("マスクマッピングが設定されていません。");

                if (!maskMapping.HasValidSources)
                    return Result<Texture2D>.Fail("有効なマスクソースが設定されていません。");

                if (repository == null)
                    return Result<Texture2D>.Fail("テクスチャリポジトリが設定されていません。");

                if (cpuProcessor == null)
                    return Result<Texture2D>.Fail("CPU処理サービスが設定されていません。");

                if (string.IsNullOrEmpty(saveBasePathNoExt))
                    return Result<Texture2D>.Fail("保存先パスが指定されていません。");

                if (outputWidth <= 0 || outputHeight <= 0)
                    return Result<Texture2D>.Fail("出力サイズが無効です。");

                Texture2D savedTexture = null;

                // 使用するテクスチャを収集
                var texturesToProcess = new List<Texture2D>();
                if (maskMapping.OutR?.IsValid == true) texturesToProcess.Add(maskMapping.OutR.Source);
                if (maskMapping.OutG?.IsValid == true) texturesToProcess.Add(maskMapping.OutG.Source);
                if (maskMapping.OutB?.IsValid == true) texturesToProcess.Add(maskMapping.OutB.Source);
                if (maskMapping.OutA?.IsValid == true) texturesToProcess.Add(maskMapping.OutA.Source);

                // 処理実行
                ProcessTexturesWithTypeDefault(texturesToProcess, repository, gpuProcessor == null, () =>
                {
                    if (gpuProcessor != null && gpuProcessor.IsSupported)
                    {
                        // GPU処理
                        var renderTexture = gpuProcessor.MergeMasks(
                            maskMapping.OutR?.Source, (int)(maskMapping.OutR?.SourceChannel ?? Channel.R), maskMapping.OutR?.Invert ?? false,
                            maskMapping.OutG?.Source, (int)(maskMapping.OutG?.SourceChannel ?? Channel.G), maskMapping.OutG?.Invert ?? false,
                            maskMapping.OutB?.Source, (int)(maskMapping.OutB?.SourceChannel ?? Channel.B), maskMapping.OutB?.Invert ?? false,
                            maskMapping.OutA?.Source, (int)(maskMapping.OutA?.SourceChannel ?? Channel.A), maskMapping.OutA?.Invert ?? false,
                            new Vector2Int(outputWidth, outputHeight),
                            0f, 0f, 0f, 1f);

                        savedTexture = repository.SaveRenderTextureAsPng(renderTexture, saveBasePathNoExt);
                        renderTexture.Release();
                    }
                    else
                    {
                        // CPU処理
                        Color32[] rPixels = maskMapping.OutR?.IsValid == true 
                            ? repository.LoadPixels(maskMapping.OutR.Source, out _, out _) : null;
                        Color32[] gPixels = maskMapping.OutG?.IsValid == true 
                            ? repository.LoadPixels(maskMapping.OutG.Source, out _, out _) : null;
                        Color32[] bPixels = maskMapping.OutB?.IsValid == true 
                            ? repository.LoadPixels(maskMapping.OutB.Source, out _, out _) : null;
                        Color32[] aPixels = maskMapping.OutA?.IsValid == true 
                            ? repository.LoadPixels(maskMapping.OutA.Source, out _, out _) : null;

                        var outputPixels = new Color32[outputWidth * outputHeight];
                        cpuProcessor.MergeMasks(outputPixels, outputWidth, outputHeight,
                            rPixels, maskMapping.OutR?.SourceChannel ?? Channel.R, maskMapping.OutR?.Invert ?? false,
                            gPixels, maskMapping.OutG?.SourceChannel ?? Channel.G, maskMapping.OutG?.Invert ?? false,
                            bPixels, maskMapping.OutB?.SourceChannel ?? Channel.B, maskMapping.OutB?.Invert ?? false,
                            aPixels, maskMapping.OutA?.SourceChannel ?? Channel.A, maskMapping.OutA?.Invert ?? false);

                        savedTexture = repository.SavePixelsAsPng(outputPixels, outputWidth, outputHeight, saveBasePathNoExt);
                    }
                });

                return savedTexture != null
                    ? Result<Texture2D>.Ok(savedTexture)
                    : Result<Texture2D>.Fail("マスクの統合に失敗しました。");
            }
            catch (Exception ex)
            {
                return Result<Texture2D>.Fail($"マスク統合中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// マスク統合が可能かどうかを判定
        /// </summary>
        /// <param name="maskMapping">マスクマッピング設定</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <returns>統合可能な場合true</returns>
        public static bool CanMergeMasks(MaskMapping maskMapping, ITextureRepository repository)
        {
            if (maskMapping == null || repository == null) return false;
            return maskMapping.HasValidSources;
        }

        /// <summary>
        /// マスクマッピングの情報を取得
        /// </summary>
        /// <param name="maskMapping">マスクマッピング設定</param>
        /// <returns>マッピング情報文字列</returns>
        public static string GetMaskMappingInfo(MaskMapping maskMapping)
        {
            if (maskMapping == null) return "マスクマッピングが設定されていません";
            
            var validCount = maskMapping.ValidSourceCount;
            return $"マスクマッピング: {validCount}個の有効なソースが設定されています";
        }

        /// <summary>
        /// 複数のテクスチャの型を一時的にデフォルトに変更して処理を実行
        /// </summary>
        private static void ProcessTexturesWithTypeDefault(
            List<Texture2D> textures, 
            ITextureRepository repository, 
            bool needReadable, 
            Action action)
        {
            ProcessTexturesRecursive(textures, 0, repository, needReadable, action);
        }

        /// <summary>
        /// 再帰的にテクスチャの型を変更して処理
        /// </summary>
        private static void ProcessTexturesRecursive(
            List<Texture2D> textures, 
            int index, 
            ITextureRepository repository, 
            bool needReadable, 
            Action action)
        {
            if (index >= textures.Count)
            {
                action();
                return;
            }

            var texture = textures[index];
            if (texture != null)
            {
                repository.WithTextureTypeDefault(texture, needReadable, () =>
                {
                    ProcessTexturesRecursive(textures, index + 1, repository, needReadable, action);
                });
            }
            else
            {
                ProcessTexturesRecursive(textures, index + 1, repository, needReadable, action);
            }
        }
    }
}