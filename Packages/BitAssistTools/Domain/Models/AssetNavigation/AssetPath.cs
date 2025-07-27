using System;

namespace BitAssistTools.Domain.Models.AssetNavigation
{
    /// <summary>
    /// アセットパスを表現する値オブジェクト
    /// </summary>
    public readonly struct AssetPath : IEquatable<AssetPath>
    {
        /// <summary>アセットパス</summary>
        public readonly string Path;

        public AssetPath(string path)
        {
            Path = path ?? string.Empty;
        }

        /// <summary>デフォルトのアセットパス</summary>
        public static AssetPath Default => new AssetPath("Assets/");

        /// <summary>
        /// このパスが有効かどうかを判定
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Path) && 
                   Path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// パスを正規化（Assets/で始まるように）
        /// </summary>
        public AssetPath Normalize()
        {
            if (string.IsNullOrEmpty(Path))
                return new AssetPath("Assets/");

            if (Path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                return this;

            // Assets/を前に付ける
            string normalizedPath = Path.TrimStart('/');
            if (!normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = "Assets/" + normalizedPath;
            }

            return new AssetPath(normalizedPath);
        }

        #region IEquatable<AssetPath> Implementation

        public bool Equals(AssetPath other)
        {
            return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is AssetPath other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Path?.ToLowerInvariant().GetHashCode() ?? 0;
        }

        public static bool operator ==(AssetPath left, AssetPath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetPath left, AssetPath right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return Path ?? string.Empty;
        }

        /// <summary>
        /// 暗黙的な文字列変換
        /// </summary>
        public static implicit operator string(AssetPath assetPath)
        {
            return assetPath.Path;
        }

        /// <summary>
        /// 暗黙的なAssetPath変換
        /// </summary>
        public static implicit operator AssetPath(string path)
        {
            return new AssetPath(path);
        }
    }
}