using System;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Transform
{
    /// <summary>
    /// Transform情報を表現する値オブジェクト
    /// 不変性を保ち、ドメインの整合性を維持する
    /// </summary>
    [Serializable]
    public readonly struct TransformData : IEquatable<TransformData>
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 LocalScale;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            Position = position;
            Rotation = rotation;
            LocalScale = localScale;
        }

        /// <summary>
        /// デフォルトのTransformData（位置:原点、回転:なし、スケール:1）
        /// </summary>
        public static TransformData Identity => new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);

        /// <summary>
        /// UnityのTransformコンポーネントからTransformDataを作成
        /// </summary>
        public static TransformData FromUnityTransform(UnityEngine.Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformData(
                transform.position,
                transform.rotation,
                transform.localScale
            );
        }

        /// <summary>
        /// 位置のみを変更した新しいTransformDataを返す
        /// </summary>
        public TransformData WithPosition(Vector3 newPosition)
        {
            return new TransformData(newPosition, Rotation, LocalScale);
        }

        /// <summary>
        /// 回転のみを変更した新しいTransformDataを返す
        /// </summary>
        public TransformData WithRotation(Quaternion newRotation)
        {
            return new TransformData(Position, newRotation, LocalScale);
        }

        /// <summary>
        /// スケールのみを変更した新しいTransformDataを返す
        /// </summary>
        public TransformData WithScale(Vector3 newScale)
        {
            return new TransformData(Position, Rotation, newScale);
        }

        /// <summary>
        /// 他のTransformDataとの合成（加算）
        /// </summary>
        public TransformData Add(TransformData other)
        {
            return new TransformData(
                Position + other.Position,
                Rotation * other.Rotation,
                Vector3.Scale(LocalScale, other.LocalScale)
            );
        }

        /// <summary>
        /// 座標系変換を適用した新しいTransformDataを返す
        /// </summary>
        public TransformData TransformBy(UnityEngine.Transform transformSpace)
        {
            if (transformSpace == null)
                return this;

            return new TransformData(
                transformSpace.TransformPoint(Position),
                transformSpace.rotation * Rotation,
                Vector3.Scale(transformSpace.lossyScale, LocalScale)
            );
        }

        #region IEquatable<TransformData> Implementation

        public bool Equals(TransformData other)
        {
            return Position.Equals(other.Position) &&
                   Rotation.Equals(other.Rotation) &&
                   LocalScale.Equals(other.LocalScale);
        }

        public override bool Equals(object obj)
        {
            return obj is TransformData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Position.GetHashCode();
                hash = hash * 23 + Rotation.GetHashCode();
                hash = hash * 23 + LocalScale.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(TransformData left, TransformData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TransformData left, TransformData right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"TransformData(Position: {Position}, Rotation: {Rotation}, Scale: {LocalScale})";
        }
    }
}