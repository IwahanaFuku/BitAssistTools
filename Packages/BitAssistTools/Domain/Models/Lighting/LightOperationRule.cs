using System;

namespace BitAssistTools.Domain.Models.Lighting
{
    /// <summary>
    /// ライト操作のルールを表現する値オブジェクト
    /// </summary>
    public readonly struct LightOperationRule : IEquatable<LightOperationRule>
    {
        /// <summary>適用する強度値</summary>
        public readonly float IntensityValue;
        
        /// <summary>四則演算の種類</summary>
        public readonly ArithmeticOperation Operation;

        public LightOperationRule(float intensityValue, ArithmeticOperation operation)
        {
            if (float.IsNaN(intensityValue) || float.IsInfinity(intensityValue))
                throw new ArgumentException("Intensity value must be a valid number", nameof(intensityValue));
            
            IntensityValue = intensityValue;
            Operation = operation;
        }

        /// <summary>デフォルトのルール（+1.0）</summary>
        public static LightOperationRule Default => new LightOperationRule(1.0f, ArithmeticOperation.Addition);

        /// <summary>
        /// 指定した現在の強度値に対してルールを適用した結果を計算
        /// </summary>
        public float ApplyTo(float currentIntensity)
        {
            switch (Operation)
            {
                case ArithmeticOperation.Addition:
                    return currentIntensity + IntensityValue;
                case ArithmeticOperation.Multiplication:
                    return currentIntensity * IntensityValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// このルールが有効かどうかを判定
        /// </summary>
        public bool IsValid()
        {
            // 乗算で0の場合は特別に許可（ライトを消すため）
            if (Operation == ArithmeticOperation.Multiplication && IntensityValue == 0.0f)
                return true;
                
            // 負数は基本的に無効（ただし減算的な加算は許可）
            return !float.IsNaN(IntensityValue) && !float.IsInfinity(IntensityValue);
        }

        #region IEquatable<LightOperationRule> Implementation

        public bool Equals(LightOperationRule other)
        {
            return Math.Abs(IntensityValue - other.IntensityValue) < float.Epsilon &&
                   Operation == other.Operation;
        }

        public override bool Equals(object obj)
        {
            return obj is LightOperationRule other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IntensityValue.GetHashCode() * 397) ^ (int)Operation;
            }
        }

        public static bool operator ==(LightOperationRule left, LightOperationRule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LightOperationRule left, LightOperationRule right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            string operationSymbol = Operation == ArithmeticOperation.Addition ? "+" : "×";
            return $"LightOperation({operationSymbol}{IntensityValue})";
        }
    }
}