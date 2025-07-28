using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Infrastructure.Unity;
using UnitySelection = UnityEditor.Selection;

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
            
            // Unity選択変更イベントの購読
            UnitySelection.selectionChanged += OnUnitySelectionChanged;
            
            // 初回プレビュー更新
            _viewModel.UpdateHierarchicalSelectionPreview();
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
            
            // Unity選択変更イベントの購読解除
            UnitySelection.selectionChanged -= OnUnitySelectionChanged;
            
            // ハイライト停止
            UnitySceneViewHighlighter.StopHighlight();
            UnityHierarchyHighlighter.StopHighlight();
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
                // プレビューハイライトのトグル
                DrawPreviewHighlightToggle();
                
                GUILayout.Space(5);
                
                DrawHierarchicalSelectionControls();
            }
        }

        /// <summary>
        /// 階層選択コントロールの描画
        /// </summary>
        private void DrawHierarchicalSelectionControls()
        {
            bool settingsChanged = false;

            // 基準とする親の階層
            var newParentDepth = EditorGUILayout.IntSlider(
                "基準とする親の階層", 
                _viewModel.HierarchicalSelection.ParentDepth, 
                1, 
                5
            );
            if (newParentDepth != _viewModel.HierarchicalSelection.ParentDepth)
            {
                _viewModel.HierarchicalSelection.ParentDepth = newParentDepth;
                settingsChanged = true;
            }

            // 選択したオブジェクトと同じ順番
            var newOnlyOrder = EditorGUILayout.Toggle(
                "選択したオブジェクトと同じ順番", 
                _viewModel.HierarchicalSelection.OnlyOrder
            );
            if (newOnlyOrder != _viewModel.HierarchicalSelection.OnlyOrder)
            {
                _viewModel.HierarchicalSelection.OnlyOrder = newOnlyOrder;
                settingsChanged = true;
            }

            // 子オブジェクトをすべて選択する
            var newSelectChildren = EditorGUILayout.Toggle(
                "子オブジェクトをすべて選択する", 
                _viewModel.HierarchicalSelection.SelectChildren
            );
            if (newSelectChildren != _viewModel.HierarchicalSelection.SelectChildren)
            {
                _viewModel.HierarchicalSelection.SelectChildren = newSelectChildren;
                settingsChanged = true;
            }

            // 設定が変更された場合、プレビューを更新
            if (settingsChanged)
            {
                _viewModel.OnHierarchicalSelectionSettingsChanged();
            }

            GUILayout.Space(5);

            // プレビュー情報の表示
            DrawPreviewSection();

            GUILayout.Space(5);

            // 実行ボタンとプレビューボタン
            DrawExecutionButtons();
        }

        /// <summary>
        /// プレビューハイライトトグルの描画
        /// </summary>
        private void DrawPreviewHighlightToggle()
        {
            var currentHighlightState = UnityHierarchyHighlighter.IsHighlightEnabled;
            var preview = _viewModel.CurrentPreview;
            
            // プレビューが無効な場合はトグルを無効化
            EditorGUI.BeginDisabledGroup(!preview.IsValid);
            
            var newHighlightState = EditorGUILayout.Toggle("📋 プレビューハイライト", currentHighlightState);
            
            EditorGUI.EndDisabledGroup();
            
            // 状態が変更された場合
            if (newHighlightState != currentHighlightState)
            {
                if (newHighlightState && preview.IsValid)
                {
                    UnityHierarchyHighlighter.StartHighlight(preview);
                }
                else
                {
                    UnityHierarchyHighlighter.StopHighlight();
                }
            }
            
            // プレビューが無効な場合の説明
            if (!preview.IsValid && newHighlightState)
            {
                EditorGUILayout.HelpBox("プレビューを表示するには、有効なGameObjectを選択してください。", MessageType.Info);
            }
        }

        /// <summary>
        /// プレビューセクションの描画
        /// </summary>
        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField("プレビュー情報", EditorStyles.boldLabel);
            
            var preview = _viewModel.CurrentPreview;
            
            if (!preview.IsValid)
            {
                EditorGUILayout.HelpBox(preview.ErrorMessage, MessageType.Warning);
                return;
            }

            // 現在の選択オブジェクト
            var selectedObjects = UnitySelection.gameObjects;
            if (selectedObjects != null && selectedObjects.Length > 1)
            {
                EditorGUILayout.LabelField("現在選択:", $"{selectedObjects.Length}個のオブジェクト");
                
                // 最初の3個のオブジェクト名を表示
                var displayCount = Mathf.Min(selectedObjects.Length, 3);
                var objectNames = new string[displayCount];
                for (int i = 0; i < displayCount; i++)
                {
                    objectNames[i] = selectedObjects[i]?.name ?? "null";
                }
                var nameList = string.Join(", ", objectNames);
                if (selectedObjects.Length > 3)
                {
                    nameList += $"... 他{selectedObjects.Length - 3}個";
                }
                EditorGUILayout.LabelField(" ", nameList, EditorStyles.wordWrappedLabel);
            }
            else
            {
                var currentSelection = UnitySelection.activeGameObject;
                EditorGUILayout.LabelField("現在選択:", currentSelection?.name ?? "なし");
            }

            // 基準親
            EditorGUILayout.LabelField("基準親:", preview.BaseParent?.name ?? "なし");

            // 兄弟数と選択対象数
            EditorGUILayout.LabelField("兄弟:", $"{preview.ParentSiblingCount}個");
            EditorGUILayout.LabelField("選択対象:", $"{preview.TargetCount}個");

            // 選択理由
            if (!string.IsNullOrEmpty(preview.SelectionReason))
            {
                EditorGUILayout.LabelField("理由:", preview.SelectionReason, EditorStyles.wordWrappedLabel);
            }

            // 選択対象のオブジェクト名を表示（最大5個まで）
            if (preview.TargetObjects != null && preview.TargetObjects.Length > 0)
            {
                var displayCount = Mathf.Min(preview.TargetObjects.Length, 5);
                var objectNames = new string[displayCount];
                
                for (int i = 0; i < displayCount; i++)
                {
                    objectNames[i] = preview.TargetObjects[i]?.name ?? "null";
                }
                
                var nameList = string.Join(", ", objectNames);
                if (preview.TargetObjects.Length > 5)
                {
                    nameList += $"... 他{preview.TargetObjects.Length - 5}個";
                }
                
                EditorGUILayout.LabelField("対象:", nameList, EditorStyles.wordWrappedLabel);
            }
        }

        /// <summary>
        /// 実行ボタンセクションの描画
        /// </summary>
        private void DrawExecutionButtons()
        {
            var preview = _viewModel.CurrentPreview;
            bool canExecute = _viewModel.CanExecuteHierarchicalSelection() && preview.IsValid;
            
            // 選択実行ボタン
            EditorGUI.BeginDisabledGroup(!canExecute);
            
            if (GUILayout.Button("選択実行"))
            {
                _viewModel.ExecuteHierarchicalSelection();
                // 実行後はハイライトを停止
                UnityHierarchyHighlighter.StopHighlight();
            }
            
            EditorGUI.EndDisabledGroup();

            if (!canExecute && preview.IsValid)
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
        /// Unity選択変更イベントハンドラー
        /// </summary>
        private void OnUnitySelectionChanged()
        {
            if (_viewModel != null)
            {
                _viewModel.UpdateHierarchicalSelectionPreview();
            }
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