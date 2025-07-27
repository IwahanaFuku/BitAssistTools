using System;
using System.Collections.Generic;
using UnityEngine;
using BitAssistTools.Domain.Models.Selection;
using BitAssistTools.Application.Services;

namespace BitAssistTools.Editor.Selection
{
    /// <summary>
    /// Selection機能のViewModel
    /// UI状態とビジネスロジックを分離
    /// </summary>
    public class SelectionViewModel
    {
        // UI展開状態管理
        public class UIExpansionState
        {
            public bool HierarchicalSelection { get; set; } = true;
        }

        // 階層選択設定
        public class HierarchicalSelectionSettings
        {
            public int ParentDepth { get; set; } = 1;
            public bool OnlyOrder { get; set; } = true;
            public bool SelectChildren { get; set; } = false;
        }

        // 選択履歴設定
        public class SelectionHistorySettings
        {
            public int MaxHistoryCount { get; set; } = 20;
        }

        // イベント定義
        public event Action OnUIStateChanged;
        public event Action<string> OnError;
        public event Action OnHistoryChanged;

        // プロパティ
        public UIExpansionState UI { get; private set; }
        public HierarchicalSelectionSettings HierarchicalSelection { get; private set; }
        public SelectionHistorySettings HistorySettings { get; private set; }

        // 選択履歴（DDD Domain Model）
        private SelectionHistory _selectionHistory;
        public SelectionHistory SelectionHistory
        {
            get => _selectionHistory;
            private set
            {
                _selectionHistory = value;
                OnHistoryChanged?.Invoke();
            }
        }

        // その他のUI状態
        public Vector2 HistoryScrollPosition { get; set; }
        public List<bool> HistoryItemCheckStates { get; private set; }
        public List<string> HistoryItemNames { get; private set; }

        public SelectionViewModel()
        {
            UI = new UIExpansionState();
            HierarchicalSelection = new HierarchicalSelectionSettings();
            HistorySettings = new SelectionHistorySettings();
            
            // 初期化
            _selectionHistory = new SelectionHistory(HistorySettings.MaxHistoryCount);
            HistoryItemCheckStates = new List<bool>();
            HistoryItemNames = new List<string>();
            
            // 履歴変更イベントの購読
            OnHistoryChanged += UpdateHistoryUI;
        }

        /// <summary>
        /// 現在の選択を履歴に追加
        /// </summary>
        public void ExecuteAddSelection()
        {
            try
            {
                SelectionHistory = SelectionHistoryService.AddCurrentSelection(
                    SelectionHistory, 
                    HistorySettings.MaxHistoryCount
                );
            }
            catch (Exception ex)
            {
                NotifyError($"選択追加中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 選択履歴から選択状態を復元
        /// </summary>
        public void ExecuteLoadSelection()
        {
            try
            {
                SelectionHistoryService.LoadSelection(SelectionHistory);
            }
            catch (Exception ex)
            {
                NotifyError($"選択読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 選択された履歴項目を削除
        /// </summary>
        public void ExecuteDeleteSelection()
        {
            try
            {
                SelectionHistory = SelectionHistoryService.DeleteSelectedItems(
                    SelectionHistory, 
                    HistoryItemCheckStates
                );
            }
            catch (Exception ex)
            {
                NotifyError($"選択削除中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 全選択項目のチェック状態を切り替え
        /// </summary>
        public void ExecuteToggleAllSelection()
        {
            try
            {
                SelectionHistory = SelectionHistoryService.ToggleAllSelections(SelectionHistory);
            }
            catch (Exception ex)
            {
                NotifyError($"全選択切り替え中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 階層選択を実行
        /// </summary>
        public void ExecuteHierarchicalSelection()
        {
            try
            {
                var rule = new HierarchicalSelectionRule(
                    HierarchicalSelection.ParentDepth,
                    HierarchicalSelection.OnlyOrder,
                    HierarchicalSelection.SelectChildren
                );

                HierarchicalSelectionService.ExecuteHierarchicalSelection(rule);
            }
            catch (Exception ex)
            {
                NotifyError($"階層選択中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 指定したインデックスの履歴項目のチェック状態を切り替え
        /// </summary>
        public void ToggleHistoryItemCheck(int index)
        {
            try
            {
                if (index >= 0 && index < HistoryItemCheckStates.Count)
                {
                    HistoryItemCheckStates[index] = !HistoryItemCheckStates[index];
                    
                    // ドメインモデルも更新
                    SelectionHistory = SelectionHistoryService.ToggleSelectionActive(SelectionHistory, index);
                }
            }
            catch (Exception ex)
            {
                NotifyError($"チェック状態切り替え中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 指定したインデックスの履歴項目の名前を更新
        /// </summary>
        public void UpdateHistoryItemName(int index, string newName)
        {
            try
            {
                if (index >= 0 && index < HistoryItemNames.Count)
                {
                    HistoryItemNames[index] = newName;
                    
                    // ドメインモデルも更新
                    SelectionHistory = SelectionHistoryService.UpdateSelectionName(SelectionHistory, index, newName);
                }
            }
            catch (Exception ex)
            {
                NotifyError($"名前更新中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 階層選択が実行可能かどうかを判定
        /// </summary>
        public bool CanExecuteHierarchicalSelection()
        {
            var rule = new HierarchicalSelectionRule(
                HierarchicalSelection.ParentDepth,
                HierarchicalSelection.OnlyOrder,
                HierarchicalSelection.SelectChildren
            );

            return HierarchicalSelectionService.CanExecuteHierarchicalSelection(rule);
        }

        /// <summary>
        /// 無効なオブジェクト参照をクリーンアップ
        /// </summary>
        public void CleanupInvalidReferences()
        {
            try
            {
                SelectionHistory = SelectionHistoryService.CleanupInvalidReferences(SelectionHistory);
            }
            catch (Exception ex)
            {
                NotifyError($"クリーンアップ中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 履歴UI状態を更新
        /// </summary>
        private void UpdateHistoryUI()
        {
            // チェック状態リストを更新
            HistoryItemCheckStates.Clear();
            HistoryItemNames.Clear();

            foreach (var selection in SelectionHistory.Selections)
            {
                HistoryItemCheckStates.Add(selection.IsActive);
                HistoryItemNames.Add(selection.Name);
            }

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
        /// 統計情報を取得
        /// </summary>
        public SelectionHistoryStats GetHistoryStats()
        {
            return SelectionHistory.GetStats();
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            OnHistoryChanged -= UpdateHistoryUI;
        }
    }
}