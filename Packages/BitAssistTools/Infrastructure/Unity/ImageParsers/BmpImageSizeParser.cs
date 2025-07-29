using System.IO;
using BitAssistTools.Domain.Services.TextureTools;

namespace BitAssistTools.Infrastructure.Unity.ImageParsers
{
    /// <summary>
    /// BMPファイルのサイズ解析クラス
    /// </summary>
    public class BmpImageSizeParser : IImageSizeParser
    {
        /// <summary>
        /// 対応する拡張子
        /// </summary>
        public string[] SupportedExtensions => new[] { ".bmp" };

        /// <summary>
        /// 指定された拡張子をサポートしているかチェック
        /// </summary>
        public bool SupportsExtension(string extension)
        {
            return extension?.ToLower() == ".bmp";
        }

        /// <summary>
        /// BMPファイルからサイズを解析
        /// </summary>
        public (int width, int height)? ParseImageSize(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                
                var buffer = new byte[26];
                if (stream.Read(buffer, 0, 26) != 26) return null;
                
                // BMP signature check (BM)
                if (buffer[0] != 0x42 || buffer[1] != 0x4D) return null;
                
                // BMP header structure:
                // bytes 0-1: signature "BM"
                // bytes 2-5: file size
                // bytes 6-9: reserved
                // bytes 10-13: offset to pixel data
                // bytes 14-17: header size
                // bytes 18-21: width (little-endian)
                // bytes 22-25: height (little-endian)
                
                int width = buffer[18] | (buffer[19] << 8) | (buffer[20] << 16) | (buffer[21] << 24);
                int height = buffer[22] | (buffer[23] << 8) | (buffer[24] << 16) | (buffer[25] << 24);
                
                // Basic validation - width and height should be positive
                if (width <= 0 || height <= 0) return null;
                
                // BMPでは高さが負の値の場合、上下反転を意味することがある
                // 実際のサイズとしては絶対値を使用
                height = System.Math.Abs(height);
                
                return (width, height);
            }
            catch
            {
                return null;
            }
        }
    }
}