namespace BitAssistTools.Domain.Models.TextureTools
{
    /// <summary>
    /// 出力ファイル名の接尾辞設定
    /// </summary>
    public class FileSuffixSettings
    {
        /// <summary>
        /// PNG変換の接尾辞
        /// </summary>
        public string PngConvertSuffix { get; set; } = "png";

        /// <summary>
        /// チャンネル反転の接尾辞
        /// </summary>
        public string ChannelInvertSuffix { get; set; } = "channel_inverted";

        /// <summary>
        /// 法線マップ編集の接尾辞
        /// </summary>
        public string NormalMapSuffix { get; set; } = "normal_adjusted";

        /// <summary>
        /// マスク統合の接尾辞
        /// </summary>
        public string MaskMergeSuffix { get; set; } = "mask_merged";

        /// <summary>
        /// デフォルト値にリセット
        /// </summary>
        public void ResetToDefaults()
        {
            PngConvertSuffix = "png";
            ChannelInvertSuffix = "channel_inverted";
            NormalMapSuffix = "normal_adjusted";
            MaskMergeSuffix = "mask_merged";
        }

        /// <summary>
        /// 指定された詳細機能に対応する接尾辞を取得
        /// </summary>
        public string GetSuffix(DetailFunction function)
        {
            switch (function)
            {
                case DetailFunction.PngConvert:
                    return PngConvertSuffix;
                case DetailFunction.ChannelInvert:
                    return ChannelInvertSuffix;
                case DetailFunction.NormalMap:
                    return NormalMapSuffix;
                case DetailFunction.MaskMerge:
                    return MaskMergeSuffix;
                default:
                    return "processed";
            }
        }

        /// <summary>
        /// 指定された詳細機能に対応する接尾辞を設定
        /// </summary>
        public void SetSuffix(DetailFunction function, string suffix)
        {
            // nullは空文字列に変換、空白文字列は許可
            if (suffix == null) suffix = string.Empty;

            switch (function)
            {
                case DetailFunction.PngConvert:
                    PngConvertSuffix = suffix;
                    break;
                case DetailFunction.ChannelInvert:
                    ChannelInvertSuffix = suffix;
                    break;
                case DetailFunction.NormalMap:
                    NormalMapSuffix = suffix;
                    break;
                case DetailFunction.MaskMerge:
                    MaskMergeSuffix = suffix;
                    break;
            }
        }
    }
}