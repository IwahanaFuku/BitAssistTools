using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;

namespace BitAssistTools.Editor.Selection
{
    /// <summary>
    /// Selection機能のView
    /// ViewModelを使用してUI状態とビジネスロジックを分離
    /// </summary>
    public class SelectionView : EditorWindow
    {
        private SelectionViewModel _viewModel;

        // UI レイアウト用の変数
        private Rect _leftRect = new Rect();
        private Rect _rightRect = new Rect();
        private GUIStyle _leftPanelStyle = null;

        [MenuItem("Bit Assist Tools/選択支援ツール", priority = 13)]
        public static void ShowWindow()
        {
            var window = GetWindow<SelectionView>("選択支援ツール");
            window.Initialize();
        }

        private void Initialize()
        {
            _viewModel = new SelectionViewModel();
            
            // イベント購読
            _viewModel.OnUIStateChanged += Repaint;
            _viewModel.OnError += ShowError;
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (_viewModel != null)
            {
                _viewModel.OnUIStateChanged -= Repaint;
                _viewModel.OnError -= ShowError;
                _viewModel.Dispose();
            }
        }

        private void OnGUI()
        {
            if (_viewModel == null)
            {
                Initialize();
            }

            CalculateRects();
            SetBackgroundStyles();

            // 左パネル: 選択履歴
            GUILayout.BeginArea(_leftRect, _leftPanelStyle);
            DrawSelectionHistoryPanel();
            GUILayout.EndArea();

            // 右パネル: 階層選択
            GUILayout.BeginArea(_rightRect);
            DrawHierarchicalSelectionPanel();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 選択履歴パネルの描画
        /// </summary>
        private void DrawSelectionHistoryPanel()
        {
            GUILayout.Label("選択履歴の保存", EditorStyles.boldLabel);

            // 最大履歴数設定
            _viewModel.HistorySettings.MaxHistoryCount = (int)EditorGUILayout.FloatField(
                "  履歴保存数", 
                _viewModel.HistorySettings.MaxHistoryCount
            );

            EditorGUILayout.LabelField("  選択履歴");

            // 履歴追加ボタン
            if (GUILayout.Button("追加"))
            {
                _viewModel.ExecuteAddSelection();
            }

            // 履歴リスト表示
            if (_viewModel.SelectionHistory.CurrentCount > 0)
            {
                DrawHistoryList();
            }

            GUILayout.FlexibleSpace();

            // 操作ボタン群
            DrawHistoryOperationButtons();
        }

        /// <summary>
        /// 履歴リストの描画
        /// </summary>
        private void DrawHistoryList()
        {
            _viewModel.HistoryScrollPosition = EditorGUILayout.BeginScrollView(_viewModel.HistoryScrollPosition);

            for (int i = 0; i < _viewModel.SelectionHistory.CurrentCount; i++)
            {
                GUILayout.BeginHorizontal();

                // チェックボックス
                bool currentCheck = i < _viewModel.HistoryItemCheckStates.Count ? 
                    _viewModel.HistoryItemCheckStates[i] : false;
                bool newCheck = GUILayout.Toggle(currentCheck, "");
                
                if (newCheck != currentCheck)
                {
                    _viewModel.ToggleHistoryItemCheck(i);
                }

                // 名前フィールド
                string currentName = i < _viewModel.HistoryItemNames.Count ? 
                    _viewModel.HistoryItemNames[i] : "Unknown";
                string newName = EditorGUILayout.TextField("", currentName);
                
                if (newName != currentName)
                {
                    _viewModel.UpdateHistoryItemName(i, newName);
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 履歴操作ボタン群の描画
        /// </summary>
        private void DrawHistoryOperationButtons()
        {
            if (GUILayout.Button("ロード"))
            {
                _viewModel.ExecuteLoadSelection();
            }

            if (GUILayout.Button("削除"))
            {
                _viewModel.ExecuteDeleteSelection();
            }

            if (GUILayout.Button("全選択"))
            {
                _viewModel.ExecuteToggleAllSelection();
            }

            // デバッグ情報表示（統計）
            if (UnityEngine.Application.isPlaying || EditorApplication.isPlaying)
            {
                var stats = _viewModel.GetHistoryStats();
                EditorGUILayout.LabelField($"統計: {stats.TotalSelections}件 (アクティブ: {stats.ActiveSelections}件)");
            }

            // クリーンアップボタン
            if (GUILayout.Button("無効参照クリーンアップ"))
            {
                _viewModel.CleanupInvalidReferences();
            }
        }

        /// <summary>
        /// 階層選択パネルの描画
        /// </summary>
        private void DrawHierarchicalSelectionPanel()
        {
            _viewModel.UI.HierarchicalSelection = EditorGUILayout.Foldout(
                _viewModel.UI.HierarchicalSelection, 
                "階層選択"
            );

            if (_viewModel.UI.HierarchicalSelection)
            {
                DrawHierarchicalSelectionControls();
            }
        }

        /// <summary>
        /// 階層選択コントロールの描画
        /// </summary>
        private void DrawHierarchicalSelectionControls()
        {
            // 基準とする親の階層
            _viewModel.HierarchicalSelection.ParentDepth = EditorGUILayout.IntSlider(
                "基準とする親の階層", 
                _viewModel.HierarchicalSelection.ParentDepth, 
                1, 
                5
            );

            // 選択したオブジェクトと同じ順番
            _viewModel.HierarchicalSelection.OnlyOrder = EditorGUILayout.Toggle(
                "選択したオブジェクトと同じ順番", 
                _viewModel.HierarchicalSelection.OnlyOrder
            );

            // 子オブジェクトをすべて選択する
            _viewModel.HierarchicalSelection.SelectChildren = EditorGUILayout.Toggle(
                "子オブジェクトをすべて選択する", 
                _viewModel.HierarchicalSelection.SelectChildren
            );

            GUILayout.Space(5);

            // 実行可能かどうかの状態表示
            bool canExecute = _viewModel.CanExecuteHierarchicalSelection();
            
            EditorGUI.BeginDisabledGroup(!canExecute);
            
            if (GUILayout.Button("選択"))
            {
                _viewModel.ExecuteHierarchicalSelection();
            }
            
            EditorGUI.EndDisabledGroup();

            if (!canExecute)
            {
                EditorGUILayout.HelpBox(
                    "アクティブなGameObjectを選択するか、親階層の設定を調整してください。", 
                    MessageType.Info
                );
            }
        }

        /// <summary>
        /// レクト計算
        /// </summary>
        private void CalculateRects()
        {
            _leftRect.Set(0, 0, position.width / 2, position.height);
            _rightRect.Set(position.width / 2, 0, position.width / 2, position.height);
        }

        /// <summary>
        /// 背景スタイル設定
        /// </summary>
        private void SetBackgroundStyles()
        {
            if (_leftPanelStyle == null)
            {
                _leftPanelStyle = new GUIStyle(GUI.skin.box);
                _leftPanelStyle.normal.background = CreateColoredTexture(
                    EditorGUIUtility.isProSkin ? 0.2f : 0.8f
                );
            }
        }

        /// <summary>
        /// 色付きテクスチャ作成
        /// </summary>
        private Texture2D CreateColoredTexture(float brightness)
        {
            Color bgColor = GUI.backgroundColor;
            Color adjustedColor = new Color(
                bgColor.r * brightness, 
                bgColor.g * brightness, 
                bgColor.b * brightness
            );
            
            int w = (int)_leftRect.width;
            int h = (int)_leftRect.height;
            Color[] pix = new Color[w * h];
            
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = adjustedColor;
            }
            
            Texture2D result = new Texture2D(w, h);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        /// <summary>
        /// エラー表示
        /// </summary>
        private void ShowError(string message)
        {
            EditorUtility.DisplayDialog("エラー", message, "OK");
        }
    }
}