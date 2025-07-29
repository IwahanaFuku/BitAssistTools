using System.Collections.Generic;
using System.IO;
using System.Linq;
using BitAssistTools.Domain.Services.TextureTools;

namespace BitAssistTools.Infrastructure.Unity.ImageParsers
{
    /// <summary>
    /// 画像サイズ解析器のファクトリークラス
    /// </summary>
    public static class ImageSizeParserFactory
    {
        /// <summary>
        /// 登録された解析器のリスト
        /// </summary>
        private static readonly List<IImageSizeParser> _parsers = new List<IImageSizeParser>
        {
            new PngImageSizeParser(),
            new JpegImageSizeParser(),
            new TgaImageSizeParser(),
            new BmpImageSizeParser()
        };

        /// <summary>
        /// 指定された拡張子に対応する解析器を取得
        /// </summary>
        /// <param name="extension">拡張子（.を含む）</param>
        /// <returns>対応する解析器、見つからない場合はnull</returns>
        public static IImageSizeParser GetParser(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return null;
            
            var normalizedExtension = extension.ToLower();
            return _parsers.FirstOrDefault(parser => parser.SupportsExtension(normalizedExtension));
        }

        /// <summary>
        /// ファイルパスから画像サイズを解析
        /// </summary>
        /// <param name="filePath">画像ファイルのパス</param>
        /// <returns>解析に成功した場合は幅と高さ、失敗した場合はnull</returns>
        public static (int width, int height)? ParseImageSize(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return null;

            var extension = System.IO.Path.GetExtension(filePath);
            var parser = GetParser(extension);
            
            if (parser == null) return null;

            try
            {
                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    return parser.ParseImageSize(fileStream);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ストリームから画像サイズを解析（拡張子指定）
        /// </summary>
        /// <param name="stream">画像データのストリーム</param>
        /// <param name="extension">拡張子（.を含む）</param>
        /// <returns>解析に成功した場合は幅と高さ、失敗した場合はnull</returns>
        public static (int width, int height)? ParseImageSize(Stream stream, string extension)
        {
            var parser = GetParser(extension);
            return parser?.ParseImageSize(stream);
        }

        /// <summary>
        /// 新しい解析器を登録
        /// </summary>
        /// <param name="parser">追加する解析器</param>
        public static void RegisterParser(IImageSizeParser parser)
        {
            if (parser != null && !_parsers.Contains(parser))
            {
                _parsers.Add(parser);
            }
        }

        /// <summary>
        /// サポートされている拡張子一覧を取得
        /// </summary>
        /// <returns>サポートされている拡張子の配列</returns>
        public static string[] GetSupportedExtensions()
        {
            return _parsers.SelectMany(parser => parser.SupportedExtensions).Distinct().ToArray();
        }
    }
}