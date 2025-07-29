using System.IO;
using BitAssistTools.Domain.Services.TextureTools;

namespace BitAssistTools.Infrastructure.Unity.ImageParsers
{
    /// <summary>
    /// JPEGファイルのサイズ解析クラス
    /// </summary>
    public class JpegImageSizeParser : IImageSizeParser
    {
        /// <summary>
        /// 対応する拡張子
        /// </summary>
        public string[] SupportedExtensions => new[] { ".jpg", ".jpeg" };

        /// <summary>
        /// 指定された拡張子をサポートしているかチェック
        /// </summary>
        public bool SupportsExtension(string extension)
        {
            var ext = extension?.ToLower();
            return ext == ".jpg" || ext == ".jpeg";
        }

        /// <summary>
        /// JPEGファイルからサイズを解析
        /// </summary>
        public (int width, int height)? ParseImageSize(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                
                var buffer = new byte[4];
                
                // JPEG signature check (FF D8)
                if (stream.Read(buffer, 0, 2) != 2) return null;
                if (buffer[0] != 0xFF || buffer[1] != 0xD8) return null;
                
                // Search for SOF0 (Start of Frame) or SOF2 marker
                while (stream.Read(buffer, 0, 2) == 2)
                {
                    if (buffer[0] != 0xFF) continue;
                    
                    if (buffer[1] == 0xC0 || buffer[1] == 0xC2) // SOF0 or SOF2
                    {
                        if (stream.Read(buffer, 0, 2) != 2) return null; // Skip length
                        if (stream.Read(buffer, 0, 1) != 1) return null; // Skip precision
                        
                        if (stream.Read(buffer, 0, 4) != 4) return null;
                        int height = (buffer[0] << 8) | buffer[1];
                        int width = (buffer[2] << 8) | buffer[3];
                        
                        return (width, height);
                    }
                    else
                    {
                        // Skip this segment
                        if (stream.Read(buffer, 0, 2) != 2) return null;
                        int segmentLength = (buffer[0] << 8) | buffer[1];
                        if (segmentLength < 2) return null;
                        stream.Seek(segmentLength - 2, SeekOrigin.Current);
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}