using System;

namespace BitAssistTools.Domain.Models.Texture
{
    /// <summary>
    /// 法線マップの強度を表現する値オブジェクト
    /// </summary>
    public readonly struct NormalStrength : IEquatable<NormalStrength>
    {
        /// <summary>法線マップの強度値</summary>
        public readonly float Value;

        public NormalStrength(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentException("Normal strength must be a valid number", nameof(value));
            if (value < 0f)
                throw new ArgumentOutOfRangeException(nameof(value), "Normal strength must be non-negative");
                
            Value = value;
        }

        /// <summary>デフォルトの法線強度（1.0）</summary>
        public static NormalStrength Default => new NormalStrength(1.0f);

        /// <summary>法線強度なし（0.0）</summary>
        public static NormalStrength None => new NormalStrength(0.0f);

        #region IEquatable<NormalStrength> Implementation

        public bool Equals(NormalStrength other)
        {
            return Math.Abs(Value - other.Value) < float.Epsilon;
        }

        public override bool Equals(object obj)
        {
            return obj is NormalStrength other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(NormalStrength left, NormalStrength right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NormalStrength left, NormalStrength right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString() => $"NormalStrength({Value:F2})";
    }
}