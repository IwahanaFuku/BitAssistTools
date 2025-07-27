using UnityEngine;
using UnityEditor;

namespace BitAssistTools.Editor.Utilities
{
    /// <summary>
    /// ナビゲーション関連の汎用ユーティリティ
    /// </summary>
    public static class NavigationUtilities
    {
        private const string HELP_URL = "https://scrapbox.io/BitAssistTools/Bit_Assist_Tools%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6";

        /// <summary>
        /// ヘルプページを開く
        /// </summary>
        [MenuItem("Bit Assist Tools/ヘルプ", priority = 210)]
        public static void OpenHelpPage()
        {
            UnityEngine.Application.OpenURL(HELP_URL);
        }

        /// <summary>
        /// 指定したURLを開く
        /// </summary>
        public static void OpenURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("URLが指定されていません。");
                return;
            }

            try
            {
                UnityEngine.Application.OpenURL(url);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"URL を開けませんでした: {url} - {ex.Message}");
            }
        }

        /// <summary>
        /// リリースノートページを開く
        /// </summary>
        public static void OpenReleaseNotes()
        {
            const string RELEASE_NOTE_URL = "https://scrapbox.io/BitAssistTools/%E3%83%AA%E3%83%AA%E3%83%BC%E3%82%B9%E3%83%8E%E3%83%BC%E3%83%88";
            OpenURL(RELEASE_NOTE_URL);
        }

        /// <summary>
        /// Unity Consoleを開く
        /// </summary>
        public static void OpenConsole()
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.ConsoleWindow,UnityEditor"));
        }

        /// <summary>
        /// Project Settings を開く
        /// </summary>
        public static void OpenProjectSettings()
        {
            SettingsService.OpenProjectSettings();
        }

        /// <summary>
        /// Package Manager を開く
        /// </summary>
        public static void OpenPackageManager()
        {
            UnityEditor.PackageManager.UI.Window.Open("");
        }
    }
}