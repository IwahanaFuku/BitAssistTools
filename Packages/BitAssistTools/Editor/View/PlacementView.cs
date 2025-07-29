using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Application.ViewModels;

namespace BitAssistTools.Editor.Placement
{
    /// <summary>
    /// Placement機能のView
    /// ViewModelを使用してUI状態とビジネスロジックを分離
    /// </summary>
    public class PlacementView : EditorWindow
    {
        private PlacementViewModel _viewModel;

        [MenuItem("Bit Assist Tools/配置支援ツール", priority = 11)]
        public static void ShowWindow()
        {
            var window = GetWindow<PlacementView>("配置支援ツール");
            window.Initialize();
        }

        private void Initialize()
        {
            _viewModel = new PlacementViewModel();
            
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
            }
        }

        private void OnGUI()
        {
            if (_viewModel == null)
            {
                Initialize();
            }

            _viewModel.ScrollPosition = EditorGUILayout.BeginScrollView(_viewModel.ScrollPosition);

            DrawSpecialDuplicateGUI();
            DrawRoundingGUI();
            DrawObjectSwapperGUI();
            DrawSpecialTransformChangeGUI();
            DrawDistributeGUI();
            DrawArrangerGUI();
            DrawPaddingGUI();
            DrawRandomPositionGUI();
            DrawRandomRotationGUI();
            DrawRandomScaleGUI();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawSpecialDuplicateGUI()
        {
            _viewModel.UI.SpecialDuplicate = EditorGUILayout.Foldout(_viewModel.UI.SpecialDuplicate, "特殊な複製");
            if (_viewModel.UI.SpecialDuplicate)
            {
                var selectedIndex = _viewModel.Duplication.Coordinate.IsGlobal ? 0 : 1;
                selectedIndex = GUILayout.SelectionGrid(selectedIndex, new string[] {"Global", "Local"}, 2);
                _viewModel.Duplication.Coordinate = selectedIndex == 0 ? CoordinateSystem.Global : CoordinateSystem.Local;

                _viewModel.Duplication.Position = EditorGUILayout.Vector3Field("  Position", _viewModel.Duplication.Position);
                
                var rotationEuler = EditorGUILayout.Vector3Field("  Rotation", _viewModel.Duplication.Rotation);
                _viewModel.Duplication.Rotation = new Vector3(
                    Mathf.Round(rotationEuler.x * 1000f) / 1000f,
                    Mathf.Round(rotationEuler.y * 1000f) / 1000f,
                    Mathf.Round(rotationEuler.z * 1000f) / 1000f
                );

                _viewModel.Duplication.Scale = EditorGUILayout.Vector3Field("  Scale", _viewModel.Duplication.Scale);
                _viewModel.Duplication.CopyCount = EditorGUILayout.IntSlider("コピー数", _viewModel.Duplication.CopyCount, 1, 100);

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("複製"))
                {
                    _viewModel.ExecuteSpecialDuplicate();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawRoundingGUI()
        {
            _viewModel.UI.Rounding = EditorGUILayout.Foldout(_viewModel.UI.Rounding, "端数を丸める");
            if (_viewModel.UI.Rounding)
            {
                _viewModel.Rounding.Multiplier = (int)EditorGUILayout.Slider("小数点の位", _viewModel.Rounding.Multiplier, 0f, 6f);

                var currentFlags = _viewModel.Rounding.TransformFlags;
                var newPosition = EditorGUILayout.Toggle("  Position", currentFlags.HasFlag(TransformFlags.Position));
                var newRotation = EditorGUILayout.Toggle("  Rotation", currentFlags.HasFlag(TransformFlags.Rotation));
                var newScale = EditorGUILayout.Toggle("  Scale", currentFlags.HasFlag(TransformFlags.Scale));

                // TransformFlagsを再構成
                var newFlags = TransformFlags.None;
                if (newPosition) newFlags |= TransformFlags.Position;
                if (newRotation) newFlags |= TransformFlags.Rotation;
                if (newScale) newFlags |= TransformFlags.Scale;
                _viewModel.Rounding.TransformFlags = newFlags;

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("丸める"))
                {
                    _viewModel.ExecuteRounding();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawObjectSwapperGUI()
        {
            _viewModel.UI.ObjectSwapper = EditorGUILayout.Foldout(_viewModel.UI.ObjectSwapper, "選択したオブジェクトを入れ替える");
            if (_viewModel.UI.ObjectSwapper)
            {
                _viewModel.ObjectSwapper.Prefab = (GameObject)EditorGUILayout.ObjectField(
                    "オブジェクト", 
                    _viewModel.ObjectSwapper.Prefab, 
                    typeof(GameObject), 
                    false);

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("入れ替え"))
                {
                    _viewModel.ExecuteObjectSwapper();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawSpecialTransformChangeGUI()
        {
            _viewModel.UI.SpecialTransformChange = EditorGUILayout.Foldout(_viewModel.UI.SpecialTransformChange, "現在の値をもとにトランスフォームの変更");
            if (_viewModel.UI.SpecialTransformChange)
            {
                var selectedIndex2 = _viewModel.TransformChange.Coordinate.IsGlobal ? 0 : 1;
                selectedIndex2 = GUILayout.SelectionGrid(selectedIndex2, new string[] {"Global", "Local"}, 2);
                _viewModel.TransformChange.Coordinate = selectedIndex2 == 0 ? CoordinateSystem.Global : CoordinateSystem.Local;
                
                _viewModel.TransformChange.Position = EditorGUILayout.Vector3Field("  Position", _viewModel.TransformChange.Position);
                
                var rotationEuler = EditorGUILayout.Vector3Field("  Rotation", _viewModel.TransformChange.Rotation);
                _viewModel.TransformChange.Rotation = new Vector3(
                    Mathf.Round(rotationEuler.x * 1000f) / 1000f,
                    Mathf.Round(rotationEuler.y * 1000f) / 1000f,
                    Mathf.Round(rotationEuler.z * 1000f) / 1000f
                );

                _viewModel.TransformChange.Scale = EditorGUILayout.Vector3Field("  Scale", _viewModel.TransformChange.Scale);

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("変更"))
                {
                    _viewModel.ExecuteTransformChange();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawDistributeGUI()
        {
            _viewModel.UI.Distribute = EditorGUILayout.Foldout(_viewModel.UI.Distribute, "分布");
            if (_viewModel.UI.Distribute)
            {
                GUILayout.Label("軸");
                _viewModel.Distribute.Axis = (Axis)GUILayout.SelectionGrid(
                    (int)_viewModel.Distribute.Axis, 
                    new string[] { "X軸", "Y軸", "Z軸" }, 3);

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("分布"))
                {
                    _viewModel.ExecuteDistribute();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawArrangerGUI()
        {
            _viewModel.UI.Arranger = EditorGUILayout.Foldout(_viewModel.UI.Arranger, "整列");
            if (_viewModel.UI.Arranger)
            {
                GUILayout.Label("軸");
                _viewModel.Arranger.Axis = (Axis)GUILayout.SelectionGrid(
                    (int)_viewModel.Arranger.Axis, 
                    new string[] { "X軸", "Y軸", "Z軸" }, 3);

                GUILayout.Label("基準値");
                _viewModel.Arranger.AlignType = (AlignmentType)GUILayout.SelectionGrid(
                    (int)_viewModel.Arranger.AlignType, 
                    new string[] { "最小", "中央", "最大" }, 3);

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("整列"))
                {
                    _viewModel.ExecuteArranger();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawPaddingGUI()
        {
            _viewModel.UI.Padding = EditorGUILayout.Foldout(_viewModel.UI.Padding, "パディング");
            if (_viewModel.UI.Padding)
            {
                _viewModel.Padding.Space = EditorGUILayout.Slider("パディング間隔", _viewModel.Padding.Space, 0f, 2f);

                GUILayout.Label("軸");
                var currentFlags = _viewModel.Padding.AxisFlags;
                var newX = EditorGUILayout.Toggle("  X軸", currentFlags.X);
                var newY = EditorGUILayout.Toggle("  Y軸", currentFlags.Y);
                var newZ = EditorGUILayout.Toggle("  Z軸", currentFlags.Z);
                _viewModel.Padding.AxisFlags = new AxisFlags(newX, newY, newZ);

                _viewModel.UI.PaddingReferencePoint = EditorGUILayout.Toggle("基準点を追加", _viewModel.UI.PaddingReferencePoint);
                if (_viewModel.UI.PaddingReferencePoint)
                {
                    _viewModel.Padding.ReferencePoint = (Transform)EditorGUILayout.ObjectField(
                        "基準点", 
                        _viewModel.Padding.ReferencePoint, 
                        typeof(Transform), 
                        true);
                }

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("パディング"))
                {
                    _viewModel.ExecutePadding();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawRandomPositionGUI()
        {
            _viewModel.UI.RandomPosition = EditorGUILayout.Foldout(_viewModel.UI.RandomPosition, "ランダムポジション");
            if (_viewModel.UI.RandomPosition)
            {
                _viewModel.RandomPosition.Radius = EditorGUILayout.Slider("半径:", _viewModel.RandomPosition.Radius, 0, 10);

                GUILayout.Label("軸");
                var currentPosFlags = _viewModel.RandomPosition.Flags;
                var newPosX = EditorGUILayout.Toggle("  X軸", currentPosFlags.X);
                var newPosY = EditorGUILayout.Toggle("  Y軸", currentPosFlags.Y);
                var newPosZ = EditorGUILayout.Toggle("  Z軸", currentPosFlags.Z);
                _viewModel.RandomPosition.Flags = new AxisFlags(newPosX, newPosY, newPosZ);

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("移動"))
                {
                    _viewModel.ExecuteRandomPosition();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawRandomRotationGUI()
        {
            _viewModel.UI.RandomRotation = EditorGUILayout.Foldout(_viewModel.UI.RandomRotation, "ランダム回転");
            if (_viewModel.UI.RandomRotation)
            {
                _viewModel.RandomRotation.Amount = EditorGUILayout.Slider("スナップ度数:", _viewModel.RandomRotation.Amount, 1, 360);
                _viewModel.UI.RandomRotationDetail = EditorGUILayout.Toggle("詳細", _viewModel.UI.RandomRotationDetail);
                
                if (_viewModel.UI.RandomRotationDetail)
                {
                    _viewModel.RandomRotation.Curve = EditorGUILayout.CurveField("  制限度数", _viewModel.RandomRotation.Curve);
                }

                GUILayout.Label("軸");
                var currentRotFlags = _viewModel.RandomRotation.Flags;
                var newRotX = EditorGUILayout.Toggle("  X軸", currentRotFlags.X);
                var newRotY = EditorGUILayout.Toggle("  Y軸", currentRotFlags.Y);
                var newRotZ = EditorGUILayout.Toggle("  Z軸", currentRotFlags.Z);
                _viewModel.RandomRotation.Flags = new AxisFlags(newRotX, newRotY, newRotZ);

                GUILayout.Space(_viewModel.SpacePixel);

                if (GUILayout.Button("回転"))
                {
                    _viewModel.ExecuteRandomRotation();
                }

                GUILayout.Space(_viewModel.SpacePixel);
            }
        }

        private void DrawRandomScaleGUI()
        {
            _viewModel.UI.RandomScale = EditorGUILayout.Foldout(_viewModel.UI.RandomScale, "ランダムスケール");
            if (_viewModel.UI.RandomScale)
            {
                _viewModel.RandomScale.Ratio = EditorGUILayout.FloatField("オフセット", _viewModel.RandomScale.Ratio);

                using (new EditorGUI.DisabledGroupScope(_viewModel.UI.RandomScaleDetail))
                {
                    _viewModel.RandomScale.Minimum = EditorGUILayout.FloatField("  最小", _viewModel.RandomScale.Minimum);
                    _viewModel.RandomScale.Maximum = EditorGUILayout.FloatField("  最大", _viewModel.RandomScale.Maximum);
                }

                _viewModel.UI.RandomScaleDetail = EditorGUILayout.Toggle("詳細", _viewModel.UI.RandomScaleDetail);

                if (!_viewModel.UI.RandomScaleDetail)
                {
                    GUILayout.Space(_viewModel.SpacePixel);

                    if (GUILayout.Button("スケール"))
                    {
                        _viewModel.ExecuteRandomScale();
                    }

                    GUILayout.Space(_viewModel.SpacePixel);
                }
                else
                {            
                    _viewModel.RandomScale.MinScale = EditorGUILayout.Vector3Field("  最小", _viewModel.RandomScale.MinScale);
                    _viewModel.RandomScale.MaxScale = EditorGUILayout.Vector3Field("  最大", _viewModel.RandomScale.MaxScale);

                    GUILayout.Space(_viewModel.SpacePixel);

                    if (GUILayout.Button("スケール"))
                    {
                        _viewModel.ExecuteRandomScale();
                    }

                    GUILayout.Space(_viewModel.SpacePixel);
                }
            }
        }

        private void ShowError(string message)
        {
            EditorUtility.DisplayDialog("エラー", message, "OK");
        }
    }
}