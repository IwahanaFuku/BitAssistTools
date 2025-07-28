using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// Unity Hierarchyでオブジェクトのハイライト表示を行うInfrastructureサービス
    /// </summary>
    public static class UnityHierarchyHighlighter
    {
        private static readonly Color BaseParentColor = new Color(0.0f, 0.8f, 0.0f, 0.3f); // 緑色（半透明）
        private static readonly Color TargetObjectColor = new Color(0.0f, 0.4f, 1.0f, 0.3f); // 青色（半透明）
        
        private static HierarchicalSelectionPreview _currentPreview;
        private static bool _isHighlightEnabled = false;
        private static HashSet<int> _baseParentIds;
        private static HashSet<int> _targetObjectIds;

        /// <summary>
        /// ハイライト表示を開始
        /// </summary>
        public static void StartHighlight(HierarchicalSelectionPreview preview)
        {
            _currentPreview = preview;
            _isHighlightEnabled = true;
            
            // オブジェクトIDセットを作成
            UpdateObjectIdSets();
            
            // HierarchyのGUIイベントに登録
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            
            // Hierarchyを再描画
            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// ハイライト表示を停止
        /// </summary>
        public static void StopHighlight()
        {
            _isHighlightEnabled = false;
            
            // HierarchyのGUIイベントから登録解除
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            
            // オブジェクトIDセットをクリア
            _baseParentIds?.Clear();
            _targetObjectIds?.Clear();
            
            // Hierarchyを再描画
            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// プレビュー情報を更新（ハイライト継続）
        /// </summary>
        public static void UpdatePreview(HierarchicalSelectionPreview preview)
        {
            _currentPreview = preview;
            
            if (_isHighlightEnabled)
            {
                UpdateObjectIdSets();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        /// <summary>
        /// ハイライトが有効かどうか
        /// </summary>
        public static bool IsHighlightEnabled => _isHighlightEnabled;

        /// <summary>
        /// オブジェクトIDセットを更新
        /// </summary>
        private static void UpdateObjectIdSets()
        {
            _baseParentIds = new HashSet<int>();
            _targetObjectIds = new HashSet<int>();

            if (!_currentPreview.IsValid) return;

            // 基準親のIDを追加
            if (_currentPreview.BaseParent != null)
            {
                _baseParentIds.Add(_currentPreview.BaseParent.GetInstanceID());
            }

            // 選択対象のIDを追加
            if (_currentPreview.TargetObjects != null)
            {
                foreach (var targetObj in _currentPreview.TargetObjects)
                {
                    if (targetObj != null)
                    {
                        _targetObjectIds.Add(targetObj.GetInstanceID());
                    }
                }
            }
        }

        /// <summary>
        /// Hierarchy項目のGUI描画イベント
        /// </summary>
        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            if (!_isHighlightEnabled) return;

            try
            {
                // 基準親のハイライト
                if (_baseParentIds != null && _baseParentIds.Contains(instanceID))
                {
                    DrawHierarchyHighlight(selectionRect, BaseParentColor);
                    DrawHierarchyLabel(selectionRect, "基準親", BaseParentColor);
                }
                // 選択対象のハイライト
                else if (_targetObjectIds != null && _targetObjectIds.Contains(instanceID))
                {
                    DrawHierarchyHighlight(selectionRect, TargetObjectColor);
                    DrawHierarchyLabel(selectionRect, "選択対象", TargetObjectColor);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityHierarchyHighlighter] Hierarchy項目描画中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// Hierarchyでのハイライト背景を描画
        /// </summary>
        private static void DrawHierarchyHighlight(Rect rect, Color color)
        {
            // 少し大きめの矩形を作成（余白を含める）
            var highlightRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            
            // 背景色の描画
            EditorGUI.DrawRect(highlightRect, color);
        }

        /// <summary>
        /// Hierarchyでのラベルを描画
        /// </summary>
        private static void DrawHierarchyLabel(Rect rect, string label, Color color)
        {
            // ラベル表示位置（右端）
            var labelRect = new Rect(rect.x + rect.width - 60, rect.y, 55, rect.height);
            
            // 元のGUI色を保存
            var originalColor = GUI.color;
            var originalContentColor = GUI.contentColor;
            
            try
            {
                // ラベル用のスタイルを作成
                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                    fontSize = 9,
                    fontStyle = FontStyle.Bold
                };
                
                // ラベル色を設定（アルファを高くして見やすくする）
                var labelColor = new Color(color.r, color.g, color.b, 1.0f);
                labelStyle.normal.textColor = labelColor;
                
                // ラベルを描画
                GUI.Label(labelRect, label, labelStyle);
            }
            finally
            {
                // 元のGUI色を復元
                GUI.color = originalColor;
                GUI.contentColor = originalContentColor;
            }
        }

        /// <summary>
        /// 指定されたオブジェクトがハイライト対象かどうかを判定
        /// </summary>
        public static bool IsHighlightedObject(GameObject obj)
        {
            if (!_isHighlightEnabled || obj == null) return false;
            
            var instanceId = obj.GetInstanceID();
            return (_baseParentIds != null && _baseParentIds.Contains(instanceId)) ||
                   (_targetObjectIds != null && _targetObjectIds.Contains(instanceId));
        }

        /// <summary>
        /// 指定されたオブジェクトのハイライト種別を取得
        /// </summary>
        public static string GetHighlightType(GameObject obj)
        {
            if (!_isHighlightEnabled || obj == null) return null;
            
            var instanceId = obj.GetInstanceID();
            
            if (_baseParentIds != null && _baseParentIds.Contains(instanceId))
                return "基準親";
            
            if (_targetObjectIds != null && _targetObjectIds.Contains(instanceId))
                return "選択対象";
            
            return null;
        }
    }
}