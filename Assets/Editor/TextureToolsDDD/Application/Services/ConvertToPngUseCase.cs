using System;
using TextureTools.Application.Ports;
using TextureTools.Shared;
using UnityEngine;

namespace TextureTools.Application.Services
{
    public interface IConvertToPngUseCase
    {
        Result<Texture2D> Execute(Texture2D src, bool deleteOriginal);
    }

    public sealed class ConvertToPngUseCase : IConvertToPngUseCase
    {
        private readonly ITextureRepository _repo;
        public ConvertToPngUseCase(ITextureRepository repo) { _repo = repo; }

        public Result<Texture2D> Execute(Texture2D src, bool deleteOriginal)
        {
            if (src == null) return Result<Texture2D>.Fail("No texture selected.");
            var srcPath = _repo.GetAssetPath(src);
            if (string.IsNullOrEmpty(srcPath)) return Result<Texture2D>.Fail("Asset path not found.");

            var baseNoExt = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(srcPath),
                System.IO.Path.GetFileNameWithoutExtension(srcPath)
            ).Replace('\\','/');

            Texture2D saved = null;
            _repo.WithTextureTypeDefault(src, needReadable: true, action: () =>
            {
                var pixels = _repo.LoadPixels(src, out var w, out var h);
                saved = _repo.SavePixelsAsPng(pixels, w, h, baseNoExt);
            });

            if (deleteOriginal)
            {
                _repo.DeleteAsset(srcPath);
            }
            return saved ? Result<Texture2D>.Ok(saved) : Result<Texture2D>.Fail("Convert failed.");
        }
    }
}
