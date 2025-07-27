using System;

namespace BitAssistTools.Domain.Models.Renaming
{
    /// <summary>
    /// 接頭辞・接尾辞追加のルールを表現する値オブジェクト
    /// </summary>
    public readonly struct PrefixSuffixRule : IEquatable<PrefixSuffixRule>
    {
        /// <summary>接頭辞</summary>
        public readonly string Prefix;
        
        /// <summary>接尾辞</summary>
        public readonly string Suffix;
        
        /// <summary>区切り文字</summary>
        public readonly string Delimiter;

        public PrefixSuffixRule(string prefix, string suffix, string delimiter)
        {
            Prefix = prefix ?? string.Empty;
            Suffix = suffix ?? string.Empty;
            Delimiter = delimiter ?? string.Empty;
        }

        /// <summary>デフォルトのルール</summary>
        public static PrefixSuffixRule Default => new PrefixSuffixRule("Prefix", "Suffix", "_");

        /// <summary>
        /// 指定した名前にルールを適用
        /// </summary>
        public string ApplyTo(string originalName)
        {
            if (string.IsNullOrEmpty(originalName))
                return originalName;

            string result = originalName;
            
            // 接頭辞を追加
            if (!string.IsNullOrEmpty(Prefix))
            {
                result = Prefix + Delimiter + result;
            }
            
            // 接尾辞を追加
            if (!string.IsNullOrEmpty(Suffix))
            {
                result = result + Delimiter + Suffix;
            }
            
            return result;
        }

        /// <summary>
        /// このルールが有効かどうかを判定
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Prefix) || !string.IsNullOrEmpty(Suffix);
        }

        #region IEquatable<PrefixSuffixRule> Implementation

        public bool Equals(PrefixSuffixRule other)
        {
            return Prefix == other.Prefix &&
                   Suffix == other.Suffix &&
                   Delimiter == other.Delimiter;
        }

        public override bool Equals(object obj)
        {
            return obj is PrefixSuffixRule other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Prefix?.GetHashCode() ?? 0);
                hash = hash * 23 + (Suffix?.GetHashCode() ?? 0);
                hash = hash * 23 + (Delimiter?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static bool operator ==(PrefixSuffixRule left, PrefixSuffixRule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PrefixSuffixRule left, PrefixSuffixRule right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"PrefixSuffixRule(Prefix: '{Prefix}', Suffix: '{Suffix}', Delimiter: '{Delimiter}')";
        }
    }
}