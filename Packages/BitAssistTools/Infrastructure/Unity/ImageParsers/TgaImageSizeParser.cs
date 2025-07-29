using System.IO;
using BitAssistTools.Domain.Services.TextureTools;

namespace BitAssistTools.Infrastructure.Unity.ImageParsers
{
    /// <summary>
    /// TGAファイルのサイズ解析クラス
    /// </summary>
    public class TgaImageSizeParser : IImageSizeParser
    {
        /// <summary>
        /// 対応する拡張子
        /// </summary>
        public string[] SupportedExtensions => new[] { ".tga" };

        /// <summary>
        /// 指定された拡張子をサポートしているかチェック
        /// </summary>
        public bool SupportsExtension(string extension)
        {
            return extension?.ToLower() == ".tga";
        }

        /// <summary>
        /// TGAファイルからサイズを解析
        /// </summary>
        public (int width, int height)? ParseImageSize(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                
                var buffer = new byte[18];
                if (stream.Read(buffer, 0, 18) != 18) return null;
                
                // TGA header structure:
                // bytes 12-13: width (little-endian)
                // bytes 14-15: height (little-endian)
                int width = buffer[12] | (buffer[13] << 8);
                int height = buffer[14] | (buffer[15] << 8);
                
                // Basic validation - width and height should be positive
                if (width <= 0 || height <= 0) return null;
                
                return (width, height);
            }
            catch
            {
                return null;
            }
        }
    }
}