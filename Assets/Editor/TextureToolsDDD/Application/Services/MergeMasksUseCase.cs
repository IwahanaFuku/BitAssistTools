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

            // 入力のあるものだけ Default 化して実行
            Action run = () =>
            {
                if (gpu != null && gpu.IsSupported)
                {
                    var rt = gpu.MergeMasks(
                        m.OutR?.Source, (int)(m.OutR?.SourceChannel ?? Channel.R),
                        m.OutG?.Source, (int)(m.OutG?.SourceChannel ?? Channel.G),
                        m.OutB?.Source, (int)(m.OutB?.SourceChannel ?? Channel.B),
                        m.OutA?.Source, (int)(m.OutA?.SourceChannel ?? Channel.A),
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
                        rPix, m.OutR?.SourceChannel ?? Channel.R,
                        gPix, m.OutG?.SourceChannel ?? Channel.G,
                        bPix, m.OutB?.SourceChannel ?? Channel.B,
                        aPix, m.OutA?.SourceChannel ?? Channel.A);

                    saved = _repo.SavePixelsAsPng(outPx, w, h, saveBase);
                }
            };

            // 4つをネストで Default 化（null はスキップ）。GPU では isReadable 不要。
            var needReadable = (gpu == null);
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
                // R が無い場合の入口
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
