using System;
using TextureTools.Application.Ports;
using TextureTools.Domain.Services;
using TextureTools.Shared;
using UnityEngine;

namespace TextureTools.Application.Services
{
    public interface IAdjustNormalMapUseCase
    {
        Result<Texture2D> Execute(Texture2D normalTex, bool invertR, bool invertG, float strength, string saveBasePathNoExt, IGpuTextureOps gpu = null);
    }

    public sealed class AdjustNormalMapUseCase : IAdjustNormalMapUseCase
    {
        private readonly ITextureRepository _repo;
        private readonly ITextureProcessor _cpu;

        public AdjustNormalMapUseCase(ITextureRepository repo, ITextureProcessor cpu)
        { _repo = repo; _cpu = cpu; }

        public Result<Texture2D> Execute(Texture2D src, bool invR, bool invG, float strength, string saveBase, IGpuTextureOps gpu = null)
        {
            if (src == null) return Result<Texture2D>.Fail("No texture selected.");

            Texture2D saved = null;
            _repo.WithTextureTypeDefault(src, needReadable: gpu == null, action: () =>
            {
                if (gpu != null && gpu.IsSupported)
                {
                    var rt = gpu.NormalOps(src, invR, invG, Mathf.Max(0f, strength), new Vector2Int(src.width, src.height));
                    saved = _repo.SaveRenderTextureAsPng(rt, saveBase);
                    rt.Release();
                }
                else
                {
                    var px = _repo.LoadPixels(src, out var w, out var h);
                    _cpu.InvertChannels(px, invR, invG, false, false);
                    _cpu.AdjustNormalStrength(px, Mathf.Max(0f, strength));
                    saved = _repo.SavePixelsAsPng(px, w, h, saveBase);
                }
            });

            return saved ? Result<Texture2D>.Ok(saved) : Result<Texture2D>.Fail("Normal adjust failed.");
        }
    }
}
