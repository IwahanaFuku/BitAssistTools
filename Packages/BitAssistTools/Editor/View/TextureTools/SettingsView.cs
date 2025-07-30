#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BitAssistTools.Application.ViewModels.TextureTools;
using BitAssistTools.Domain.Models.TextureTools;

namespace BitAssistTools.Editor.View.TextureTools
{
    /// <summary>
    /// テクスチャツール設定View
    /// </summary>
    public static class SettingsView
    {
        private static readonly int[] PreviewSizeOptions = { 128, 256, 512, 1024 };
        private static readonly string[] PreviewSizeLabels = { "128", "256", "512", "1024" };

        /// <summary>
        /// 設定パネルを描画
        /// </summary>
        public static void DrawSettingsView(TextureToolsMainViewModel viewModel)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                // ヘッダー
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("設定", EditorStyles.boldLabel);
                }

                EditorGUILayout.Space(10);

                // ライブプレビュー設定
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("ライブプレビュー設定", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);

                    DrawPreviewSizeSettings(viewModel);
                }

                EditorGUILayout.Space(10);

                // 出力設定
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("出力設定", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);

                    DrawOutputSizeReferenceSettings(viewModel);
                }

                EditorGUILayout.Space(10);

                // ファイル名接尾辞設定
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("ファイル名接尾辞設定", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);

                    DrawFileSuffixSettings(viewModel);
                }

                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawPreviewSizeSettings(TextureToolsMainViewModel viewModel)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("プレビューサイズ:", GUILayout.Width(120));

                int currentSize = viewModel.DetailFunction.PreviewSize;
                int selectedIndex = System.Array.IndexOf(PreviewSizeOptions, currentSize);
                if (selectedIndex == -1) selectedIndex = 0; // デフォルトは128（インデックス0）

                int newIndex = EditorGUILayout.Popup(selectedIndex, PreviewSizeLabels, GUILayout.Width(100));

                if (newIndex != selectedIndex)
                {
                    int newPreviewSize = PreviewSizeOptions[newIndex];
                    viewModel.DetailFunction.PreviewSize = newPreviewSize;
                    viewModel.Preview.UpdateSettings(
                        viewModel.Preview.Settings.Mode,
                        viewModel.Preview.Settings.ShowBeforeAfter,
                        newPreviewSize
                    );
                    viewModel.DetailFunction.NotifySettingsChanged();
                }

                EditorGUILayout.LabelField("pixel", GUILayout.Width(50));
                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawOutputSizeReferenceSettings(TextureToolsMainViewModel viewModel)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("出力サイズ参照:", GUILayout.Width(120));

                EditorGUI.BeginChangeCheck();
                var newReference = (OutputSizeReference)EditorGUILayout.EnumPopup(viewModel.DetailFunction.OutputSizeReference, GUILayout.Width(100));
                if (EditorGUI.EndChangeCheck())
                {
                    viewModel.DetailFunction.OutputSizeReference = newReference;
                    viewModel.DetailFunction.NotifySettingsChanged();
                }

                GUILayout.FlexibleSpace();
            }

            // 説明テキスト
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("", GUILayout.Width(120)); // インデント用
                string reference = viewModel.DetailFunction.OutputSizeReference == OutputSizeReference.Raw
                    ? "元画像サイズ"
                    : "Unityリサイズ後サイズ";
                string description = $"PNG変換・チャンネル反転・法線マップ編集・マスク統合の「参照サイズ」で、{reference}を適用";
                EditorGUILayout.LabelField(description, EditorStyles.miniLabel);
            }
        }

        private static void DrawFileSuffixSettings(TextureToolsMainViewModel viewModel)
        {
            var suffixSettings = viewModel.DetailFunction.FileSuffixSettings;
            
            // PNG変換
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("PNG変換:", GUILayout.Width(100));
                EditorGUI.BeginChangeCheck();
                var newPngSuffix = EditorGUILayout.TextField(suffixSettings.PngConvertSuffix ?? string.Empty);
                if (EditorGUI.EndChangeCheck())
                {
                    suffixSettings.PngConvertSuffix = newPngSuffix ?? string.Empty;
                    viewModel.DetailFunction.NotifySettingsChanged();
                }
            }

            // チャンネル反転
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("チャンネル反転:", GUILayout.Width(100));
                EditorGUI.BeginChangeCheck();
                var newChannelSuffix = EditorGUILayout.TextField(suffixSettings.ChannelInvertSuffix ?? string.Empty);
                if (EditorGUI.EndChangeCheck())
                {
                    suffixSettings.ChannelInvertSuffix = newChannelSuffix ?? string.Empty;
                    viewModel.DetailFunction.NotifySettingsChanged();
                }
            }

            // 法線マップ編集
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("法線マップ編集:", GUILayout.Width(100));
                EditorGUI.BeginChangeCheck();
                var newNormalSuffix = EditorGUILayout.TextField(suffixSettings.NormalMapSuffix ?? string.Empty);
                if (EditorGUI.EndChangeCheck())
                {
                    suffixSettings.NormalMapSuffix = newNormalSuffix ?? string.Empty;
                    viewModel.DetailFunction.NotifySettingsChanged();
                }
            }

            // マスク統合
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("マスク統合:", GUILayout.Width(100));
                EditorGUI.BeginChangeCheck();
                var newMaskSuffix = EditorGUILayout.TextField(suffixSettings.MaskMergeSuffix ?? string.Empty);
                if (EditorGUI.EndChangeCheck())
                {
                    suffixSettings.MaskMergeSuffix = newMaskSuffix ?? string.Empty;
                    viewModel.DetailFunction.NotifySettingsChanged();
                }
            }

            EditorGUILayout.Space(5);
            
            // 説明テキスト
            EditorGUILayout.LabelField("ファイル名形式: 元ファイル名_接尾辞.png", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("接尾辞が空白の場合: 元ファイル名.png", EditorStyles.miniLabel);

            EditorGUILayout.Space(5);

            // デフォルトに戻すボタン
            if (GUILayout.Button("デフォルトに戻す", GUILayout.Height(25)))
            {
                suffixSettings.ResetToDefaults();
                viewModel.DetailFunction.NotifySettingsChanged();
            }
        }
    }
}
#endif