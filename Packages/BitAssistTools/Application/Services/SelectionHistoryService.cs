using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// 選択履歴管理のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class SelectionHistoryService
    {
        /// <summary>
        /// 現在の選択を履歴に追加（DDDベース、Undo対応）
        /// </summary>
        public static SelectionHistory AddCurrentSelection(SelectionHistory history, int maxHistoryCount)
        {
            try
            {
                Undo.SetCurrentGroupName("Add Selection History");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = Selection.objects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("Select one or more objects.");
                    return history;
                }

                // 新しい履歴インスタンスを作成（元の履歴は不変）
                var newHistory = new SelectionHistory(maxHistoryCount);
                
                // 既存の履歴をコピー
                foreach (var selection in history.Selections)
                {
                    newHistory.AddSelection(selection);
                }

                // 現在の選択を追加
                newHistory.AddCurrentSelection(selectedObjects);

                Undo.CollapseUndoOperations(undoGroup);
                return newHistory;
            }
            catch (Exception ex)
            {
                Debug.LogError($"選択履歴追加中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 選択履歴から選択状態を復元（DDDベース、Undo対応）
        /// </summary>
        public static void LoadSelection(SelectionHistory history)
        {
            try
            {
                Undo.SetCurrentGroupName("Load Selection History");
                int undoGroup = Undo.GetCurrentGroup();

                var allActiveObjects = history.GetAllActiveObjects().ToArray();
                
                if (allActiveObjects.Length == 0)
                {
                    Debug.LogWarning("アクティブな選択履歴がありません。");
                    return;
                }

                Selection.objects = allActiveObjects;

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"選択履歴読み込み中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 選択された履歴項目を削除（DDDベース、Undo対応）
        /// </summary>
        public static SelectionHistory DeleteSelectedItems(SelectionHistory history, List<bool> selectionStates)
        {
            try
            {
                Undo.SetCurrentGroupName("Delete Selection History");
                int undoGroup = Undo.GetCurrentGroup();

                // 新しい履歴インスタンスを作成
                var newHistory = new SelectionHistory(history.MaxHistoryCount);

                // 削除対象でない項目のみをコピー
                for (int i = 0; i < history.Selections.Count && i < selectionStates.Count; i++)
                {
                    if (!selectionStates[i]) // チェックされていない項目は保持
                    {
                        newHistory.AddSelection(history.Selections[i]);
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
                return newHistory;
            }
            catch (Exception ex)
            {
                Debug.LogError($"選択履歴削除中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 全選択項目のチェック状態を切り替え（DDDベース、Undo対応）
        /// </summary>
        public static SelectionHistory ToggleAllSelections(SelectionHistory history)
        {
            try
            {
                Undo.SetCurrentGroupName("Toggle All Selections");
                int undoGroup = Undo.GetCurrentGroup();

                // 全てチェックされているかどうかを判定
                bool allActive = history.Selections.All(s => s.IsActive);
                bool newActiveState = !allActive;

                // 新しい履歴インスタンスを作成
                var newHistory = new SelectionHistory(history.MaxHistoryCount);

                // 全ての選択項目のアクティブ状態を切り替え
                foreach (var selection in history.Selections)
                {
                    var updatedSelection = selection.WithActiveState(newActiveState);
                    newHistory.AddSelection(updatedSelection);
                }

                Undo.CollapseUndoOperations(undoGroup);
                return newHistory;
            }
            catch (Exception ex)
            {
                Debug.LogError($"全選択切り替え中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 指定したインデックスの選択項目のアクティブ状態を切り替え
        /// </summary>
        public static SelectionHistory ToggleSelectionActive(SelectionHistory history, int index)
        {
            try
            {
                if (index < 0 || index >= history.Selections.Count)
                {
                    Debug.LogWarning($"無効なインデックスです: {index}");
                    return history;
                }

                // 新しい履歴インスタンスを作成
                var newHistory = new SelectionHistory(history.MaxHistoryCount);

                // 指定したインデックス以外はそのままコピー
                for (int i = 0; i < history.Selections.Count; i++)
                {
                    var selection = history.Selections[i];
                    if (i == index)
                    {
                        // 指定したインデックスのアクティブ状態を切り替え
                        selection = selection.WithActiveState(!selection.IsActive);
                    }
                    newHistory.AddSelection(selection);
                }

                return newHistory;
            }
            catch (Exception ex)
            {
                Debug.LogError($"選択状態切り替え中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 指定したインデックスの選択項目の名前を更新
        /// </summary>
        public static SelectionHistory UpdateSelectionName(SelectionHistory history, int index, string newName)
        {
            try
            {
                if (index < 0 || index >= history.Selections.Count)
                {
                    Debug.LogWarning($"無効なインデックスです: {index}");
                    return history;
                }

                if (string.IsNullOrWhiteSpace(newName))
                {
                    Debug.LogWarning("名前は空にできません。");
                    return history;
                }

                // 新しい履歴インスタンスを作成
                var newHistory = new SelectionHistory(history.MaxHistoryCount);

                // 指定したインデックス以外はそのままコピー
                for (int i = 0; i < history.Selections.Count; i++)
                {
                    var selection = history.Selections[i];
                    if (i == index)
                    {
                        // 指定したインデックスの名前を更新
                        selection = selection.WithName(newName);
                    }
                    newHistory.AddSelection(selection);
                }

                return newHistory;
            }
            catch (Exception ex)
            {
                Debug.LogError($"選択名更新中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 無効なオブジェクト参照をクリーンアップ
        /// </summary>
        public static SelectionHistory CleanupInvalidReferences(SelectionHistory history)
        {
            try
            {
                var newHistory = new SelectionHistory(history.MaxHistoryCount);
                
                foreach (var selection in history.Selections)
                {
                    if (selection.HasValidObjects)
                    {
                        var cleanedSelection = selection.GetValidObjectsOnly();
                        if (cleanedSelection.Count > 0)
                        {
                            newHistory.AddSelection(cleanedSelection);
                        }
                    }
                }

                return newHistory;
            }
            catch (Exception ex)
            {
                Debug.LogError($"無効参照クリーンアップ中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }
    }
}