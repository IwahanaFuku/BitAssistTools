using System.IO;

namespace BitAssistTools.Domain.Services.TextureTools
{
    /// <summary>
    /// 画像ファイルからサイズ情報を解析するインターフェース
    /// </summary>
    public interface IImageSizeParser
    {
        /// <summary>
        /// 対応する拡張子の配列
        /// </summary>
        string[] SupportedExtensions { get; }
        
        /// <summary>
        /// 指定された拡張子をサポートしているかチェック
        /// </summary>
        /// <param name="extension">拡張子（.を含む、小文字）</param>
        /// <returns>サポートしている場合はtrue</returns>
        bool SupportsExtension(string extension);
        
        /// <summary>
        /// ストリームから画像サイズを解析
        /// </summary>
        /// <param name="stream">画像ファイルのストリーム</param>
        /// <returns>解析に成功した場合は幅と高さ、失敗した場合はnull</returns>
        (int width, int height)? ParseImageSize(Stream stream);
    }
}