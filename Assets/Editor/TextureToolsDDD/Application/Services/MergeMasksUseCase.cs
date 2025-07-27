using System;
using TextureTools.Application.Ports;
using TextureTools.Domain.Model;
using TextureTools.Domain.Services;
using TextureTools.Shared;
using UnityEngine;

namespace TextureTools.Application.Services
{
    public interface IMergeMasksUseCase
    {
        Result<Texture2D> Execute(MaskMapping mapping, int width, int height, string saveBasePathNoExt, IGpuTextureOps gpu = null);
    }

    public sealed class MergeMasksUseCase : IMergeMasksUseCase
    {
        private readonly ITextureRepository _repo;
        private readonly ITextureProcessor _cpu;

        public MergeMasksUseCase(ITextureRepository repo, ITextureProcessor cpu)
        { _repo = repo; _cpu = cpu; }

        public Result<Texture2D> Execute(MaskMapping m, int w, int h, string saveBase, IGpuTextureOps gpu = null)
        {
            Texture2D saved = null;

            Action run = () =>
            {
                if (gpu != null && gpu.IsSupported)
                {
                    var rt = gpu.MergeMasks(
                        m.OutR?.Source, (int)(m.OutR?.SourceChannel ?? Channel.R), (m.OutR?.Invert ?? false),
                        m.OutG?.Source, (int)(m.OutG?.SourceChannel ?? Channel.G), (m.OutG?.Invert ?? false),
                        m.OutB?.Source, (int)(m.OutB?.SourceChannel ?? Channel.B), (m.OutB?.Invert ?? false),
                        m.OutA?.Source, (int)(m.OutA?.SourceChannel ?? Channel.A), (m.OutA?.Invert ?? false),
                        new Vector2Int(w, h),
                        0f, 0f, 0f, 1f
                    );
                    saved = _repo.SaveRenderTextureAsPng(rt, saveBase);
                    rt.Release();
                }
                else
                {
                    Color32[] rPix = m.OutR?.Source != null ? _repo.LoadPixels(m.OutR.Source, out var _, out var _) : null;
                    Color32[] gPix = m.OutG?.Source != null ? _repo.LoadPixels(m.OutG.Source, out var _, out var _) : null;
                    Color32[] bPix = m.OutB?.Source != null ? _repo.LoadPixels(m.OutB.Source, out var _, out var _) : null;
                    Color32[] aPix = m.OutA?.Source != null ? _repo.LoadPixels(m.OutA.Source, out var _, out var _) : null;

                    var outPx = new Color32[w * h];
                    _cpu.MergeMasks(outPx, w, h,
                        rPix, m.OutR?.SourceChannel ?? Channel.R, (m.OutR?.Invert ?? false),
                        gPix, m.OutG?.SourceChannel ?? Channel.G, (m.OutG?.Invert ?? false),
                        bPix, m.OutB?.SourceChannel ?? Channel.B, (m.OutB?.Invert ?? false),
                        aPix, m.OutA?.SourceChannel ?? Channel.A, (m.OutA?.Invert ?? false));

                    saved = _repo.SavePixelsAsPng(outPx, w, h, saveBase);
                }
            };

            var needReadable = (gpu == null);
            // （以下の WithTextureTypeDefault ネストは前回のまま。省略せずそのまま使用）
            if (m.OutR?.Source != null)
                _repo.WithTextureTypeDefault(m.OutR.Source, needReadable, () =>
                {
                    if (m.OutG?.Source != null)
                        _repo.WithTextureTypeDefault(m.OutG.Source, needReadable, () =>
                        {
                            if (m.OutB?.Source != null)
                                _repo.WithTextureTypeDefault(m.OutB.Source, needReadable, () =>
                                {
                                    if (m.OutA?.Source != null)
                                        _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                                    else
                                        run();
                                });
                            else
                            {
                                if (m.OutA?.Source != null)
                                    _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                                else
                                    run();
                            }
                        });
                    else
                    {
                        if (m.OutB?.Source != null)
                            _repo.WithTextureTypeDefault(m.OutB.Source, needReadable, () =>
                            {
                                if (m.OutA?.Source != null)
                                    _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                                else
                                    run();
                            });
                        else
                        {
                            if (m.OutA?.Source != null)
                                _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                            else
                                run();
                        }
                    }
                });
            else
            {
                if (m.OutG?.Source != null)
                    _repo.WithTextureTypeDefault(m.OutG.Source, needReadable, () =>
                    {
                        if (m.OutB?.Source != null)
                            _repo.WithTextureTypeDefault(m.OutB.Source, needReadable, () =>
                            {
                                if (m.OutA?.Source != null)
                                    _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                                else
                                    run();
                            });
                        else
                        {
                            if (m.OutA?.Source != null)
                                _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                            else
                                run();
                        }
                    });
                else
                {
                    if (m.OutB?.Source != null)
                        _repo.WithTextureTypeDefault(m.OutB.Source, needReadable, () =>
                        {
                            if (m.OutA?.Source != null)
                                _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                            else
                                run();
                        });
                    else
                    {
                        if (m.OutA?.Source != null)
                            _repo.WithTextureTypeDefault(m.OutA.Source, needReadable, run);
                        else
                            run();
                    }
                }
            }

            return saved ? Result<Texture2D>.Ok(saved) : Result<Texture2D>.Fail("Merge failed.");
        }
    }
}
