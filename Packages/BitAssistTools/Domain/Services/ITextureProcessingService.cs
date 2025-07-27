using UnityEngine;
using BitAssistTools.Domain.Models.Texture;

namespace BitAssistTools.Domain.Services
{
    /// <summary>
    /// CPU テクスチャ処理のドメインサービスインターフェース
    /// </summary>
    public interface ITextureProcessingService
    {
        /// <summary>
        /// 全チャンネルを反転
        /// </summary>
        /// <param name="pixels">ピクセルデータ</param>
        void InvertAllChannels(Color32[] pixels);

        /// <summary>
        /// 指定されたチャンネルを反転
        /// </summary>
        /// <param name="pixels">ピクセルデータ</param>
        /// <param name="invertR">Rチャンネルを反転するか</param>
        /// <param name="invertG">Gチャンネルを反転するか</param>
        /// <param name="invertB">Bチャンネルを反転するか</param>
        /// <param name="invertA">Aチャンネルを反転するか</param>
        void InvertChannels(Color32[] pixels, bool invertR, bool invertG, bool invertB, bool invertA);

        /// <summary>
        /// 複数のマスクを統合（基本版）
        /// </summary>
        /// <param name="outputPixels">出力ピクセル配列</param>
        /// <param name="width">テクスチャ幅</param>
        /// <param name="height">テクスチャ高さ</param>
        /// <param name="rSource">Rチャンネル用ソースピクセル</param>
        /// <param name="rChannel">Rチャンネル用ソースチャンネル</param>
        /// <param name="gSource">Gチャンネル用ソースピクセル</param>
        /// <param name="gChannel">Gチャンネル用ソースチャンネル</param>
        /// <param name="bSource">Bチャンネル用ソースピクセル</param>
        /// <param name="bChannel">Bチャンネル用ソースチャンネル</param>
        /// <param name="aSource">Aチャンネル用ソースピクセル</param>
        /// <param name="aChannel">Aチャンネル用ソースチャンネル</param>
        void MergeMasks(
            Color32[] outputPixels, int width, int height,
            Color32[] rSource, Channel rChannel,
            Color32[] gSource, Channel gChannel,
            Color32[] bSource, Channel bChannel,
            Color32[] aSource, Channel aChannel);

        /// <summary>
        /// 複数のマスクを統合（反転サポート版）
        /// </summary>
        /// <param name="outputPixels">出力ピクセル配列</param>
        /// <param name="width">テクスチャ幅</param>
        /// <param name="height">テクスチャ高さ</param>
        /// <param name="rSource">Rチャンネル用ソースピクセル</param>
        /// <param name="rChannel">Rチャンネル用ソースチャンネル</param>
        /// <param name="rInvert">Rチャンネルを反転するか</param>
        /// <param name="gSource">Gチャンネル用ソースピクセル</param>
        /// <param name="gChannel">Gチャンネル用ソースチャンネル</param>
        /// <param name="gInvert">Gチャンネルを反転するか</param>
        /// <param name="bSource">Bチャンネル用ソースピクセル</param>
        /// <param name="bChannel">Bチャンネル用ソースチャンネル</param>
        /// <param name="bInvert">Bチャンネルを反転するか</param>
        /// <param name="aSource">Aチャンネル用ソースピクセル</param>
        /// <param name="aChannel">Aチャンネル用ソースチャンネル</param>
        /// <param name="aInvert">Aチャンネルを反転するか</param>
        void MergeMasks(
            Color32[] outputPixels, int width, int height,
            Color32[] rSource, Channel rChannel, bool rInvert,
            Color32[] gSource, Channel gChannel, bool gInvert,
            Color32[] bSource, Channel bChannel, bool bInvert,
            Color32[] aSource, Channel aChannel, bool aInvert);

        /// <summary>
        /// 法線マップの強度を調整
        /// </summary>
        /// <param name="pixels">法線マップピクセルデータ</param>
        /// <param name="strength">強度値</param>
        void AdjustNormalStrength(Color32[] pixels, float strength);
    }
}