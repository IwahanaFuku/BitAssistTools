using System;

namespace BitAssistTools.Domain.Models.Renaming
{
    /// <summary>
    /// 文字列置換のルールを表現する値オブジェクト
    /// </summary>
    public readonly struct StringReplaceRule : IEquatable<StringReplaceRule>
    {
        /// <summary>検索する文字列</summary>
        public readonly string SearchString;
        
        /// <summary>置換する文字列</summary>
        public readonly string ReplaceString;

        public StringReplaceRule(string searchString, string replaceString)
        {
            SearchString = searchString ?? string.Empty;
            ReplaceString = replaceString ?? string.Empty;
        }

        /// <summary>デフォルトのルール</summary>
        public static StringReplaceRule Default => new StringReplaceRule("", "");

        /// <summary>
        /// 指定した名前にルールを適用
        /// </summary>
        public string ApplyTo(string originalName)
        {
            if (string.IsNullOrEmpty(originalName) || string.IsNullOrEmpty(SearchString))
                return originalName;

            return originalName.Replace(SearchString, ReplaceString);
        }

        /// <summary>
        /// このルールが有効かどうかを判定
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(SearchString);
        }

        #region IEquatable<StringReplaceRule> Implementation

        public bool Equals(StringReplaceRule other)
        {
            return SearchString == other.SearchString &&
                   ReplaceString == other.ReplaceString;
        }

        public override bool Equals(object obj)
        {
            return obj is StringReplaceRule other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SearchString?.GetHashCode() ?? 0) * 397) ^ (ReplaceString?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(StringReplaceRule left, StringReplaceRule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringReplaceRule left, StringReplaceRule right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"StringReplaceRule('{SearchString}' → '{ReplaceString}')";
        }
    }
}