using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Domain.Models.ProjectMaintenance;

namespace BitAssistTools.Editor.ProjectMaintenance
{
    /// <summary>
    /// ProjectMaintenance機能のView
    /// ViewModelを使用してUI状態とビジネスロジックを分離
    /// </summary>
    public class ProjectMaintenanceView : EditorWindow
    {
        private ProjectMaintenanceViewModel _viewModel;

        [MenuItem("Bit Assist Tools/Utility/Missing Scriptsを削除", priority = 70)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectMaintenanceView>("プロジェクトメンテナンス");
            window.Initialize();
        }

        private void Initialize()
        {
            _viewModel = new ProjectMaintenanceViewModel();
            
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

            DrawCurrentSelectionInfo();
            EditorGUILayout.Space();
            DrawMissingScriptCleanupPanel();
        }

        /// <summary>
        /// 現在の選択状況表示
        /// </summary>
        private void DrawCurrentSelectionInfo()
        {
            var stats = _viewModel.GetCurrentStats();
            
            if (stats.HasMissingScripts)
            {
                EditorGUILayout.HelpBox($"検出: {stats}", MessageType.Warning);
            }
            else if (_viewModel.CanExecuteRemoveMissingScripts())
            {
                EditorGUILayout.HelpBox($"選択中: {stats}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("クリーンアップするGameObjectを選択してください。", MessageType.Info);
            }
        }

        /// <summary>
        /// Missing Scriptクリーンアップパネルの描画
        /// </summary>
        private void DrawMissingScriptCleanupPanel()
        {
            _viewModel.UI.MissingScriptCleanup = EditorGUILayout.Foldout(
                _viewModel.UI.MissingScriptCleanup, 
                "Missing Script クリーンアップ"
            );

            if (_viewModel.UI.MissingScriptCleanup)
            {
                EditorGUILayout.Space();

                EditorGUILayout.HelpBox(
                    "選択されたGameObjectとその子オブジェクトから、Missing Scriptコンポーネントを削除します。\n" +
                    "Prefabの場合は、ソースアセットも同時に処理されます。",
                    MessageType.Info
                );

                EditorGUILayout.Space();

                bool canExecute = _viewModel.CanExecuteOperation(CleanupOperationType.MissingScripts);
                EditorGUI.BeginDisabledGroup(!canExecute);
                
                if (GUILayout.Button("Missing Scripts を削除"))
                {
                    if (EditorUtility.DisplayDialog(
                        "Missing Scripts削除の確認",
                        "選択されたオブジェクトからMissing Scriptコンポーネントを削除します。\nこの操作は元に戻すことができます。\n\n実行しますか？",
                        "実行",
                        "キャンセル"))
                    {
                        _viewModel.ExecuteRemoveMissingScripts();
                    }
                }
                
                EditorGUI.EndDisabledGroup();

                if (!canExecute)
                {
                    EditorGUILayout.HelpBox(
                        "HierarchyでクリーンアップするGameObjectを選択してください。",
                        MessageType.Warning
                    );
                }

                EditorGUILayout.Space();
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
        }
    }
}