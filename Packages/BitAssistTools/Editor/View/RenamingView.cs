using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Domain.Models.Renaming;

namespace BitAssistTools.Editor.Renaming
{
    /// <summary>
    /// Renaming機能のView
    /// ViewModelを使用してUI状態とビジネスロジックを分離
    /// </summary>
    public class RenamingView : EditorWindow
    {
        private RenamingViewModel _viewModel;
        private Vector2 _scrollPosition;
        private const int SpacePixel = 8;

        [MenuItem("Bit Assist Tools/リネーマー", priority = 50)]
        public static void ShowWindow()
        {
            var window = GetWindow<RenamingView>("リネーマー");
            window.Initialize();
        }

        private void Initialize()
        {
            _viewModel = new RenamingViewModel();
            
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

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawCurrentSelectionInfo();
            EditorGUILayout.Space();

            DrawPrefixSuffixPanel();
            DrawStringReplacePanel();
            DrawTextureRenamePanel();
            DrawObjectNumberPanel();
            DrawPrefabAssetPanel();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 現在の選択状況表示
        /// </summary>
        private void DrawCurrentSelectionInfo()
        {
            var stats = _viewModel.GetCurrentStats();
            
            if (stats.HasSelection)
            {
                EditorGUILayout.HelpBox($"選択中: {stats}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("リネームするオブジェクトを選択してください。", MessageType.Warning);
            }
        }

        /// <summary>
        /// 接頭辞・接尾辞パネルの描画
        /// </summary>
        private void DrawPrefixSuffixPanel()
        {
            _viewModel.UI.PrefixSuffix = EditorGUILayout.Foldout(
                _viewModel.UI.PrefixSuffix, 
                "接頭辞と接尾辞を追加"
            );

            if (_viewModel.UI.PrefixSuffix)
            {
                GUILayout.Space(SpacePixel);

                string newPrefix = EditorGUILayout.TextField("接頭辞", _viewModel.PrefixSuffix.Prefix);
                if (newPrefix != _viewModel.PrefixSuffix.Prefix)
                {
                    _viewModel.UpdatePrefix(newPrefix);
                }

                string newSuffix = EditorGUILayout.TextField("接尾辞", _viewModel.PrefixSuffix.Suffix);
                if (newSuffix != _viewModel.PrefixSuffix.Suffix)
                {
                    _viewModel.UpdateSuffix(newSuffix);
                }

                string newDelimiter = EditorGUILayout.TextField("区切り文字", _viewModel.PrefixSuffix.Delimiter);
                if (newDelimiter != _viewModel.PrefixSuffix.Delimiter)
                {
                    _viewModel.UpdateDelimiter(newDelimiter);
                }

                EditorGUI.BeginDisabledGroup(!_viewModel.CanExecuteOperation(RenameOperationType.PrefixSuffix));
                if (GUILayout.Button("実行"))
                {
                    _viewModel.ExecutePrefixSuffixOperation();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(SpacePixel);
            }
        }

        /// <summary>
        /// 文字列置換パネルの描画
        /// </summary>
        private void DrawStringReplacePanel()
        {
            _viewModel.UI.StringReplace = EditorGUILayout.Foldout(
                _viewModel.UI.StringReplace, 
                "置換"
            );

            if (_viewModel.UI.StringReplace)
            {
                GUILayout.Space(SpacePixel);

                string newSearchString = EditorGUILayout.TextField("検索", _viewModel.StringReplace.SearchString);
                if (newSearchString != _viewModel.StringReplace.SearchString)
                {
                    _viewModel.UpdateSearchString(newSearchString);
                }

                string newReplaceString = EditorGUILayout.TextField("置換", _viewModel.StringReplace.ReplaceString);
                if (newReplaceString != _viewModel.StringReplace.ReplaceString)
                {
                    _viewModel.UpdateReplaceString(newReplaceString);
                }

                EditorGUI.BeginDisabledGroup(!_viewModel.CanExecuteOperation(RenameOperationType.StringReplace));
                if (GUILayout.Button("実行"))
                {
                    _viewModel.ExecuteStringReplaceOperation();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(SpacePixel);
            }
        }

        /// <summary>
        /// テクスチャリネームパネルの描画
        /// </summary>
        private void DrawTextureRenamePanel()
        {
            _viewModel.UI.TextureRename = EditorGUILayout.Foldout(
                _viewModel.UI.TextureRename, 
                "テクスチャを基準にリネーム"
            );

            if (_viewModel.UI.TextureRename)
            {
                GUILayout.Space(SpacePixel);

                EditorGUI.BeginDisabledGroup(!_viewModel.CanExecuteOperation(RenameOperationType.TextureName));
                if (GUILayout.Button("マテリアルをテクスチャの名前でリネーム"))
                {
                    _viewModel.ExecuteTextureNameOperation();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!_viewModel.CanExecuteOperation(RenameOperationType.MaterialName));
                if (GUILayout.Button("オブジェクトをマテリアルの名前でリネーム"))
                {
                    _viewModel.ExecuteMaterialNameOperation();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(SpacePixel);
            }
        }

        /// <summary>
        /// オブジェクト番号パネルの描画
        /// </summary>
        private void DrawObjectNumberPanel()
        {
            _viewModel.UI.ObjectNumber = EditorGUILayout.Foldout(
                _viewModel.UI.ObjectNumber, 
                "オブジェクトに番号を付与/除去"
            );

            if (_viewModel.UI.ObjectNumber)
            {
                GUILayout.Space(SpacePixel);

                EditorGUI.BeginDisabledGroup(!_viewModel.CanExecuteOperation(RenameOperationType.AddNumber));
                if (GUILayout.Button("オブジェクトに番号を付与"))
                {
                    _viewModel.ExecuteAddNumberOperation();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!_viewModel.CanExecuteOperation(RenameOperationType.RemoveNumber));
                if (GUILayout.Button("オブジェクトから番号を除去"))
                {
                    _viewModel.ExecuteRemoveNumberOperation();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(SpacePixel);
            }
        }

        /// <summary>
        /// Prefabアセットパネルの描画
        /// </summary>
        private void DrawPrefabAssetPanel()
        {
            _viewModel.UI.PrefabAsset = EditorGUILayout.Foldout(
                _viewModel.UI.PrefabAsset, 
                "Prefab Assetのリネーム"
            );

            if (_viewModel.UI.PrefabAsset)
            {
                GUILayout.Space(SpacePixel);

                EditorGUI.BeginDisabledGroup(!_viewModel.CanExecuteOperation(RenameOperationType.PrefabAsset));
                if (GUILayout.Button("選択したPrefabの名前でPrefab Assetをリネーム"))
                {
                    _viewModel.ExecutePrefabAssetOperation();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(SpacePixel);
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