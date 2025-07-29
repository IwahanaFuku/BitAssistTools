using System;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Transform;
using UERandom = UnityEngine.Random;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// ランダム化のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class RandomizationService
    {
        /// <summary>
        /// ランダムポジションを実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteRandomPosition(float radius, AxisFlags axisFlags)
        {
            try
            {
                Undo.SetCurrentGroupName("Random Position");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedTransforms = Selection.transforms;
                Undo.RecordObjects(selectedTransforms, "Random Position Objects");

                foreach (var transform in selectedTransforms)
                {
                    var randomOffset = UERandom.insideUnitSphere * radius;
                    
                    // 軸フラグに応じて制限
                    randomOffset = new Vector3(
                        axisFlags.X ? randomOffset.x : 0,
                        axisFlags.Y ? randomOffset.y : 0,
                        axisFlags.Z ? randomOffset.z : 0
                    );

                    transform.position += randomOffset;
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ランダムポジション中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ランダム回転を実行（DDDベース、Undo対応）
        /// 元実装通り: 軸別個別回転（transform.Rotate使用）
        /// </summary>
        public static void ExecuteRandomRotation(float snapDegrees, AxisFlags axisFlags, AnimationCurve limitCurve = null)
        {
            try
            {
                Undo.SetCurrentGroupName("Random Rotation");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedTransforms = Selection.transforms;
                Undo.RecordObjects(selectedTransforms, "Random Rotation Objects");

                foreach (var transform in selectedTransforms)
                {
                    // 元実装通り: 軸別個別回転
                    if (axisFlags.X)
                    {
                        var xRotation = GenerateRandomAngle(snapDegrees, limitCurve);
                        transform.Rotate(Vector3.right, xRotation, Space.World);
                    }

                    if (axisFlags.Y)
                    {
                        var yRotation = GenerateRandomAngle(snapDegrees, limitCurve);
                        transform.Rotate(Vector3.up, yRotation, Space.World);
                    }

                    if (axisFlags.Z)
                    {
                        var zRotation = GenerateRandomAngle(snapDegrees, limitCurve);
                        transform.Rotate(Vector3.forward, zRotation, Space.World);
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ランダム回転中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ランダムスケールを実行（DDDベース、Undo対応）
        /// 元実装通り: 通常モード = uniform scale
        /// </summary>
        public static void ExecuteRandomScale(float ratio, float minimum, float maximum)
        {
            try
            {
                Undo.SetCurrentGroupName("Random Scale");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedTransforms = Selection.transforms;
                Undo.RecordObjects(selectedTransforms, "Random Scale Objects");

                foreach (var transform in selectedTransforms)
                {
                    // 元実装通り: newscale * Random.Range(minimum, maximum)
                    var randomScaleValue = UERandom.Range(minimum, maximum);
                    var newScale = Vector3.one * randomScaleValue;
                    transform.localScale = newScale;
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ランダムスケール中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 詳細ランダムスケールを実行（DDDベース、Undo対応）
        /// 元実装通り: 詳細モード = 軸別scale
        /// </summary>
        public static void ExecuteRandomDetailScale(float ratio, Vector3 minScale, Vector3 maxScale)
        {
            try
            {
                Undo.SetCurrentGroupName("Random Detail Scale");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedTransforms = Selection.transforms;
                Undo.RecordObjects(selectedTransforms, "Random Detail Scale Objects");

                foreach (var transform in selectedTransforms)
                {
                    // 元実装通り: scale.x = scaleRatio * Random.Range(Mathf.Min(minScale.x, maxScale.x), Mathf.Max(minScale.x, maxScale.x))
                    var newScale = new Vector3(
                        ratio * UERandom.Range(Mathf.Min(minScale.x, maxScale.x), Mathf.Max(minScale.x, maxScale.x)),
                        ratio * UERandom.Range(Mathf.Min(minScale.y, maxScale.y), Mathf.Max(minScale.y, maxScale.y)),
                        ratio * UERandom.Range(Mathf.Min(minScale.z, maxScale.z), Mathf.Max(minScale.z, maxScale.z))
                    );

                    transform.localScale = newScale;
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"詳細ランダムスケール中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// パディングを実行（DDDベース、Undo対応）
        /// 基準点が指定された場合のみ実行される
        /// </summary>
        public static void ExecutePadding(float space, Transform referencePoint, AxisFlags axisFlags)
        {
            try
            {
                var selectedTransforms = Selection.transforms;
                if (selectedTransforms.Length < 1) return;

                // 基準点が指定されていない場合は処理しない（想定される元動作）
                if (referencePoint == null)
                {
                    Debug.LogWarning("パディング: 基準点が指定されていません。基準点を設定してからパディングを実行してください。");
                    return;
                }

                Undo.SetCurrentGroupName("Padding Objects");
                int undoGroup = Undo.GetCurrentGroup();
                Undo.RecordObjects(selectedTransforms, "Padding Objects");

                var reference = referencePoint.position;
                
                foreach (var transform in selectedTransforms)
                {
                    // 基準点からの方向ベクトルを計算
                    var direction = transform.position - reference;
                    
                    // 方向ベクトルの長さを基準として、spaceの倍率で距離を調整
                    var currentDistance = direction.magnitude;
                    if (currentDistance > 0.001f) // 極小値チェック
                    {
                        var normalizedDirection = direction.normalized;
                        var newDistance = currentDistance + space;
                        var paddingOffset = normalizedDirection * newDistance;

                        // 軸フラグに応じてパディング方向を制限
                        var finalOffset = new Vector3(
                            axisFlags.X ? paddingOffset.x : direction.x,
                            axisFlags.Y ? paddingOffset.y : direction.y,
                            axisFlags.Z ? paddingOffset.z : direction.z
                        );

                        transform.position = reference + finalOffset;
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"パディング中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        // プライベートヘルパーメソッド
        private static float GenerateRandomAngle(float snapDegrees, AnimationCurve limitCurve)
        {
            var randomAngle = UERandom.Range(0f, 360f);
            
            if (limitCurve != null)
            {
                var curveValue = limitCurve.Evaluate(UERandom.Range(0f, 1f));
                randomAngle = Mathf.Lerp(-180f, 180f, curveValue / 360f + 0.5f);
            }
            
            return SnapToGrid(randomAngle, snapDegrees);
        }

        private static float SnapToGrid(float value, float snapSize)
        {
            if (snapSize <= 0) return value;
            return Mathf.Round(value / snapSize) * snapSize;
        }
    }
}