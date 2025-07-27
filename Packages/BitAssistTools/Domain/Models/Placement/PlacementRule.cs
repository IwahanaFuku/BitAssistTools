using System;
using BitAssistTools.Domain.Models.Transform;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Placement
{
    /// <summary>
    /// 配置時の整列タイプを表現する値オブジェクト
    /// </summary>
    public enum AlignType
    {
        /// <summary>最大値に整列</summary>
        Maximum,
        /// <summary>中央に整列</summary>
        Center,
        /// <summary>最小値に整列</summary>
        Minimum
    }


    /// <summary>
    /// オブジェクト配置のルールを定義する値オブジェクト
    /// </summary>
    [Serializable]
    public readonly struct PlacementRule : IEquatable<PlacementRule>
    {
        public readonly AlignType AlignType;
        public readonly Axis TargetAxis;
        public readonly CoordinateReference CoordinateReference;
        public readonly bool PreserveOriginalValues;

        public PlacementRule(AlignType alignType, Axis targetAxis, CoordinateReference coordinateReference, bool preserveOriginalValues = false)
        {
            AlignType = alignType;
            TargetAxis = targetAxis;
            CoordinateReference = coordinateReference;
            PreserveOriginalValues = preserveOriginalValues;
        }

        /// <summary>
        /// デフォルトの配置ルール（中央・Y軸・グローバル座標）
        /// </summary>
        public static PlacementRule Default => new PlacementRule(AlignType.Center, Axis.Y, CoordinateReference.Global);

        /// <summary>
        /// 指定した複数のオブジェクトの境界を計算
        /// </summary>
        public static Bounds CalculateBounds(UnityEngine.Transform[] transforms, CoordinateReference coordinateReference)
        {
            if (transforms == null || transforms.Length == 0)
                return new Bounds();

            var bounds = new Bounds();
            bool initialized = false;

            foreach (var transform in transforms)
            {
                if (transform == null) continue;

                Vector3 position = coordinateReference == CoordinateReference.Global 
                    ? transform.position 
                    : transform.localPosition;

                if (!initialized)
                {
                    bounds = new Bounds(position, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(position);
                }
            }

            return bounds;
        }

        /// <summary>
        /// 配置ルールに基づいて目標位置を計算
        /// </summary>
        public Vector3 CalculateTargetPosition(Bounds bounds)
        {
            switch (AlignType)
            {
                case AlignType.Maximum:
                    return GetAxisValue(bounds.max);
                case AlignType.Center:
                    return GetAxisValue(bounds.center);
                case AlignType.Minimum:
                    return GetAxisValue(bounds.min);
                default:
                    return bounds.center;
            }
        }

        private Vector3 GetAxisValue(Vector3 source)
        {
            switch (TargetAxis)
            {
                case Axis.X:
                    return new Vector3(source.x, 0, 0);
                case Axis.Y:
                    return new Vector3(0, source.y, 0);
                case Axis.Z:
                    return new Vector3(0, 0, source.z);
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// 指定した軸の値のみを取得
        /// </summary>
        public float GetAxisValue(Vector3 vector, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return vector.x;
                case Axis.Y:
                    return vector.y;
                case Axis.Z:
                    return vector.z;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 指定した軸の値のみを設定
        /// </summary>
        public Vector3 SetAxisValue(Vector3 original, Axis axis, float value)
        {
            switch (axis)
            {
                case Axis.X:
                    return new Vector3(value, original.y, original.z);
                case Axis.Y:
                    return new Vector3(original.x, value, original.z);
                case Axis.Z:
                    return new Vector3(original.x, original.y, value);
                default:
                    return original;
            }
        }

        #region IEquatable<PlacementRule> Implementation

        public bool Equals(PlacementRule other)
        {
            return AlignType == other.AlignType &&
                   TargetAxis == other.TargetAxis &&
                   CoordinateReference == other.CoordinateReference &&
                   PreserveOriginalValues == other.PreserveOriginalValues;
        }

        public override bool Equals(object obj)
        {
            return obj is PlacementRule other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + AlignType.GetHashCode();
                hash = hash * 23 + TargetAxis.GetHashCode();
                hash = hash * 23 + CoordinateReference.GetHashCode();
                hash = hash * 23 + PreserveOriginalValues.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(PlacementRule left, PlacementRule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlacementRule left, PlacementRule right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"PlacementRule(Align: {AlignType}, Axis: {TargetAxis}, Coordinate: {CoordinateReference}, Preserve: {PreserveOriginalValues})";
        }
    }
}