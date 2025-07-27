using System;
using BitAssistTools.Domain.Services;
using UnityEngine;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// Unity ComputeShader を使用したGPUテクスチャ操作サービス
    /// </summary>
    public sealed class UnityGpuTextureOperationService : IGpuTextureOperationService
    {
        private readonly ComputeShader _computeShader;
        private readonly int _kernelInvert;
        private readonly int _kernelMerge;
        private readonly int _kernelNormal;

        /// <summary>
        /// GPU操作がサポートされているかどうか
        /// </summary>
        public bool IsSupported => SystemInfo.supportsComputeShaders && _computeShader != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="computeShader">使用するComputeShader</param>
        public UnityGpuTextureOperationService(ComputeShader computeShader)
        {
            _computeShader = computeShader ?? throw new ArgumentNullException(nameof(computeShader));
            
            // カーネルを取得
            _kernelInvert = _computeShader.FindKernel("Invert");
            _kernelMerge = _computeShader.FindKernel("MergeMasks");
            _kernelNormal = _computeShader.FindKernel("NormalOps");
        }

        /// <summary>
        /// テクスチャのチャンネルを反転
        /// </summary>
        public RenderTexture InvertChannels(Texture source, bool invertR, bool invertG, bool invertB, bool invertA, Vector2Int outputSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!IsSupported)
                throw new InvalidOperationException("GPU compute shaders are not supported on this platform");

            var renderTexture = CreateRenderTexture(outputSize);
            
            _computeShader.SetTexture(_kernelInvert, "_Src", source);
            _computeShader.SetTexture(_kernelInvert, "_Dst", renderTexture);
            SetSizeParameters(_kernelInvert, outputSize);
            _computeShader.SetInt("_InvR", invertR ? 1 : 0);
            _computeShader.SetInt("_InvG", invertG ? 1 : 0);
            _computeShader.SetInt("_InvB", invertB ? 1 : 0);
            _computeShader.SetInt("_InvA", invertA ? 1 : 0);
            
            DispatchCompute(_kernelInvert, outputSize);
            return renderTexture;
        }

        /// <summary>
        /// 複数のマスクテクスチャを統合
        /// </summary>
        public RenderTexture MergeMasks(
            Texture rSource, int rChannel, bool rInvert,
            Texture gSource, int gChannel, bool gInvert,
            Texture bSource, int bChannel, bool bInvert,
            Texture aSource, int aChannel, bool aInvert,
            Vector2Int outputSize,
            float rFill = 0f, float gFill = 0f, float bFill = 0f, float aFill = 1f)
        {
            if (!IsSupported)
                throw new InvalidOperationException("GPU compute shaders are not supported on this platform");

            var renderTexture = CreateRenderTexture(outputSize);
            
            // ソーステクスチャを設定
            _computeShader.SetTexture(_kernelMerge, "_Rsrc", rSource);
            _computeShader.SetTexture(_kernelMerge, "_Gsrc", gSource);
            _computeShader.SetTexture(_kernelMerge, "_Bsrc", bSource);
            _computeShader.SetTexture(_kernelMerge, "_Asrc", aSource);
            _computeShader.SetTexture(_kernelMerge, "_Dst", renderTexture);

            SetSizeParameters(_kernelMerge, outputSize);
            
            // チャンネル設定
            _computeShader.SetInt("_Rch", rChannel);
            _computeShader.SetInt("_Gch", gChannel);
            _computeShader.SetInt("_Bch", bChannel);
            _computeShader.SetInt("_Ach", aChannel);

            // 反転フラグ設定
            _computeShader.SetInt("_InvRsrc", rInvert ? 1 : 0);
            _computeShader.SetInt("_InvGsrc", gInvert ? 1 : 0);
            _computeShader.SetInt("_InvBsrc", bInvert ? 1 : 0);
            _computeShader.SetInt("_InvAsrc", aInvert ? 1 : 0);

            // デフォルト値設定
            _computeShader.SetFloat("_Rfill", rFill);
            _computeShader.SetFloat("_Gfill", gFill);
            _computeShader.SetFloat("_Bfill", bFill);
            _computeShader.SetFloat("_Afill", aFill);

            // ソース有効フラグ設定
            _computeShader.SetInt("_HasR", rSource != null ? 1 : 0);
            _computeShader.SetInt("_HasG", gSource != null ? 1 : 0);
            _computeShader.SetInt("_HasB", bSource != null ? 1 : 0);
            _computeShader.SetInt("_HasA", aSource != null ? 1 : 0);

            DispatchCompute(_kernelMerge, outputSize);
            return renderTexture;
        }

        /// <summary>
        /// 法線マップの処理（反転と強度調整）
        /// </summary>
        public RenderTexture ProcessNormalMap(Texture source, bool invertR, bool invertG, float strength, Vector2Int outputSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!IsSupported)
                throw new InvalidOperationException("GPU compute shaders are not supported on this platform");
            if (strength < 0f)
                throw new ArgumentOutOfRangeException(nameof(strength), "Strength must be non-negative");

            var renderTexture = CreateRenderTexture(outputSize);
            
            _computeShader.SetTexture(_kernelNormal, "_Src", source);
            _computeShader.SetTexture(_kernelNormal, "_Dst", renderTexture);
            SetSizeParameters(_kernelNormal, outputSize);
            _computeShader.SetInt("_NInvR", invertR ? 1 : 0);
            _computeShader.SetInt("_NInvG", invertG ? 1 : 0);
            _computeShader.SetFloat("_Strength", Mathf.Max(0f, strength));
            
            DispatchCompute(_kernelNormal, outputSize);
            return renderTexture;
        }

        #region Private Helper Methods

        /// <summary>
        /// RenderTextureを作成
        /// </summary>
        private static RenderTexture CreateRenderTexture(Vector2Int size)
        {
            var renderTexture = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            renderTexture.Create();
            return renderTexture;
        }

        /// <summary>
        /// サイズパラメータを設定
        /// </summary>
        private void SetSizeParameters(int kernel, Vector2Int size)
        {
            _computeShader.SetInts("_Size", size.x, size.y);
        }

        /// <summary>
        /// ComputeShaderをディスパッチ
        /// </summary>
        private void DispatchCompute(int kernel, Vector2Int size)
        {
            // 8x8のスレッドグループを使用
            int threadGroupsX = Mathf.CeilToInt(size.x / 8f);
            int threadGroupsY = Mathf.CeilToInt(size.y / 8f);
            _computeShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
        }

        #endregion
    }
}