using System;
using TextureTools.Application.Ports;
using TextureTools.Domain.Services;
using TextureTools.Shared;
using UnityEngine;

namespace TextureTools.Application.Services
{
    public interface IInvertTextureUseCase
    {
        Result<Texture2D> Execute(Texture2D src, bool r, bool g, bool b, bool a, string saveBasePathNoExt, IGpuTextureOps gpu = null);
    }

    public sealed class InvertTextureUseCase : IInvertTextureUseCase
    {
        private readonly ITextureRepository _repo;
        private readonly ITextureProcessor _cpu;

        public InvertTextureUseCase(ITextureRepository repo, ITextureProcessor cpu)
        {
            _repo = repo; _cpu = cpu;
        }

        public Result<Texture2D> Execute(Texture2D src, bool r, bool g, bool b, bool a, string saveBase, IGpuTextureOps gpu = null)
        {
            if (src == null) return Result<Texture2D>.Fail("No texture selected.");

            Texture2D saved = null;

            // 処理中だけ TextureType=Default
            _repo.WithTextureTypeDefault(src, needReadable: gpu == null, action: () =>
            {
                if (gpu != null && gpu.IsSupported)
                {
                    var rt = gpu.Invert(src, r, g, b, a, new Vector2Int(src.width, src.height));
                    saved = _repo.SaveRenderTextureAsPng(rt, saveBase);
                    rt.Release();
                }
                else
                {
                    var px = _repo.LoadPixels(src, out var w, out var h);
                    _cpu.InvertChannels(px, r, g, b, a);
                    saved = _repo.SavePixelsAsPng(px, w, h, saveBase);
                }
            });

            return saved ? Result<Texture2D>.Ok(saved) : Result<Texture2D>.Fail("Invert failed.");
        }
    }
}
