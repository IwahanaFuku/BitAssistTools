using System;
using UnityEngine;
using BitAssistTools.Domain.Models.Renaming;
using BitAssistTools.Application.Services;

namespace BitAssistTools.Application.ViewModels
{
    /// <summary>
    /// Renaming機能のViewModel
    /// UI状態とビジネスロジックを分離
    /// </summary>
    public class RenamingViewModel
    {
        // UI展開状態管理
        public class UIExpansionState
        {
            public bool PrefixSuffix { get; set; } = true;
            public bool StringReplace { get; set; } = true;
            public bool TextureRename { get; set; } = true;
            public bool ObjectNumber { get; set; } = true;
            public bool PrefabAsset { get; set; } = true;
        }

        // 接頭辞・接尾辞設定
        public class PrefixSuffixSettings
        {
            public string Prefix { get; set; } = "Prefix";
            public string Suffix { get; set; } = "Suffix";
            public string Delimiter { get; set; } = "_";
        }

        // 文字列置換設定
        public class StringReplaceSettings
        {
            public string SearchString { get; set; } = "";
            public string ReplaceString { get; set; } = "";
        }

        // イベント定義
        public event Action OnUIStateChanged;
        public event Action<string> OnError;
        public event Action<string> OnSuccess;

        // プロパティ
        public UIExpansionState UI { get; private set; }
        public PrefixSuffixSettings PrefixSuffix { get; private set; }
        public StringReplaceSettings StringReplace { get; private set; }

        public RenamingViewModel()
        {
            UI = new UIExpansionState();
            PrefixSuffix = new PrefixSuffixSettings();
            StringReplace = new StringReplaceSettings();
        }

        /// <summary>
        /// 接頭辞・接尾辞追加を実行
        /// </summary>
        public void ExecutePrefixSuffixOperation()
        {
            try
            {
                var rule = new PrefixSuffixRule(
                    PrefixSuffix.Prefix,
                    PrefixSuffix.Suffix,
                    PrefixSuffix.Delimiter
                );

                if (!rule.IsValid())
                {
                    NotifyError("接頭辞または接尾辞を入力してください。");
                    return;
                }

                if (!RenamingService.CanExecuteOperation(RenameOperationType.PrefixSuffix))
                {
                    NotifyError("リネームするGameObjectを選択してください。");
                    return;
                }

                RenamingService.AddPrefixAndSuffix(rule);
                NotifySuccess($"接頭辞・接尾辞を追加しました: {rule}");
            }
            catch (Exception ex)
            {
                NotifyError($"接頭辞・接尾辞追加中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 文字列置換を実行
        /// </summary>
        public void ExecuteStringReplaceOperation()
        {
            try
            {
                var rule = new StringReplaceRule(
                    StringReplace.SearchString,
                    StringReplace.ReplaceString
                );

                if (!rule.IsValid())
                {
                    NotifyError("検索文字列を入力してください。");
                    return;
                }

                if (!RenamingService.CanExecuteOperation(RenameOperationType.StringReplace))
                {
                    NotifyError("リネームするGameObjectを選択してください。");
                    return;
                }

                RenamingService.ReplaceString(rule);
                NotifySuccess($"文字列を置換しました: {rule}");
            }
            catch (Exception ex)
            {
                NotifyError($"文字列置換中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// テクスチャ名でリネームを実行
        /// </summary>
        public void ExecuteTextureNameOperation()
        {
            try
            {
                if (!RenamingService.CanExecuteOperation(RenameOperationType.TextureName))
                {
                    NotifyError("テクスチャを持つMaterialを選択してください。");
                    return;
                }

                RenamingService.RenameToTextureName();
                NotifySuccess("マテリアルをテクスチャ名でリネームしました。");
            }
            catch (Exception ex)
            {
                NotifyError($"テクスチャ名リネーム中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// マテリアル名でリネームを実行
        /// </summary>
        public void ExecuteMaterialNameOperation()
        {
            try
            {
                if (!RenamingService.CanExecuteOperation(RenameOperationType.MaterialName))
                {
                    NotifyError("Rendererコンポーネントを持つGameObjectを選択してください。");
                    return;
                }

                RenamingService.RenameToMaterialName();
                NotifySuccess("オブジェクトをマテリアル名でリネームしました。");
            }
            catch (Exception ex)
            {
                NotifyError($"マテリアル名リネーム中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 番号付与を実行
        /// </summary>
        public void ExecuteAddNumberOperation()
        {
            try
            {
                if (!RenamingService.CanExecuteOperation(RenameOperationType.AddNumber))
                {
                    NotifyError("番号を付与するGameObjectを選択してください。");
                    return;
                }

                RenamingService.AddObjectNumber();
                NotifySuccess("オブジェクトに番号を付与しました。");
            }
            catch (Exception ex)
            {
                NotifyError($"番号付与中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 番号除去を実行
        /// </summary>
        public void ExecuteRemoveNumberOperation()
        {
            try
            {
                if (!RenamingService.CanExecuteOperation(RenameOperationType.RemoveNumber))
                {
                    NotifyError("番号を除去するGameObjectを選択してください。");
                    return;
                }

                RenamingService.RemoveObjectNumber();
                NotifySuccess("オブジェクトから番号を除去しました。");
            }
            catch (Exception ex)
            {
                NotifyError($"番号除去中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// Prefabアセットリネームを実行
        /// </summary>
        public void ExecutePrefabAssetOperation()
        {
            try
            {
                if (!RenamingService.CanExecuteOperation(RenameOperationType.PrefabAsset))
                {
                    NotifyError("Prefabインスタンスを選択してください。");
                    return;
                }

                RenamingService.RenameSelectedPrefabAsset();
                NotifySuccess("Prefabアセットをリネームしました。");
            }
            catch (Exception ex)
            {
                NotifyError($"Prefabアセットリネーム中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 指定した操作が実行可能かどうかを判定
        /// </summary>
        public bool CanExecuteOperation(RenameOperationType operationType)
        {
            return RenamingService.CanExecuteOperation(operationType);
        }

        /// <summary>
        /// 現在の選択状況の統計を取得
        /// </summary>
        public RenamingService.RenamingStats GetCurrentStats()
        {
            return RenamingService.GetRenamingStats();
        }

        /// <summary>
        /// 接頭辞を更新
        /// </summary>
        public void UpdatePrefix(string prefix)
        {
            PrefixSuffix.Prefix = prefix;
            NotifyUIStateChanged();
        }

        /// <summary>
        /// 接尾辞を更新
        /// </summary>
        public void UpdateSuffix(string suffix)
        {
            PrefixSuffix.Suffix = suffix;
            NotifyUIStateChanged();
        }

        /// <summary>
        /// 区切り文字を更新
        /// </summary>
        public void UpdateDelimiter(string delimiter)
        {
            PrefixSuffix.Delimiter = delimiter;
            NotifyUIStateChanged();
        }

        /// <summary>
        /// 検索文字列を更新
        /// </summary>
        public void UpdateSearchString(string searchString)
        {
            StringReplace.SearchString = searchString;
            NotifyUIStateChanged();
        }

        /// <summary>
        /// 置換文字列を更新
        /// </summary>
        public void UpdateReplaceString(string replaceString)
        {
            StringReplace.ReplaceString = replaceString;
            NotifyUIStateChanged();
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