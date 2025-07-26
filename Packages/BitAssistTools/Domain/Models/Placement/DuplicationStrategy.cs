using System;
using BitAssistTools.Domain.Models.Transform;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Placement
{
    /// <summary>
    /// 複製戦略を定義する値オブジェクト
    /// </summary>
    [Serializable]
    public readonly struct DuplicationStrategy : IEquatable<DuplicationStrategy>
    {
        public readonly int CopyCount;
        public readonly TransformData DeltaTransform;
        public readonly CoordinateReference CoordinateReference;
        public readonly bool PreserveHierarchy;
        public readonly bool UseProgressiveScaling;

        public DuplicationStrategy(
            int copyCount, 
            TransformData deltaTransform, 
            CoordinateReference coordinateReference = CoordinateReference.Global,
            bool preserveHierarchy = true,
            bool useProgressiveScaling = false)
        {
            if (copyCount < 0)
                throw new ArgumentException("Copy count cannot be negative", nameof(copyCount));

            CopyCount = copyCount;
            DeltaTransform = deltaTransform;
            CoordinateReference = coordinateReference;
            PreserveHierarchy = preserveHierarchy;
            UseProgressiveScaling = useProgressiveScaling;
        }

        /// <summary>
        /// デフォルトの複製戦略（5個、Y軸に1単位ずつ配置）
        /// </summary>
        public static DuplicationStrategy Default => new DuplicationStrategy(
            5,
            new TransformData(Vector3.up, Quaternion.identity, Vector3.one)
        );

        /// <summary>
        /// 複製なしの戦略
        /// </summary>
        public static DuplicationStrategy None => new DuplicationStrategy(
            0,
            TransformData.Identity
        );

        /// <summary>
        /// 指定したインデックスでの累積Transform値を計算
        /// </summary>
        public TransformData CalculateCumulativeTransform(int index)
        {
            if (index < 0)
                throw new ArgumentException("Index cannot be negative", nameof(index));

            // プログレッシブスケーリングを使用する場合
            if (UseProgressiveScaling)
            {
                return new TransformData(
                    DeltaTransform.Position * index,
                    Quaternion.Slerp(Quaternion.identity, DeltaTransform.Rotation, index),
                    Vector3.Lerp(Vector3.one, DeltaTransform.LocalScale, index)
                );
            }

            // 通常の累積計算
            var rotationMultiplier = Quaternion.identity;
            for (int i = 0; i < index; i++)
            {
                rotationMultiplier *= DeltaTransform.Rotation;
            }

            return new TransformData(
                DeltaTransform.Position * index,
                rotationMultiplier,
                Vector3Scale(Vector3.one, DeltaTransform.LocalScale, index)
            );
        }

        /// <summary>
        /// 複製が有効かどうかを判定
        /// </summary>
        public bool IsValid => CopyCount > 0;

        /// <summary>
        /// 座標参照を変更した新しい戦略を返す
        /// </summary>
        public DuplicationStrategy WithCoordinateReference(CoordinateReference newCoordinateReference)
        {
            return new DuplicationStrategy(
                CopyCount,
                DeltaTransform,
                newCoordinateReference,
                PreserveHierarchy,
                UseProgressiveScaling
            );
        }

        /// <summary>
        /// コピー数を変更した新しい戦略を返す
        /// </summary>
        public DuplicationStrategy WithCopyCount(int newCopyCount)
        {
            return new DuplicationStrategy(
                newCopyCount,
                DeltaTransform,
                CoordinateReference,
                PreserveHierarchy,
                UseProgressiveScaling
            );
        }

        /// <summary>
        /// デルタTransformを変更した新しい戦略を返す
        /// </summary>
        public DuplicationStrategy WithDeltaTransform(TransformData newDeltaTransform)
        {
            return new DuplicationStrategy(
                CopyCount,
                newDeltaTransform,
                CoordinateReference,
                PreserveHierarchy,
                UseProgressiveScaling
            );
        }

        /// <summary>
        /// Vector3のべき乗計算（スケールの累積用）
        /// </summary>
        private static Vector3 Vector3Scale(Vector3 baseValue, Vector3 multiplier, int times)
        {
            var result = baseValue;
            for (int i = 0; i < times; i++)
            {
                result = Vector3.Scale(result, multiplier);
            }
            return result;
        }

        /// <summary>
        /// 複製に必要な総時間を推定（パフォーマンス指標）
        /// </summary>
        public float EstimateProcessingTime()
        {
            // 基本時間 + コピー数による増加
            const float baseTime = 0.1f;
            const float perCopyTime = 0.05f;
            
            return baseTime + (CopyCount * perCopyTime);
        }

        #region IEquatable<DuplicationStrategy> Implementation

        public bool Equals(DuplicationStrategy other)
        {
            return CopyCount == other.CopyCount &&
                   DeltaTransform.Equals(other.DeltaTransform) &&
                   CoordinateReference == other.CoordinateReference &&
                   PreserveHierarchy == other.PreserveHierarchy &&
                   UseProgressiveScaling == other.UseProgressiveScaling;
        }

        public override bool Equals(object obj)
        {
            return obj is DuplicationStrategy other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + CopyCount.GetHashCode();
                hash = hash * 23 + DeltaTransform.GetHashCode();
                hash = hash * 23 + CoordinateReference.GetHashCode();
                hash = hash * 23 + PreserveHierarchy.GetHashCode();
                hash = hash * 23 + UseProgressiveScaling.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(DuplicationStrategy left, DuplicationStrategy right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DuplicationStrategy left, DuplicationStrategy right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"DuplicationStrategy(Count: {CopyCount}, Delta: {DeltaTransform}, Coordinate: {CoordinateReference}, PreserveHierarchy: {PreserveHierarchy}, Progressive: {UseProgressiveScaling})";
        }
    }
}