using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;

namespace BitAssistTools.Editor.AssetNavigation
{
    /// <summary>
    /// AssetNavigation機能のView
    /// ViewModelを使用してUI状態とビジネスロジックを分離
    /// </summary>
    public class AssetNavigationView : EditorWindow
    {
        private AssetNavigationViewModel _viewModel;

        [MenuItem("Bit Assist Tools/ファイルパス選択", priority = 51)]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetNavigationView>("ファイルパス選択");
            window.Initialize();
        }

        private void Initialize()
        {
            _viewModel = new AssetNavigationViewModel();
            
            // イベント購読
            _viewModel.OnUIStateChanged += Repaint;
            _viewModel.OnError += ShowError;
            _viewModel.OnSuccess += ShowSuccess;
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (_viewModel != null)
            {
                _viewModel.OnUIStateChanged -= Repaint;
                _viewModel.OnError -= ShowError;
                _viewModel.OnSuccess -= ShowSuccess;
                _viewModel.Dispose();
            }
        }

        private void OnGUI()
        {
            if (_viewModel == null)
            {
                Initialize();
            }

            DrawPathInputSection();
            EditorGUILayout.Space();
            DrawAssetInfoSection();
            EditorGUILayout.Space();
            DrawActionButtons();
        }

        /// <summary>
        /// パス入力セクションの描画
        /// </summary>
        private void DrawPathInputSection()
        {
            GUILayout.Label("アセットパス:", EditorStyles.boldLabel);
            
            string newPath = EditorGUILayout.TextField(_viewModel.PathInput.FilePath);
            if (newPath != _viewModel.PathInput.FilePath)
            {
                _viewModel.UpdateFilePath(newPath);
            }

            // パス正規化ボタン
            if (GUILayout.Button("パスを正規化"))
            {
                _viewModel.NormalizeFilePath();
            }
        }

        /// <summary>
        /// アセット情報セクションの描画
        /// </summary>
        private void DrawAssetInfoSection()
        {
            string hint = _viewModel.GetPathHint();
            var info = _viewModel.GetCurrentAssetInfo();
            
            MessageType messageType = info.Exists ? MessageType.Info : MessageType.Warning;
            EditorGUILayout.HelpBox(hint, messageType);

            if (info.Exists && info.Asset != null)
            {
                EditorGUILayout.ObjectField("プレビュー", info.Asset, typeof(UnityEngine.Object), false);
            }
        }

        /// <summary>
        /// アクションボタンの描画
        /// </summary>
        private void DrawActionButtons()
        {
            GUILayout.BeginHorizontal();

            // 現在の選択を読み込み
            if (GUILayout.Button("現在の選択を読み込み"))
            {
                _viewModel.LoadCurrentSelectionPath();
            }

            // アセット選択実行
            bool canExecute = _viewModel.CanExecuteSelectAsset();
            EditorGUI.BeginDisabledGroup(!canExecute);
            
            if (GUILayout.Button("選択"))
            {
                _viewModel.ExecuteSelectAsset();
            }
            
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            if (!canExecute)
            {
                EditorGUILayout.HelpBox(
                    "有効なアセットパスを入力してください。",
                    MessageType.Info
                );
            }
        }

        /// <summary>
        /// エラー表示
        /// </summary>
        private void ShowError(string message)
        {
            EditorUtility.DisplayDialog("エラー", message, "OK");
        }

        /// <summary>
        /// 成功表示
        /// </summary>
        private void ShowSuccess(string message)
        {
            Debug.Log(message);
            // 必要に応じてステータスバーや通知で表示可能
        }
    }
}