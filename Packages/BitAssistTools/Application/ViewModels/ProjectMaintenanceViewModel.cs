using System;
using UnityEngine;
using BitAssistTools.Domain.Models.ProjectMaintenance;
using BitAssistTools.Application.Services;

namespace BitAssistTools.Application.ViewModels
{
    /// <summary>
    /// ProjectMaintenance機能のViewModel
    /// UI状態とビジネスロジックを分離
    /// </summary>
    public class ProjectMaintenanceViewModel
    {
        // UI展開状態管理
        public class UIExpansionState
        {
            public bool MissingScriptCleanup { get; set; } = true;
        }

        // イベント定義
        public event Action OnUIStateChanged;
        public event Action<string> OnError;
        public event Action<string> OnSuccess;

        // プロパティ
        public UIExpansionState UI { get; private set; }

        public ProjectMaintenanceViewModel()
        {
            UI = new UIExpansionState();
        }

        /// <summary>
        /// Missing Script削除を実行
        /// </summary>
        public void ExecuteRemoveMissingScripts()
        {
            try
            {
                if (!ProjectMaintenanceService.CanRemoveMissingScripts())
                {
                    NotifyError("Missing Scriptを削除するGameObjectを選択してください。");
                    return;
                }

                var beforeStats = ProjectMaintenanceService.GetMissingScriptStats();
                if (!beforeStats.HasMissingScripts)
                {
                    NotifySuccess("選択されたオブジェクトにMissing Scriptはありません。");
                    return;
                }

                ProjectMaintenanceService.RemoveMissingScriptsFromSelected();
                
                var afterStats = ProjectMaintenanceService.GetMissingScriptStats();
                int removedCount = beforeStats.TotalMissingScripts - afterStats.TotalMissingScripts;
                
                NotifySuccess($"Missing Script削除完了: {removedCount}個のコンポーネントを削除しました。");
            }
            catch (Exception ex)
            {
                NotifyError($"Missing Script削除中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// Missing Script削除が実行可能かどうかを判定
        /// </summary>
        public bool CanExecuteRemoveMissingScripts()
        {
            return ProjectMaintenanceService.CanRemoveMissingScripts();
        }

        /// <summary>
        /// 現在の選択状況の統計を取得
        /// </summary>
        public ProjectMaintenanceService.MissingScriptStats GetCurrentStats()
        {
            return ProjectMaintenanceService.GetMissingScriptStats();
        }

        /// <summary>
        /// 操作の実行可能性を取得
        /// </summary>
        public bool CanExecuteOperation(CleanupOperationType operationType)
        {
            switch (operationType)
            {
                case CleanupOperationType.MissingScripts:
                    return CanExecuteRemoveMissingScripts();
                default:
                    return false;
            }
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