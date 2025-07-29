using System;
using UnityEngine;
using BitAssistTools.Domain.Models.AssetNavigation;
using BitAssistTools.Application.Services;

namespace BitAssistTools.Application.ViewModels
{
    /// <summary>
    /// AssetNavigation機能のViewModel
    /// UI状態とビジネスロジックを分離
    /// </summary>
    public class AssetNavigationViewModel
    {
        // パス入力設定
        public class PathInputSettings
        {
            public string FilePath { get; set; } = "Assets/";
        }

        // イベント定義
        public event Action OnUIStateChanged;
        public event Action<string> OnError;
        public event Action<string> OnSuccess;

        // プロパティ
        public PathInputSettings PathInput { get; private set; }

        public AssetNavigationViewModel()
        {
            PathInput = new PathInputSettings();
        }

        /// <summary>
        /// アセット選択を実行
        /// </summary>
        public void ExecuteSelectAsset()
        {
            try
            {
                var assetPath = new AssetPath(PathInput.FilePath);
                
                if (!assetPath.IsValid())
                {
                    NotifyError("有効なアセットパス（Assets/で始まる）を入力してください。");
                    return;
                }

                if (!AssetNavigationService.CanSelectAsset(assetPath))
                {
                    NotifyError($"指定されたパスにアセットが見つかりません: {assetPath}");
                    return;
                }

                AssetNavigationService.SelectAsset(assetPath);
                NotifySuccess($"アセットを選択しました: {assetPath}");
            }
            catch (Exception ex)
            {
                NotifyError($"アセット選択中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 現在選択されているアセットのパスを取得
        /// </summary>
        public void LoadCurrentSelectionPath()
        {
            try
            {
                var selectedPath = AssetNavigationService.GetSelectedAssetPath();
                UpdateFilePath(selectedPath);
                NotifySuccess($"現在の選択パスを読み込みました: {selectedPath}");
            }
            catch (Exception ex)
            {
                NotifyError($"選択パス読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// アセット選択が実行可能かどうかを判定
        /// </summary>
        public bool CanExecuteSelectAsset()
        {
            var assetPath = new AssetPath(PathInput.FilePath);
            return AssetNavigationService.CanSelectAsset(assetPath);
        }

        /// <summary>
        /// 現在のパスのアセット情報を取得
        /// </summary>
        public AssetNavigationService.AssetInfo GetCurrentAssetInfo()
        {
            var assetPath = new AssetPath(PathInput.FilePath);
            return AssetNavigationService.GetAssetInfo(assetPath);
        }

        /// <summary>
        /// ファイルパスを更新
        /// </summary>
        public void UpdateFilePath(string filePath)
        {
            PathInput.FilePath = filePath;
            NotifyUIStateChanged();
        }

        /// <summary>
        /// パスを正規化
        /// </summary>
        public void NormalizeFilePath()
        {
            var assetPath = new AssetPath(PathInput.FilePath);
            var normalizedPath = assetPath.Normalize();
            UpdateFilePath(normalizedPath);
        }

        /// <summary>
        /// パス入力のヒントテキストを取得
        /// </summary>
        public string GetPathHint()
        {
            var assetPath = new AssetPath(PathInput.FilePath);
            if (!assetPath.IsValid())
                return "パスは 'Assets/' で始まる必要があります";

            var info = GetCurrentAssetInfo();
            return info.Exists ? $"有効なアセット ({info.TypeOrError})" : info.TypeOrError;
        }

        /// <summary>
        /// UI状態変更通知
        /// </summary>
        public void NotifyUIStateChanged()
        {
            OnUIStateChanged?.Invoke();
        }

        /// <summary>
        /// エラー通知
        /// </summary>
        private void NotifyError(string message)
        {
            OnError?.Invoke(message);
        }

        /// <summary>
        /// 成功通知
        /// </summary>
        private void NotifySuccess(string message)
        {
            OnSuccess?.Invoke(message);
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            // 現在は特別なクリーンアップは不要
        }
    }
}