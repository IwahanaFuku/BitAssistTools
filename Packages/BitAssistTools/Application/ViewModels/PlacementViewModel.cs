using System;
using UnityEngine;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Application.Services;

namespace BitAssistTools.Editor.Placement
{
    /// <summary>
    /// Placement機能のViewModel
    /// UI状態とビジネスロジックを分離
    /// </summary>
    public class PlacementViewModel
    {
        // UI展開状態管理
        public class UIExpansionState
        {
            public bool SpecialDuplicate { get; set; } = false;
            public bool Rounding { get; set; } = false;
            public bool ObjectSwapper { get; set; } = false;
            public bool SpecialTransformChange { get; set; } = false;
            public bool Distribute { get; set; } = false;
            public bool Arranger { get; set; } = false;
            public bool Padding { get; set; } = false;
            public bool RandomPosition { get; set; } = false;
            public bool RandomRotation { get; set; } = false;
            public bool RandomScale { get; set; } = false;
            
            // 詳細設定展開状態
            public bool RoundingDetail { get; set; } = false;
            public bool PaddingReferencePoint { get; set; } = false;
            public bool RandomRotationDetail { get; set; } = false;
            public bool RandomScaleDetail { get; set; } = false;
        }

        // 特殊複製設定
        public class DuplicationSettings
        {
            public int CopyCount { get; set; } = 5;
            public CoordinateSystem Coordinate { get; set; } = CoordinateSystem.Global;
            public Vector3 Position { get; set; } = Vector3.zero;
            public Vector3 Rotation { get; set; } = Vector3.zero;
            public Vector3 Scale { get; set; } = Vector3.one;
        }

        // 端数丸め設定
        public class RoundingSettings
        {
            public int Multiplier { get; set; } = 2;
            public TransformFlags TransformFlags { get; set; } = TransformFlags.All;
        }

        // オブジェクト入れ替え設定
        public class ObjectSwapperSettings
        {
            public GameObject Prefab { get; set; } = null;
        }

        // トランスフォーム変更設定
        public class TransformChangeSettings
        {
            public CoordinateSystem Coordinate { get; set; } = CoordinateSystem.Global;
            public Vector3 Position { get; set; } = Vector3.zero;
            public Vector3 Rotation { get; set; } = Vector3.zero;
            public Vector3 Scale { get; set; } = Vector3.zero;
        }

        // 分布設定
        public class DistributeSettings
        {
            public Axis Axis { get; set; } = Axis.X;
        }

        // 整列設定
        public class ArrangerSettings
        {
            public Axis Axis { get; set; } = Axis.X;
            public AlignmentType AlignType { get; set; } = AlignmentType.Center;
        }

        // パディング設定
        public class PaddingSettings
        {
            public float Space { get; set; } = 1f;
            public Transform ReferencePoint { get; set; } = null;
            public AxisFlags AxisFlags { get; set; } = AxisFlags.All;
        }

        // ランダムポジション設定
        public class RandomPositionSettings
        {
            public float Radius { get; set; } = 1f;
            public AxisFlags Flags { get; set; } = AxisFlags.All;
        }

        // ランダム回転設定
        public class RandomRotationSettings
        {
            public float Amount { get; set; } = 1f;
            public AxisFlags Flags { get; set; } = AxisFlags.YOnly;
            public AnimationCurve Curve { get; set; } = AnimationCurve.Linear(0, -180, 1, 180);
        }

        // ランダムスケール設定
        public class RandomScaleSettings
        {
            public float Ratio { get; set; } = 1f;
            public float Minimum { get; set; } = 0.5f;
            public float Maximum { get; set; } = 1.5f;
            public Vector3 MinScale { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);
            public Vector3 MaxScale { get; set; } = new Vector3(1.5f, 1.5f, 1.5f);
        }

        // イベント定義
        public event Action OnUIStateChanged;
        public event Action<string> OnError;

        // プロパティ
        public UIExpansionState UI { get; private set; }
        public DuplicationSettings Duplication { get; private set; }
        public RoundingSettings Rounding { get; private set; }
        public ObjectSwapperSettings ObjectSwapper { get; private set; }
        public TransformChangeSettings TransformChange { get; private set; }
        public DistributeSettings Distribute { get; private set; }
        public ArrangerSettings Arranger { get; private set; }
        public PaddingSettings Padding { get; private set; }
        public RandomPositionSettings RandomPosition { get; private set; }
        public RandomRotationSettings RandomRotation { get; private set; }
        public RandomScaleSettings RandomScale { get; private set; }

        // その他の状態
        public Vector2 ScrollPosition { get; set; }
        public int SpacePixel { get; set; } = 8;

        public PlacementViewModel()
        {
            UI = new UIExpansionState();
            Duplication = new DuplicationSettings();
            Rounding = new RoundingSettings();
            ObjectSwapper = new ObjectSwapperSettings();
            TransformChange = new TransformChangeSettings();
            Distribute = new DistributeSettings();
            Arranger = new ArrangerSettings();
            Padding = new PaddingSettings();
            RandomPosition = new RandomPositionSettings();
            RandomRotation = new RandomRotationSettings();
            RandomScale = new RandomScaleSettings();
        }

        /// <summary>
        /// 特殊複製を実行
        /// </summary>
        public void ExecuteSpecialDuplicate()
        {
            try
            {
                var transformData = new TransformData(
                    Duplication.Position,
                    Quaternion.Euler(Duplication.Rotation),
                    Duplication.Scale
                );

                ObjectPlacementService.ExecuteSpecialDuplicate(
                    Duplication.Coordinate,
                    transformData,
                    Duplication.CopyCount
                );
            }
            catch (Exception ex)
            {
                NotifyError($"特殊複製中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 端数丸めを実行
        /// </summary>
        public void ExecuteRounding()
        {
            try
            {
                TransformCalculationService.ExecuteRounding(
                    Rounding.Multiplier,
                    Rounding.TransformFlags
                );
            }
            catch (Exception ex)
            {
                NotifyError($"端数丸め中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// オブジェクト入れ替えを実行
        /// </summary>
        public void ExecuteObjectSwapper()
        {
            try
            {
                ObjectPlacementService.ExecuteObjectSwapper(ObjectSwapper.Prefab);
            }
            catch (Exception ex)
            {
                NotifyError($"オブジェクト入れ替え中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// トランスフォーム変更を実行
        /// </summary>
        public void ExecuteTransformChange()
        {
            try
            {
                var transformData = new TransformData(
                    TransformChange.Position,
                    Quaternion.Euler(TransformChange.Rotation),
                    TransformChange.Scale
                );

                TransformCalculationService.ExecuteTransformChange(
                    TransformChange.Coordinate,
                    transformData
                );
            }
            catch (Exception ex)
            {
                NotifyError($"トランスフォーム変更中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 分布を実行
        /// </summary>
        public void ExecuteDistribute()
        {
            try
            {
                TransformCalculationService.ExecuteDistribute(Distribute.Axis);
            }
            catch (Exception ex)
            {
                NotifyError($"分布中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 整列を実行
        /// </summary>
        public void ExecuteArranger()
        {
            try
            {
                TransformCalculationService.ExecuteAlignment(
                    Arranger.Axis,
                    Arranger.AlignType
                );
            }
            catch (Exception ex)
            {
                NotifyError($"整列中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// パディングを実行
        /// </summary>
        public void ExecutePadding()
        {
            try
            {
                RandomizationService.ExecutePadding(
                    Padding.Space,
                    Padding.ReferencePoint,
                    Padding.AxisFlags
                );
            }
            catch (Exception ex)
            {
                NotifyError($"パディング中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ランダムポジションを実行
        /// </summary>
        public void ExecuteRandomPosition()
        {
            try
            {
                RandomizationService.ExecuteRandomPosition(
                    RandomPosition.Radius,
                    RandomPosition.Flags
                );
            }
            catch (Exception ex)
            {
                NotifyError($"ランダムポジション中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ランダム回転を実行
        /// </summary>
        public void ExecuteRandomRotation()
        {
            try
            {
                var curve = UI.RandomRotationDetail ? RandomRotation.Curve : null;
                RandomizationService.ExecuteRandomRotation(
                    RandomRotation.Amount,
                    RandomRotation.Flags,
                    curve
                );
            }
            catch (Exception ex)
            {
                NotifyError($"ランダム回転中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ランダムスケールを実行
        /// </summary>
        public void ExecuteRandomScale()
        {
            try
            {
                if (UI.RandomScaleDetail)
                {
                    RandomizationService.ExecuteRandomDetailScale(
                        RandomScale.Ratio,
                        RandomScale.MinScale,
                        RandomScale.MaxScale
                    );
                }
                else
                {
                    RandomizationService.ExecuteRandomScale(
                        RandomScale.Ratio,
                        RandomScale.Minimum,
                        RandomScale.Maximum
                    );
                }
            }
            catch (Exception ex)
            {
                NotifyError($"ランダムスケール中にエラーが発生しました: {ex.Message}");
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

    }
}