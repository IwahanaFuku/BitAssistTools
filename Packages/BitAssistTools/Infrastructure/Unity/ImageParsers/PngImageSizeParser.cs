using System.IO;
using BitAssistTools.Domain.Services.TextureTools;

namespace BitAssistTools.Infrastructure.Unity.ImageParsers
{
    /// <summary>
    /// PNGファイルのサイズ解析クラス
    /// </summary>
    public class PngImageSizeParser : IImageSizeParser
    {
        /// <summary>
        /// 対応する拡張子
        /// </summary>
        public string[] SupportedExtensions => new[] { ".png" };

        /// <summary>
        /// 指定された拡張子をサポートしているかチェック
        /// </summary>
        public bool SupportsExtension(string extension)
        {
            return extension?.ToLower() == ".png";
        }

        /// <summary>
        /// PNGファイルからサイズを解析
        /// </summary>
        public (int width, int height)? ParseImageSize(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                
                var buffer = new byte[24];
                if (stream.Read(buffer, 0, 24) != 24) return null;
                
                // PNG signature check (89 50 4E 47 0D 0A 1A 0A)
                if (buffer[0] != 0x89 || buffer[1] != 0x50 || buffer[2] != 0x4E || buffer[3] != 0x47) 
                    return null;
                
                // IHDR chunk should start at byte 12
                if (buffer[12] != 'I' || buffer[13] != 'H' || buffer[14] != 'D' || buffer[15] != 'R') 
                    return null;
                
                // Extract width and height (big-endian format)
                int width = (buffer[16] << 24) | (buffer[17] << 16) | (buffer[18] << 8) | buffer[19];
                int height = (buffer[20] << 24) | (buffer[21] << 16) | (buffer[22] << 8) | buffer[23];
                
                return (width, height);
            }
            catch
            {
                return null;
            }
        }
    }
}