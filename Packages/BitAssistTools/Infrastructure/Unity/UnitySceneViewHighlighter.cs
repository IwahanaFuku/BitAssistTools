using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// Unity HierarchyとScene Viewでオブジェクトのハイライト表示を行うInfrastructureサービス
    /// </summary>
    public static class UnitySceneViewHighlighter
    {
        private static readonly Color BaseParentColor = Color.green;
        private static readonly Color TargetObjectColor = Color.blue;
        private static readonly Color OutlineColor = Color.white;
        private static readonly float OutlineWidth = 2.0f;

        private static HierarchicalSelectionPreview _currentPreview;
        private static bool _isHighlightEnabled = false;

        /// <summary>
        /// ハイライト表示を開始
        /// </summary>
        public static void StartHighlight(HierarchicalSelectionPreview preview)
        {
            _currentPreview = preview;
            _isHighlightEnabled = true;
            
            // SceneViewのGUIイベントに登録
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// ハイライト表示を停止
        /// </summary>
        public static void StopHighlight()
        {
            _isHighlightEnabled = false;
            
            // SceneViewのGUIイベントから登録解除
            SceneView.duringSceneGui -= OnSceneGUI;
            
            // SceneViewを再描画
            SceneView.RepaintAll();
        }

        /// <summary>
        /// プレビュー情報を更新（ハイライト継続）
        /// </summary>
        public static void UpdatePreview(HierarchicalSelectionPreview preview)
        {
            _currentPreview = preview;
            
            if (_isHighlightEnabled)
            {
                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// ハイライトが有効かどうか
        /// </summary>
        public static bool IsHighlightEnabled => _isHighlightEnabled;

        /// <summary>
        /// Scene GUI描画イベント
        /// </summary>
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!_isHighlightEnabled || !_currentPreview.IsValid)
                return;

            try
            {
                // ハンドルの元の色を保存
                var originalColor = Handles.color;
                var originalMatrix = Handles.matrix;

                // 基準親オブジェクトのハイライト
                if (_currentPreview.BaseParent != null)
                {
                    DrawObjectHighlight(_currentPreview.BaseParent, BaseParentColor, "基準親");
                }

                // 選択対象オブジェクトのハイライト
                if (_currentPreview.TargetObjects != null)
                {
                    foreach (var targetObj in _currentPreview.TargetObjects)
                    {
                        if (targetObj != null)
                        {
                            DrawObjectHighlight(targetObj, TargetObjectColor, "選択対象");
                        }
                    }
                }

                // 元の色とマトリックスを復元
                Handles.color = originalColor;
                Handles.matrix = originalMatrix;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnitySceneViewHighlighter] ハイライト描画中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 個別オブジェクトのハイライト描画
        /// </summary>
        private static void DrawObjectHighlight(GameObject obj, Color color, string label)
        {
            if (obj == null) return;

            var transform = obj.transform;
            var position = transform.position;

            // バウンディングボックスを取得
            var bounds = GetObjectBounds(obj);
            
            if (bounds.size != Vector3.zero)
            {
                // バウンディングボックスに基づいてハイライト
                DrawBoundsHighlight(bounds, color, label);
            }
            else
            {
                // バウンディングボックスがない場合はポイントハイライト
                DrawPointHighlight(position, color, label);
            }
        }

        /// <summary>
        /// バウンディングボックスハイライトの描画
        /// </summary>
        private static void DrawBoundsHighlight(Bounds bounds, Color color, string label)
        {
            // 輪郭線の描画
            Handles.color = OutlineColor;
            Handles.DrawWireCube(bounds.center, bounds.size);
            
            // メインカラーでのハイライト
            Handles.color = new Color(color.r, color.g, color.b, 0.3f);
            
            // 半透明の面を描画
            var corners = GetBoundsCorners(bounds);
            DrawHighlightFaces(corners);
            
            // ラベルの描画
            DrawLabel(bounds.center + Vector3.up * (bounds.size.y * 0.5f + 0.5f), label, color);
        }

        /// <summary>
        /// ポイントハイライトの描画
        /// </summary>
        private static void DrawPointHighlight(Vector3 position, Color color, string label)
        {
            // 円形のハイライト
            Handles.color = color;
            Handles.DrawWireDisc(position, Vector3.up, 1.0f);
            
            // ラベルの描画
            DrawLabel(position + Vector3.up * 1.5f, label, color);
        }

        /// <summary>
        /// ラベルの描画
        /// </summary>
        private static void DrawLabel(Vector3 position, string text, Color color)
        {
            var guiColor = GUI.color;
            GUI.color = color;
            
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = color },
                fontSize = 12
            };
            
            Handles.Label(position, text, style);
            GUI.color = guiColor;
        }

        /// <summary>
        /// オブジェクトのバウンディングボックスを取得
        /// </summary>
        private static Bounds GetObjectBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(obj.transform.position, Vector3.zero);
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        /// <summary>
        /// バウンディングボックスの角の座標を取得
        /// </summary>
        private static Vector3[] GetBoundsCorners(Bounds bounds)
        {
            var center = bounds.center;
            var extents = bounds.extents;
            
            return new Vector3[]
            {
                center + new Vector3(-extents.x, -extents.y, -extents.z), // 0: left-bottom-back
                center + new Vector3(+extents.x, -extents.y, -extents.z), // 1: right-bottom-back
                center + new Vector3(+extents.x, +extents.y, -extents.z), // 2: right-top-back
                center + new Vector3(-extents.x, +extents.y, -extents.z), // 3: left-top-back
                center + new Vector3(-extents.x, -extents.y, +extents.z), // 4: left-bottom-front
                center + new Vector3(+extents.x, -extents.y, +extents.z), // 5: right-bottom-front
                center + new Vector3(+extents.x, +extents.y, +extents.z), // 6: right-top-front
                center + new Vector3(-extents.x, +extents.y, +extents.z)  // 7: left-top-front
            };
        }

        /// <summary>
        /// ハイライト面の描画
        /// </summary>
        private static void DrawHighlightFaces(Vector3[] corners)
        {
            // 6つの面を描画（前後左右上下）
            
            // 前面
            DrawQuad(corners[4], corners[5], corners[6], corners[7]);
            
            // 後面
            DrawQuad(corners[3], corners[2], corners[1], corners[0]);
            
            // 左面
            DrawQuad(corners[0], corners[4], corners[7], corners[3]);
            
            // 右面
            DrawQuad(corners[1], corners[2], corners[6], corners[5]);
            
            // 上面
            DrawQuad(corners[3], corners[7], corners[6], corners[2]);
            
            // 下面
            DrawQuad(corners[0], corners[1], corners[5], corners[4]);
        }

        /// <summary>
        /// 四角形の描画
        /// </summary>
        private static void DrawQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Handles.DrawAAConvexPolygon(p1, p2, p3, p4);
        }
    }
}