#if UNITY_EDITOR
using UnityEditor;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// テクスチャツールの設定管理
    /// EditorPrefsを使用してUnityプロジェクト間で設定を共有
    /// </summary>
    public static class TextureToolsSettingsManager
    {
        #region Keys
        private const string PREVIEW_SIZE_KEY = "BitAssistTools.TextureTools.PreviewSize";
        private const string OUTPUT_WIDTH_KEY = "BitAssistTools.TextureTools.OutputWidth";
        private const string OUTPUT_HEIGHT_KEY = "BitAssistTools.TextureTools.OutputHeight";
        private const string OUTPUT_FILENAME_KEY = "BitAssistTools.TextureTools.OutputFileName";
        private const string OUTPUT_SIZE_REFERENCE_KEY = "BitAssistTools.TextureTools.OutputSizeReference";
        private const string PNG_CONVERT_SUFFIX_KEY = "BitAssistTools.TextureTools.PngConvertSuffix";
        private const string CHANNEL_INVERT_SUFFIX_KEY = "BitAssistTools.TextureTools.ChannelInvertSuffix";
        private const string NORMAL_MAP_SUFFIX_KEY = "BitAssistTools.TextureTools.NormalMapSuffix";
        private const string MASK_MERGE_SUFFIX_KEY = "BitAssistTools.TextureTools.MaskMergeSuffix";
        #endregion

        #region Save Methods
        /// <summary>
        /// プレビューサイズを保存
        /// </summary>
        public static void SavePreviewSize(int previewSize)
        {
            EditorPrefs.SetInt(PREVIEW_SIZE_KEY, previewSize);
        }

        /// <summary>
        /// 出力サイズを保存
        /// </summary>
        public static void SaveOutputSize(int width, int height)
        {
            EditorPrefs.SetInt(OUTPUT_WIDTH_KEY, width);
            EditorPrefs.SetInt(OUTPUT_HEIGHT_KEY, height);
        }

        /// <summary>
        /// 出力ファイル名を保存
        /// </summary>
        public static void SaveOutputFileName(string fileName)
        {
            EditorPrefs.SetString(OUTPUT_FILENAME_KEY, fileName);
        }

        /// <summary>
        /// 出力サイズ参照設定を保存
        /// </summary>
        public static void SaveOutputSizeReference(BitAssistTools.Domain.Models.TextureTools.OutputSizeReference reference)
        {
            EditorPrefs.SetInt(OUTPUT_SIZE_REFERENCE_KEY, (int)reference);
        }

        /// <summary>
        /// ファイル接尾辞設定を保存
        /// </summary>
        public static void SaveFileSuffixSettings(BitAssistTools.Domain.Models.TextureTools.FileSuffixSettings settings)
        {
            EditorPrefs.SetString(PNG_CONVERT_SUFFIX_KEY, settings.PngConvertSuffix);
            EditorPrefs.SetString(CHANNEL_INVERT_SUFFIX_KEY, settings.ChannelInvertSuffix);
            EditorPrefs.SetString(NORMAL_MAP_SUFFIX_KEY, settings.NormalMapSuffix);
            EditorPrefs.SetString(MASK_MERGE_SUFFIX_KEY, settings.MaskMergeSuffix);
        }
        #endregion

        #region Load Methods
        /// <summary>
        /// プレビューサイズを読み込み
        /// </summary>
        public static int LoadPreviewSize(int defaultValue = 256)
        {
            return EditorPrefs.GetInt(PREVIEW_SIZE_KEY, defaultValue);
        }

        /// <summary>
        /// 出力幅を読み込み
        /// </summary>
        public static int LoadOutputWidth(int defaultValue = 1024)
        {
            return EditorPrefs.GetInt(OUTPUT_WIDTH_KEY, defaultValue);
        }

        /// <summary>
        /// 出力高さを読み込み
        /// </summary>
        public static int LoadOutputHeight(int defaultValue = 1024)
        {
            return EditorPrefs.GetInt(OUTPUT_HEIGHT_KEY, defaultValue);
        }

        /// <summary>
        /// 出力ファイル名を読み込み
        /// </summary>
        public static string LoadOutputFileName(string defaultValue = "mask_merged")
        {
            return EditorPrefs.GetString(OUTPUT_FILENAME_KEY, defaultValue);
        }

        /// <summary>
        /// 出力サイズ参照設定を読み込み
        /// </summary>
        public static BitAssistTools.Domain.Models.TextureTools.OutputSizeReference LoadOutputSizeReference(
            BitAssistTools.Domain.Models.TextureTools.OutputSizeReference defaultValue = BitAssistTools.Domain.Models.TextureTools.OutputSizeReference.Raw)
        {
            return (BitAssistTools.Domain.Models.TextureTools.OutputSizeReference)EditorPrefs.GetInt(OUTPUT_SIZE_REFERENCE_KEY, (int)defaultValue);
        }

        /// <summary>
        /// ファイル接尾辞設定を読み込み
        /// </summary>
        public static BitAssistTools.Domain.Models.TextureTools.FileSuffixSettings LoadFileSuffixSettings()
        {
            var settings = new BitAssistTools.Domain.Models.TextureTools.FileSuffixSettings();
            
            settings.PngConvertSuffix = EditorPrefs.GetString(PNG_CONVERT_SUFFIX_KEY, settings.PngConvertSuffix);
            settings.ChannelInvertSuffix = EditorPrefs.GetString(CHANNEL_INVERT_SUFFIX_KEY, settings.ChannelInvertSuffix);
            settings.NormalMapSuffix = EditorPrefs.GetString(NORMAL_MAP_SUFFIX_KEY, settings.NormalMapSuffix);
            settings.MaskMergeSuffix = EditorPrefs.GetString(MASK_MERGE_SUFFIX_KEY, settings.MaskMergeSuffix);
            
            return settings;
        }
        #endregion

        #region Reset Methods
        /// <summary>
        /// 全ての設定をリセット
        /// </summary>
        public static void ResetAllSettings()
        {
            EditorPrefs.DeleteKey(PREVIEW_SIZE_KEY);
            EditorPrefs.DeleteKey(OUTPUT_WIDTH_KEY);
            EditorPrefs.DeleteKey(OUTPUT_HEIGHT_KEY);
            EditorPrefs.DeleteKey(OUTPUT_FILENAME_KEY);
            EditorPrefs.DeleteKey(OUTPUT_SIZE_REFERENCE_KEY);
            EditorPrefs.DeleteKey(PNG_CONVERT_SUFFIX_KEY);
            EditorPrefs.DeleteKey(CHANNEL_INVERT_SUFFIX_KEY);
            EditorPrefs.DeleteKey(NORMAL_MAP_SUFFIX_KEY);
            EditorPrefs.DeleteKey(MASK_MERGE_SUFFIX_KEY);
        }

        /// <summary>
        /// 設定が存在するかチェック
        /// </summary>
        public static bool HasSettings()
        {
            return EditorPrefs.HasKey(PREVIEW_SIZE_KEY) ||
                   EditorPrefs.HasKey(OUTPUT_WIDTH_KEY) ||
                   EditorPrefs.HasKey(OUTPUT_HEIGHT_KEY) ||
                   EditorPrefs.HasKey(OUTPUT_FILENAME_KEY) ||
                   EditorPrefs.HasKey(OUTPUT_SIZE_REFERENCE_KEY) ||
                   EditorPrefs.HasKey(PNG_CONVERT_SUFFIX_KEY) ||
                   EditorPrefs.HasKey(CHANNEL_INVERT_SUFFIX_KEY) ||
                   EditorPrefs.HasKey(NORMAL_MAP_SUFFIX_KEY) ||
                   EditorPrefs.HasKey(MASK_MERGE_SUFFIX_KEY);
        }
        #endregion
    }
}
#endif