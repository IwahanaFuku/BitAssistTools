using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Application.Services;
using ColliderPlacemenAssistancer;
using AbstractPosition = BitAssistTools.Application.Services.AbstractPosition;
using PointObjectInfo = BitAssistTools.Application.ViewModels.PointObjectInfo;

namespace BitAssistTools.Editor.ColliderPlacement
{
    /// <summary>
    /// ColliderPlacement機能のView（リファクタリング版）
    /// ViewModelを使用してUI状態とビジネスロジックを分離
    /// </summary>
    public class ColliderPlacementView : EditorWindow
    {
        private ColliderPlacementViewModel _viewModel;
        private ReorderableList _reorderableList;
        private GUIStyle _firstStyle;
        private Rect _firstRect;
        private Rect _secondRect;

        [MenuItem("Bit Assist Tools/コライダー配置支援ツール（リファクタリング版）", priority = 12)]
        public static void ShowWindow()
        {
            var window = GetWindow<ColliderPlacementView>("コライダー配置支援ツール（新）");
            window.Initialize();
        }

        private void Initialize()
        {
            _viewModel = new ColliderPlacementViewModel();
            
            // イベント購読
            _viewModel.OnDataChanged += OnViewModelDataChanged;
            _viewModel.OnUIStateChanged += Repaint;
            _viewModel.OnError += ShowError;
            
            // ViewModelが初期化された後でReorderableListを初期化
            InitializeReorderableList();
            
            // リアルタイム監視開始
            EditorApplication.update += CheckObjectPositions;
        }

        private void InitializeReorderableList()
        {
            _reorderableList = new ReorderableList(_viewModel.Data.Points, typeof(PointObjectInfo), true, true, true, true);
            
            _reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                // クリアボタンを追加（旧実装互換）
                Rect buttonRect = new Rect(rect.x + rect.width - 60f, rect.y, 60f, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(buttonRect, "クリア"))
                {
                    ClearAllPoints();
                }
                
                // ヘッダーラベル
                Rect labelRect = new Rect(rect.x, rect.y, rect.width - 65f, rect.height);
                EditorGUI.LabelField(labelRect, "ポイントオブジェクト");
            };
            
            _reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var points = _viewModel.Data.Points;
                if (index >= 0 && index < points.Count)
                {
                    var element = points[index];
                    rect.y += 2;
                    
                    // オブジェクトフィールドの幅を調整（旧実装互換）
                    float originalWidth = rect.width;
                    int ratio = 8;
                    rect.width = originalWidth * (ratio - 1) / ratio;
                    rect.x += originalWidth / ratio;
                    
                    var newGameObject = (GameObject)EditorGUI.ObjectField(
                        rect, element.GameObject, typeof(GameObject), true);
                    
                    if (newGameObject != element.GameObject)
                    {
                        _viewModel.UpdatePointGameObject(index, newGameObject);
                    }
                }
            };
            
            _reorderableList.onAddCallback = (ReorderableList list) =>
            {
                Debug.Log("onAddCallback called");
                _viewModel.AddPoint();
                Debug.Log($"Point added. Total count: {_viewModel.Data.Points.Count}");
                list.index = _viewModel.Data.Points.Count - 1; // 新しいアイテムを選択
            };
            
            _reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                if (list.index >= 0)
                {
                    _viewModel.RemovePointAt(list.index);
                }
            };
            
            // ReorderableListの自動並び替えのみを使用（旧版互換）
            // onReorderCallbackWithDetailsは削除し、ReorderableListに完全に委ねる
            
            // 旧実装互換：リスト変更時の自動アップデート
            _reorderableList.onChangedCallback = (ReorderableList list) =>
            {
                Debug.Log("onChangedCallback called, auto-update check");
                if (_viewModel.UI.IsAutoUpdate)
                {
                    Debug.Log("Auto-update enabled, generating box connections");
                    _viewModel.GenerateBoxConnections();
                }
            };
        }

        /// <summary>
        /// 全ポイントをクリア（旧版互換：GameObjectを物理削除）
        /// </summary>
        private void ClearAllPoints()
        {
            // 旧版互換：GameObjectを物理削除
            var gameObjectsToDelete = _viewModel.Data.Points
                .Where(p => p.GameObject != null)
                .Select(p => p.GameObject)
                .ToArray();
            
            if (gameObjectsToDelete.Length > 0)
            {
                Undo.RecordObjects(gameObjectsToDelete, "Clear Points");
                
                foreach (var obj in gameObjectsToDelete)
                {
                    Undo.DestroyObjectImmediate(obj);
                }
                
                Undo.FlushUndoRecordObjects();
            }
            
            _viewModel.Data.Points.Clear();
            _viewModel.NotifyDataChanged();
        }

        private void OnViewModelDataChanged()
        {
            // ReorderableListはすでに同じリスト参照を持っているので更新不要
            // しかし念のため確実に同期
            if (_reorderableList != null && _reorderableList.list != _viewModel.Data.Points)
            {
                _reorderableList.list = _viewModel.Data.Points;
            }
            Repaint();
        }

        private void OnDestroy()
        {
            // リアルタイム監視停止
            EditorApplication.update -= CheckObjectPositions;
            
            // イベント購読解除
            if (_viewModel != null)
            {
                _viewModel.OnDataChanged -= OnViewModelDataChanged;
                _viewModel.OnUIStateChanged -= Repaint;
                _viewModel.OnError -= ShowError;
            }
        }

        private void OnGUI()
        {
            if (_viewModel == null)
            {
                Initialize();
            }
            
            if (_reorderableList == null)
            {
                Debug.LogWarning("ReorderableList is null, reinitializing...");
                InitializeReorderableList();
            }

            CalculateRects();
            SetBackgroundStyles();

            GUILayout.BeginArea(_firstRect, _firstStyle);
            DrawConnectionColliderGUI();
            GUILayout.EndArea();

            GUILayout.BeginArea(_secondRect);
            DrawColliderUtilityGUI();
            GUILayout.EndArea();
        }

        private void CalculateRects()
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            float dividerPosition = windowWidth * 0.6f;

            _firstRect = new Rect(0, 0, dividerPosition, windowHeight);
            _secondRect = new Rect(dividerPosition, 0, windowWidth - dividerPosition, windowHeight);
        }

        private void SetBackgroundStyles()
        {
            if (_firstStyle == null)
            {
                _firstStyle = new GUIStyle(GUI.skin.box);
                _firstStyle.normal.background = CreateColoredTexture(EditorGUIUtility.isProSkin ? 0.2f : 0.8f);
            }
        }

        private Texture2D CreateColoredTexture(float brightness)
        {
            Color bgColor = GUI.backgroundColor;
            Color adjustedColor = new Color(bgColor.r * brightness, bgColor.g * brightness, bgColor.b * brightness);
            int w = (int)_firstRect.width;
            int h = (int)_firstRect.height;
            if (w <= 0 || h <= 0)
            {
                return null;
            }
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

        private void DrawConnectionColliderGUI()
        {
            GUILayout.Label("ボックスコネクションツール", EditorStyles.boldLabel);
            
            _viewModel.UI.ConnectionScrollPos = EditorGUILayout.BeginScrollView(_viewModel.UI.ConnectionScrollPos);

            // 自動アップデート設定
            var newAutoUpdate = EditorGUILayout.Toggle("  自動アップデート", _viewModel.UI.IsAutoUpdate);
            if (newAutoUpdate != _viewModel.UI.IsAutoUpdate)
            {
                _viewModel.UI.IsAutoUpdate = newAutoUpdate;
            }

            GUILayout.Space(_viewModel.BoxSettings.SpacePixel);

            // ポイントリストペアレント設定
            var newPointListParent = (GameObject)EditorGUILayout.ObjectField(
                "  ポイントリストペアレント", 
                _viewModel.BoxSettings.PointListParent, 
                typeof(GameObject), 
                true);
            
            if (newPointListParent != _viewModel.BoxSettings.PointListParent)
            {
                _viewModel.UpdatePointListParent(newPointListParent);
            }

            // ボックスリストペアレント設定
            _viewModel.BoxSettings.BoxListParent = (GameObject)EditorGUILayout.ObjectField(
                "  ボックスリストペアレント", 
                _viewModel.BoxSettings.BoxListParent, 
                typeof(GameObject), 
                true);

            // ペアレント作成ボタン
            if (GUILayout.Button("ポイントとボックスの親を追加"))
            {
                CreateParentObjects();
            }

            // ポイントリスト表示
            DrawPointsList();

            // ボックス詳細設定
            DrawBoxSettings();

            EditorGUILayout.EndScrollView();
        }

        private void DrawPointsList()
        {
            _viewModel.UI.IsPointListExpanded = EditorGUILayout.Foldout(
                _viewModel.UI.IsPointListExpanded, 
                "ポイントリスト");
                
            if (_viewModel.UI.IsPointListExpanded && _reorderableList != null)
            {
                _reorderableList.DoLayoutList();
            }
        }

        private void DrawBoxSettings()
        {
            _viewModel.UI.IsLineSettingExpanded = EditorGUILayout.Foldout(
                _viewModel.UI.IsLineSettingExpanded, 
                "ボックスの詳細設定");
                
            if (_viewModel.UI.IsLineSettingExpanded)
            {
                // 閉じる設定
                var newIsClosed = EditorGUILayout.Toggle("始点と終点を閉じる", _viewModel.BoxSettings.IsClosed);
                if (newIsClosed != _viewModel.BoxSettings.IsClosed)
                {
                    _viewModel.UpdateBoxSettings(() => _viewModel.BoxSettings.IsClosed = newIsClosed);
                }

                GUILayout.Space(_viewModel.BoxSettings.SpacePixel);

                // 基準設定
                GUILayout.Label("基準");
                var newAbstractPosition = (AbstractPosition)GUILayout.SelectionGrid(
                    (int)_viewModel.BoxSettings.BoxAbstractPosition,
                    new string[] { "左揃え", "中心", "右揃え" }, 
                    3);

                if (newAbstractPosition != _viewModel.BoxSettings.BoxAbstractPosition)
                {
                    _viewModel.UpdateBoxSettings(() => _viewModel.BoxSettings.BoxAbstractPosition = newAbstractPosition);
                }

                GUILayout.Space(_viewModel.BoxSettings.SpacePixel);

                // スケール設定
                DrawScaleSettings();

                GUILayout.Space(_viewModel.BoxSettings.SpacePixel);

                // コーナー生成設定
                DrawCornerSettings();
            }

            // 生成ボタン
            if (GUILayout.Button("ボックス接続を生成"))
            {
                _viewModel.GenerateBoxConnections();
            }
        }

        private void DrawScaleSettings()
        {
            GUILayout.Label("ボックスのスケール");

            var newEdgeWidth = EditorGUILayout.Slider("  幅", _viewModel.BoxSettings.EdgeWidth, 0f, 2f);
            if (Mathf.Abs(newEdgeWidth - _viewModel.BoxSettings.EdgeWidth) > 0.001f)
            {
                _viewModel.UpdateBoxSettings(() => _viewModel.BoxSettings.EdgeWidth = newEdgeWidth);
            }

            var newBoxHeight = EditorGUILayout.Slider("  高さ", _viewModel.BoxSettings.BoxHeight, 0f, 10f);
            if (Mathf.Abs(newBoxHeight - _viewModel.BoxSettings.BoxHeight) > 0.001f)
            {
                _viewModel.UpdateBoxSettings(() => _viewModel.BoxSettings.BoxHeight = newBoxHeight);
            }
        }

        private void DrawCornerSettings()
        {
            var newGenerateCorner = EditorGUILayout.Toggle("角を生成", _viewModel.BoxSettings.IsGenerateCorner);
            if (newGenerateCorner != _viewModel.BoxSettings.IsGenerateCorner)
            {
                _viewModel.UpdateBoxSettings(() => _viewModel.BoxSettings.IsGenerateCorner = newGenerateCorner);
            }

            if (_viewModel.BoxSettings.IsGenerateCorner)
            {
                var newCornerWidth = EditorGUILayout.Slider("  幅", _viewModel.BoxSettings.CornerWidth, 0f, 10f);
                if (Mathf.Abs(newCornerWidth - _viewModel.BoxSettings.CornerWidth) > 0.001f)
                {
                    _viewModel.UpdateBoxSettings(() => _viewModel.BoxSettings.CornerWidth = newCornerWidth);
                }
            }
        }

        private void DrawColliderUtilityGUI()
        {
            GUILayout.Label("コライダーユーティリティ", EditorStyles.boldLabel);
            
            _viewModel.UI.UtilityScrollPos = EditorGUILayout.BeginScrollView(_viewModel.UI.UtilityScrollPos);

            // BoxCollider → Transform変換
            DrawBoxColliderToTransformUtility();

            GUILayout.Space(_viewModel.BoxSettings.SpacePixel);

            // Transform → BoxCollider変換
            DrawTransformToBoxColliderUtility();

            GUILayout.Space(_viewModel.BoxSettings.SpacePixel);

            // 回転分割ツール
            DrawRotationalFrequencyUtility();

            EditorGUILayout.EndScrollView();
        }

        private void DrawBoxColliderToTransformUtility()
        {
            _viewModel.UI.IsConvertBoxColliderToTransformExpanded = EditorGUILayout.Foldout(
                _viewModel.UI.IsConvertBoxColliderToTransformExpanded,
                "BoxCollider → Transform変換");

            if (_viewModel.UI.IsConvertBoxColliderToTransformExpanded)
            {
                if (GUILayout.Button("選択したBoxColliderをTransformに変換"))
                {
                    ConvertSelectedBoxCollidersToTransform();
                }
            }
        }

        private void DrawTransformToBoxColliderUtility()
        {
            _viewModel.UI.IsConvertTransformToBoxColliderExpanded = EditorGUILayout.Foldout(
                _viewModel.UI.IsConvertTransformToBoxColliderExpanded,
                "Transform → BoxCollider変換");

            if (_viewModel.UI.IsConvertTransformToBoxColliderExpanded)
            {
                if (GUILayout.Button("選択したTransformにBoxColliderを追加"))
                {
                    AddBoxCollidersToSelectedTransforms();
                }
            }
        }

        private void DrawRotationalFrequencyUtility()
        {
            _viewModel.UI.IsRotationalFrequencyExpanded = EditorGUILayout.Foldout(
                _viewModel.UI.IsRotationalFrequencyExpanded,
                "回転分割ツール");

            if (_viewModel.UI.IsRotationalFrequencyExpanded)
            {
                _viewModel.BoxSettings.RotationalFrequency = EditorGUILayout.Slider(
                    "回転角度", 
                    _viewModel.BoxSettings.RotationalFrequency, 
                    0f, 
                    360f);

                if (GUILayout.Button("回転分割を適用"))
                {
                    ApplyRotationalDivision();
                }
            }
        }

        private void CreateParentObjects()
        {
            if (_viewModel.BoxSettings.PointListParent == null)
            {
                _viewModel.BoxSettings.PointListParent = ColliderPlacementService.CreateDefaultParent("PointListParent");
            }

            if (_viewModel.BoxSettings.BoxListParent == null)
            {
                _viewModel.BoxSettings.BoxListParent = ColliderPlacementService.CreateDefaultParent("BoxListParent");
            }

            Repaint();
        }

        private void ConvertSelectedBoxCollidersToTransform()
        {
            var selectedObjects = Selection.gameObjects;
            int convertedCount = 0;

            foreach (var obj in selectedObjects)
            {
                var boxCollider = obj.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    Debug.LogError("Selected GameObject does not have a BoxCollider component.");
                    continue;
                }

                // MeshRendererとMeshFilterを追加（旧実装互換）
                if (obj.GetComponent<MeshRenderer>() == null)
                {
                    MeshRenderer meshRenderer = Undo.AddComponent<MeshRenderer>(obj);
                    meshRenderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                    meshRenderer.enabled = false;
                }

                if (obj.GetComponent<MeshFilter>() == null)
                {
                    MeshFilter meshFilter = Undo.AddComponent<MeshFilter>(obj);
                    meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                }

                Undo.RecordObjects(new UnityEngine.Object[] { obj.transform, boxCollider }, "Convert BoxCollider Transform");

                // 旧実装の変換ロジック
                obj.transform.position = obj.transform.TransformPoint(boxCollider.center);
                obj.transform.localScale = Vector3.Scale(obj.transform.localScale, boxCollider.size);

                boxCollider.center = Vector3.zero;
                boxCollider.size = Vector3.one;

                convertedCount++;
            }

            ShowNotification(new GUIContent($"{convertedCount}個のBoxColliderを変換しました"));
        }

        private void AddBoxCollidersToSelectedTransforms()
        {
            Transform[] transforms = Selection.transforms;
            int addedCount = 0;

            foreach (Transform transform in transforms)
            {
                // 既存のMeshFilterとMeshRendererを削除（旧実装互換）
                MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();
                if (meshFilter != null || meshRenderer != null)
                {
                    if (meshFilter != null) Undo.DestroyObjectImmediate(meshFilter);
                    if (meshRenderer != null) Undo.DestroyObjectImmediate(meshRenderer);
                }

                BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    boxCollider = Undo.AddComponent<BoxCollider>(transform.gameObject);
                }

                // 旧実装の変換ロジック
                boxCollider.size = new Vector3(
                    Mathf.Abs(transform.localScale.x), 
                    Mathf.Abs(transform.localScale.y), 
                    Mathf.Abs(transform.localScale.z)
                );

                Vector3 center = Vector3.zero;
                Renderer renderer = transform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    center = renderer.bounds.center - transform.position;
                }
                boxCollider.center = center;

                transform.localScale = Vector3.one;
                addedCount++;
            }

            ShowNotification(new GUIContent($"{addedCount}個のBoxColliderを追加しました"));
        }

        private void ApplyRotationalDivision()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                ShowNotification(new GUIContent("オブジェクトを選択してください"));
                return;
            }

            Undo.SetCurrentGroupName("Rotational Division");
            int group = Undo.GetCurrentGroup();

            foreach (var obj in selectedObjects)
            {
                ColliderPlacementService.RotateDuplicate(obj, _viewModel.BoxSettings.RotationalFrequency);
            }

            Undo.CollapseUndoOperations(group);

            ShowNotification(new GUIContent($"{selectedObjects.Length}個のオブジェクトを回転複製しました"));
        }

        private void ShowError(string message)
        {
            EditorUtility.DisplayDialog("エラー", message, "OK");
        }

        /// <summary>
        /// オブジェクトの位置変更を監視（旧実装互換）
        /// </summary>
        private void CheckObjectPositions()
        {
            if (_viewModel == null || !_viewModel.UI.IsAutoUpdate) return;
            
            bool hasChanged = false;
            
            // ポイントオブジェクトの位置変更をチェック
            foreach (var point in _viewModel.Data.Points)
            {
                if (point.GameObject != null)
                {
                    // ここでは簡易的な実装。実際の旧版では前回位置との比較を行っていた
                    hasChanged = true;
                    break;
                }
            }
            
            if (hasChanged && (_viewModel.Data.Edges?.Count > 0 || _viewModel.Data.Corners?.Count > 0))
            {
                _viewModel.UpdateExistingConnections();
            }
        }
    }
}