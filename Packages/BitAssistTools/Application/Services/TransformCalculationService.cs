using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Transform;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// Transform計算・変換のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class TransformCalculationService
    {
        /// <summary>
        /// 端数丸めを実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteRounding(int decimalPlaces, TransformFlags transformFlags)
        {
            try
            {
                Undo.SetCurrentGroupName("Transform Rounding");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedTransforms = Selection.transforms;
                Undo.RecordObjects(selectedTransforms, "Round Transforms");

                foreach (var transform in selectedTransforms)
                {
                    var rounded = RoundTransformData(
                        TransformData.FromTransform(transform),
                        decimalPlaces,
                        transformFlags
                    );
                    
                    rounded.ApplyToTransform(transform);
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"端数丸め中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Transform変更を実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteTransformChange(CoordinateSystem coordinate, TransformData change)
        {
            try
            {
                Undo.SetCurrentGroupName("Transform Change");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedTransforms = Selection.transforms;
                Undo.RecordObjects(selectedTransforms, "Change Transforms");

                foreach (var transform in selectedTransforms)
                {
                    var current = TransformData.FromTransform(transform);
                    var newData = coordinate.IsGlobal 
                        ? ApplyGlobalChange(current, change)
                        : ApplyLocalChange(transform, current, change);
                    
                    newData.ApplyToTransform(transform);
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Transform変更中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// オブジェクト分布を実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteDistribute(Axis axis)
        {
            try
            {
                var selectedTransforms = Selection.transforms;
                if (selectedTransforms.Length < 2) return;

                Undo.SetCurrentGroupName("Distribute Objects");
                int undoGroup = Undo.GetCurrentGroup();
                Undo.RecordObjects(selectedTransforms, "Distribute Objects");

                // 軸に応じてソート
                var sorted = selectedTransforms.OrderBy(t => GetAxisValue(t.position, axis)).ToArray();
                
                if (sorted.Length < 2) return;

                var firstPos = GetAxisValue(sorted[0].position, axis);
                var lastPos = GetAxisValue(sorted[sorted.Length - 1].position, axis);
                var totalDistance = lastPos - firstPos;

                // 等間隔で配置
                for (int i = 1; i < sorted.Length - 1; i++)
                {
                    var t = sorted[i].position;
                    var newAxisValue = firstPos + (totalDistance * i / (sorted.Length - 1));
                    
                    var newPosition = SetAxisValue(t, axis, newAxisValue);
                    sorted[i].position = newPosition;
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"分布中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// オブジェクト整列を実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteAlignment(Axis axis, AlignmentType alignType)
        {
            try
            {
                var selectedTransforms = Selection.transforms;
                if (selectedTransforms.Length < 2) return;

                Undo.SetCurrentGroupName("Align Objects");
                int undoGroup = Undo.GetCurrentGroup();
                Undo.RecordObjects(selectedTransforms, "Align Objects");

                var targetValue = CalculateAlignmentValue(selectedTransforms, axis, alignType);

                foreach (var transform in selectedTransforms)
                {
                    var newPosition = SetAxisValue(transform.position, axis, targetValue);
                    transform.position = newPosition;
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"整列中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        // プライベートヘルパーメソッド
        private static TransformData RoundTransformData(TransformData data, int decimalPlaces, TransformFlags transformFlags)
        {
            var position = data.Position;
            var rotation = data.Rotation;
            var scale = data.LocalScale;

            if (transformFlags.HasFlag(TransformFlags.Position))
            {
                position = RoundVector3(position, decimalPlaces);
            }

            if (transformFlags.HasFlag(TransformFlags.Rotation))
            {
                var euler = rotation.eulerAngles;
                euler = RoundVector3(euler, decimalPlaces);
                rotation = Quaternion.Euler(euler);
            }

            if (transformFlags.HasFlag(TransformFlags.Scale))
            {
                scale = RoundVector3(scale, decimalPlaces);
            }

            return new TransformData(position, rotation, scale);
        }

        private static Vector3 RoundVector3(Vector3 vector, int decimalPlaces)
        {
            var multiplier = Mathf.Pow(10, decimalPlaces);
            
            return new Vector3(
                Mathf.Round(vector.x * multiplier) / multiplier,
                Mathf.Round(vector.y * multiplier) / multiplier,
                Mathf.Round(vector.z * multiplier) / multiplier
            );
        }

        private static TransformData ApplyGlobalChange(TransformData current, TransformData change)
        {
            // 元実装通り軸別回転適用
            var newRotation = current.Rotation;
            var changeEuler = change.Rotation.eulerAngles;
            newRotation = Quaternion.AngleAxis(changeEuler.x, Vector3.right) * newRotation;
            newRotation = Quaternion.AngleAxis(changeEuler.y, Vector3.up) * newRotation;
            newRotation = Quaternion.AngleAxis(changeEuler.z, Vector3.forward) * newRotation;
            
            return new TransformData(
                current.Position + change.Position,
                newRotation,
                current.LocalScale + change.LocalScale  // 元実装は加算
            );
        }

        private static TransformData ApplyLocalChange(Transform transform, TransformData current, TransformData change)
        {
            return new TransformData(
                current.Position + transform.TransformDirection(change.Position),
                current.Rotation * change.Rotation,  // 元実装通り
                current.LocalScale + change.LocalScale  // 元実装は加算
            );
        }

        private static float GetAxisValue(Vector3 position, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return position.x;
                case Axis.Y:
                    return position.y;
                case Axis.Z:
                    return position.z;
                default:
                    throw new ArgumentException($"Invalid axis: {axis}");
            }
        }

        private static Vector3 SetAxisValue(Vector3 position, Axis axis, float value)
        {
            switch (axis)
            {
                case Axis.X:
                    return new Vector3(value, position.y, position.z);
                case Axis.Y:
                    return new Vector3(position.x, value, position.z);
                case Axis.Z:
                    return new Vector3(position.x, position.y, value);
                default:
                    throw new ArgumentException($"Invalid axis: {axis}");
            }
        }

        private static float CalculateAlignmentValue(Transform[] transforms, Axis axis, AlignmentType alignType)
        {
            var values = transforms.Select(t => GetAxisValue(t.position, axis)).ToArray();
            
            switch (alignType)
            {
                case AlignmentType.Minimum:
                    return values.Min();
                case AlignmentType.Maximum:
                    return values.Max();
                case AlignmentType.Center:
                    return values.Average();
                default:
                    throw new ArgumentException($"Invalid alignment type: {alignType}");
            }
        }
    }

}