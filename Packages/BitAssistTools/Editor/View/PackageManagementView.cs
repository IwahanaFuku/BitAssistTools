using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;

namespace BitAssistTools.Editor.View
{
    /// <summary>
    /// パッケージ管理ウィンドウ（MVVM対応）
    /// </summary>
    public class PackageManagementView : EditorWindow
    {
        private PackageManagementViewModel _viewModel;
        private const string RELEASE_NOTE_URL = "https://scrapbox.io/BitAssistTools/%E3%83%AA%E3%83%AA%E3%83%BC%E3%82%B9%E3%83%8E%E3%83%BC%E3%83%88";

        [MenuItem("Bit Assist Tools/バージョンアップデート", priority = 200)]
        private static void ShowWindow()
        {
            var window = GetWindow<PackageManagementView>("Bit Assist Tools Update");
            Vector2 windowSize = new Vector2(350, 130);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.Show();
        }

        public static void ShowWindowIfUpdateAvailable()
        {
            var tempViewModel = new PackageManagementViewModel();
            if (tempViewModel.HasNewerVersion)
            {
                ShowWindow();
            }
            tempViewModel.Dispose();
        }

        private void OnEnable()
        {
            _viewModel = new PackageManagementViewModel();
            _viewModel.OnUIStateChanged += Repaint;
            _viewModel.OnError += ShowError;
        }

        private void OnDisable()
        {
            if (_viewModel != null)
            {
                _viewModel.OnUIStateChanged -= Repaint;
                _viewModel.OnError -= ShowError;
                _viewModel.Dispose();
                _viewModel = null;
            }
        }

        private void OnGUI()
        {
            if (_viewModel == null) return;

            var hyperlinkStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(32f / 255f, 157f / 255f, 240f / 255f) },
                stretchWidth = true,
                wordWrap = true
            };

            GUILayout.Space(10);

            if (_viewModel.HasNewerVersion)
            {
                DrawUpdateAvailableUI(hyperlinkStyle);
            }
            else
            {
                DrawUpToDateUI(hyperlinkStyle);
            }
        }

        private void DrawUpdateAvailableUI(GUIStyle hyperlinkStyle)
        {
            GUILayout.Label($"{_viewModel.DisplayName} の最新バージョン【{_viewModel.LatestVersion}】がリリースされています。 \nアップデートしますか？");
            GUILayout.Label($"現在のバージョン：{_viewModel.CurrentVersion} ");
            
            DrawReleaseNoteLink(hyperlinkStyle);

            GUILayout.FlexibleSpace();
            
            DrawSettingsUI();
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("いいえ"))
            {
                Close();
            }

            if (GUILayout.Button("はい"))
            {
                _viewModel.UpdatePackage();
                Close();
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawUpToDateUI(GUIStyle hyperlinkStyle)
        {
            GUILayout.Label($"ご使用の{_viewModel.DisplayName} は最新です");
            GUILayout.Label($"現在のバージョン：{_viewModel.CurrentVersion} ");
            
            DrawReleaseNoteLink(hyperlinkStyle);
            
            GUILayout.FlexibleSpace();
            
            DrawSettingsUI();
            
            if (GUILayout.Button("閉じる"))
            {
                Close();
            }
        }

        private void DrawSettingsUI()
        {
            bool newValue = EditorGUILayout.Toggle(
                "自動アップデート通知", 
                _viewModel.IsAutoUpdateNotificationEnabled
            );
            
            if (newValue != _viewModel.IsAutoUpdateNotificationEnabled)
            {
                _viewModel.IsAutoUpdateNotificationEnabled = newValue;
            }
        }

        private void DrawReleaseNoteLink(GUIStyle hyperlinkStyle)
        {
            GUILayout.Label("リリースノートを確認", hyperlinkStyle);
            if (Event.current.type == EventType.MouseDown && 
                GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                UnityEngine.Application.OpenURL(RELEASE_NOTE_URL);
            }
        }

        private void ShowError(string message)
        {
            EditorUtility.DisplayDialog("エラー", message, "OK");
        }
    }
}