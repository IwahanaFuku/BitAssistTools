using System;
using BitAssistTools.Domain.Models.Shared;
using BitAssistTools.Domain.Repositories;
using UnityEngine;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// テクスチャ変換のアプリケーションサービス
    /// </summary>
    public static class TextureConversionService
    {
        /// <summary>
        /// テクスチャをPNG形式に変換
        /// </summary>
        /// <param name="sourceTexture">変換対象のテクスチャ</param>
        /// <param name="deleteOriginal">元のアセットを削除するかどうか</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <returns>変換結果</returns>
        public static Result<Texture2D> ConvertToPng(Texture2D sourceTexture, bool deleteOriginal, ITextureRepository repository)
        {
            try
            {
                if (sourceTexture == null) 
                    return Result<Texture2D>.Fail("変換対象のテクスチャが選択されていません。");

                if (repository == null)
                    return Result<Texture2D>.Fail("テクスチャリポジトリが設定されていません。");

                var sourcePath = repository.GetAssetPath(sourceTexture);
                if (string.IsNullOrEmpty(sourcePath)) 
                    return Result<Texture2D>.Fail("アセットパスが見つかりません。");

                // 保存先パスを生成（拡張子なし）
                var basePathNoExt = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(sourcePath),
                    System.IO.Path.GetFileNameWithoutExtension(sourcePath)
                ).Replace('\\', '/');

                Texture2D savedTexture = null;
                repository.WithTextureTypeDefault(sourceTexture, needReadable: true, action: () =>
                {
                    var pixels = repository.LoadPixels(sourceTexture, out var width, out var height);
                    savedTexture = repository.SavePixelsAsPng(pixels, width, height, basePathNoExt);
                });

                if (savedTexture == null)
                    return Result<Texture2D>.Fail("PNG変換に失敗しました。");

                // 元のアセットを削除（必要に応じて）
                if (deleteOriginal)
                {
                    if (!repository.DeleteAsset(sourcePath))
                        return Result<Texture2D>.Fail("元のアセットの削除に失敗しました。");
                }

                return Result<Texture2D>.Ok(savedTexture);
            }
            catch (Exception ex)
            {
                return Result<Texture2D>.Fail($"PNG変換中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 複数のテクスチャをバッチでPNG変換
        /// </summary>
        /// <param name="sourceTextures">変換対象のテクスチャ配列</param>
        /// <param name="deleteOriginal">元のアセットを削除するかどうか</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <returns>変換結果の配列</returns>
        public static Result<Texture2D>[] ConvertToPngBatch(Texture2D[] sourceTextures, bool deleteOriginal, ITextureRepository repository)
        {
            if (sourceTextures == null || sourceTextures.Length == 0)
                return new[] { Result<Texture2D>.Fail("変換対象のテクスチャが指定されていません。") };

            var results = new Result<Texture2D>[sourceTextures.Length];
            for (int i = 0; i < sourceTextures.Length; i++)
            {
                results[i] = ConvertToPng(sourceTextures[i], deleteOriginal, repository);
            }

            return results;
        }

        /// <summary>
        /// テクスチャがPNG変換可能かどうかを判定
        /// </summary>
        /// <param name="texture">判定対象のテクスチャ</param>
        /// <param name="repository">テクスチャリポジトリ</param>
        /// <returns>変換可能な場合true</returns>
        public static bool CanConvertToPng(Texture2D texture, ITextureRepository repository)
        {
            if (texture == null || repository == null) return false;
            
            var assetPath = repository.GetAssetPath(texture);
            return !string.IsNullOrEmpty(assetPath);
        }
    }
}