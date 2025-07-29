using UnityEngine;

namespace BitAssistTools.Domain.Services
{
    /// <summary>
    /// GPU テクスチャ操作のドメインサービスインターフェース
    /// </summary>
    public interface IGpuTextureOperationService
    {
        /// <summary>GPU操作がサポートされているかどうか</summary>
        bool IsSupported { get; }

        /// <summary>
        /// テクスチャのチャンネルを反転
        /// </summary>
        /// <param name="source">ソーステクスチャ</param>
        /// <param name="invertR">Rチャンネルを反転するか</param>
        /// <param name="invertG">Gチャンネルを反転するか</param>
        /// <param name="invertB">Bチャンネルを反転するか</param>
        /// <param name="invertA">Aチャンネルを反転するか</param>
        /// <param name="outputSize">出力サイズ</param>
        /// <returns>反転処理されたRenderTexture</returns>
        RenderTexture InvertChannels(Texture source, bool invertR, bool invertG, bool invertB, bool invertA, Vector2Int outputSize);

        /// <summary>
        /// 複数のマスクテクスチャを統合
        /// </summary>
        /// <param name="rSource">Rチャンネル用ソース</param>
        /// <param name="rChannel">Rチャンネル用ソースチャンネル</param>
        /// <param name="rInvert">Rチャンネルを反転するか</param>
        /// <param name="gSource">Gチャンネル用ソース</param>
        /// <param name="gChannel">Gチャンネル用ソースチャンネル</param>
        /// <param name="gInvert">Gチャンネルを反転するか</param>
        /// <param name="bSource">Bチャンネル用ソース</param>
        /// <param name="bChannel">Bチャンネル用ソースチャンネル</param>
        /// <param name="bInvert">Bチャンネルを反転するか</param>
        /// <param name="aSource">Aチャンネル用ソース</param>
        /// <param name="aChannel">Aチャンネル用ソースチャンネル</param>
        /// <param name="aInvert">Aチャンネルを反転するか</param>
        /// <param name="outputSize">出力サイズ</param>
        /// <param name="rFill">Rチャンネルのデフォルト値</param>
        /// <param name="gFill">Gチャンネルのデフォルト値</param>
        /// <param name="bFill">Bチャンネルのデフォルト値</param>
        /// <param name="aFill">Aチャンネルのデフォルト値</param>
        /// <returns>統合されたRenderTexture</returns>
        RenderTexture MergeMasks(
            Texture rSource, int rChannel, bool rInvert,
            Texture gSource, int gChannel, bool gInvert,
            Texture bSource, int bChannel, bool bInvert,
            Texture aSource, int aChannel, bool aInvert,
            Vector2Int outputSize,
            float rFill = 0f, float gFill = 0f, float bFill = 0f, float aFill = 1f);

        /// <summary>
        /// 法線マップの処理（反転と強度調整）
        /// </summary>
        /// <param name="source">ソース法線マップ</param>
        /// <param name="invertR">Rチャンネルを反転するか</param>
        /// <param name="invertG">Gチャンネルを反転するか</param>
        /// <param name="strength">法線の強度</param>
        /// <param name="outputSize">出力サイズ</param>
        /// <returns>処理された法線マップのRenderTexture</returns>
        RenderTexture ProcessNormalMap(Texture source, bool invertR, bool invertG, float strength, Vector2Int outputSize);
    }
}