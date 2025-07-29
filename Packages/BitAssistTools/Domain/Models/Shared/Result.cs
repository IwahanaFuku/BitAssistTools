using System;

namespace BitAssistTools.Domain.Models.Shared
{
    /// <summary>
    /// 操作の成功・失敗を表現する値オブジェクト
    /// </summary>
    public readonly struct Result<T> : IEquatable<Result<T>>
    {
        /// <summary>操作が成功したかどうか</summary>
        public readonly bool IsOk;
        
        /// <summary>エラーメッセージ（失敗時のみ）</summary>
        public readonly string Error;
        
        /// <summary>結果の値（成功時のみ）</summary>
        public readonly T Value;

        /// <summary>操作が失敗したかどうか</summary>
        public bool IsError => !IsOk;

        private Result(bool ok, T value, string error) 
        { 
            IsOk = ok; 
            Value = value; 
            Error = error; 
        }

        /// <summary>成功結果を作成</summary>
        public static Result<T> Ok(T value) => new Result<T>(true, value, null);
        
        /// <summary>失敗結果を作成</summary>
        public static Result<T> Fail(string error) => new Result<T>(false, default, error);

        /// <summary>
        /// 成功時に値を取得、失敗時にデフォルト値を返す
        /// </summary>
        public T GetValueOrDefault(T defaultValue = default) => IsOk ? Value : defaultValue;

        /// <summary>
        /// 成功時に値を取得、失敗時に例外をスロー
        /// </summary>
        public T GetValueOrThrow()
        {
            if (!IsOk)
                throw new InvalidOperationException($"Result is in error state: {Error}");
            return Value;
        }

        #region IEquatable<Result<T>> Implementation

        public bool Equals(Result<T> other)
        {
            if (IsOk != other.IsOk) return false;
            if (IsOk)
            {
                return Equals(Value, other.Value);
            }
            else
            {
                return string.Equals(Error, other.Error);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Result<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IsOk.GetHashCode();
                hashCode = (hashCode * 397) ^ (IsOk ? (Value?.GetHashCode() ?? 0) : (Error?.GetHashCode() ?? 0));
                return hashCode;
            }
        }

        public static bool operator ==(Result<T> left, Result<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Result<T> left, Result<T> right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return IsOk ? $"Ok({Value})" : $"Error({Error})";
        }
    }

    /// <summary>
    /// 値のないResult（Unit型）
    /// </summary>
    public readonly struct Result : IEquatable<Result>
    {
        /// <summary>操作が成功したかどうか</summary>
        public readonly bool IsOk;
        
        /// <summary>エラーメッセージ（失敗時のみ）</summary>
        public readonly string Error;

        /// <summary>操作が失敗したかどうか</summary>
        public bool IsError => !IsOk;

        private Result(bool ok, string error) 
        { 
            IsOk = ok; 
            Error = error; 
        }

        /// <summary>成功結果を作成</summary>
        public static Result Ok() => new Result(true, null);
        
        /// <summary>失敗結果を作成</summary>
        public static Result Fail(string error) => new Result(false, error);

        #region IEquatable<Result> Implementation

        public bool Equals(Result other)
        {
            return IsOk == other.IsOk && string.Equals(Error, other.Error);
        }

        public override bool Equals(object obj)
        {
            return obj is Result other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IsOk.GetHashCode() * 397) ^ (Error?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(Result left, Result right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Result left, Result right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return IsOk ? "Ok" : $"Error({Error})";
        }
    }
}