using System;
using UnityEngine;

namespace BitAssistTools.Domain.Repositories
{
    /// <summary>
    /// テクスチャ操作のリポジトリインターフェース
    /// </summary>
    public interface ITextureRepository
    {
        /// <summary>
        /// テクスチャからピクセルデータを読み込み
        /// </summary>
        Color32[] LoadPixels(Texture2D texture, out int width, out int height);

        /// <summary>
        /// ピクセルデータをPNGとして保存
        /// </summary>
        Texture2D SavePixelsAsPng(Color32[] pixels, int width, int height, string basePathNoExt);

        /// <summary>
        /// RenderTextureをPNGとして保存
        /// </summary>
        Texture2D SaveRenderTextureAsPng(RenderTexture renderTexture, string basePathNoExt);

        /// <summary>
        /// アセットを削除
        /// </summary>
        bool DeleteAsset(string assetPath);

        /// <summary>
        /// オブジェクトのアセットパスを取得
        /// </summary>
        string GetAssetPath(UnityEngine.Object obj);

        /// <summary>
        /// 空のテクスチャを作成
        /// </summary>
        Texture2D CreateEmpty(int width, int height, TextureFormat format = TextureFormat.RGBA32, bool linear = true);

        /// <summary>
        /// テクスチャの設定を一時的にデフォルトに変更して処理を実行
        /// </summary>
        void WithTextureTypeDefault(Texture2D texture, bool needReadable, Action action);

        /// <summary>
        /// テクスチャの設定を一時的にデフォルトに変更して処理を実行（戻り値あり）
        /// </summary>
        T WithTextureTypeDefault<T>(Texture2D texture, bool needReadable, Func<T> func);
    }
}