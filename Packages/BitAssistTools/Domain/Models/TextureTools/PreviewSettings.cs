using System;

namespace BitAssistTools.Domain.Models.TextureTools
{
    /// <summary>
    /// プレビュー設定のドメインモデル
    /// </summary>
    public readonly struct PreviewSettings : IEquatable<PreviewSettings>
    {
        public readonly PreviewMode Mode;
        public readonly bool ShowBeforeAfter;
        public readonly int PreviewSize;

        public PreviewSettings(PreviewMode mode, bool showBeforeAfter, int previewSize = 256)
        {
            Mode = mode;
            ShowBeforeAfter = showBeforeAfter;
            PreviewSize = previewSize;
        }

        public bool Equals(PreviewSettings other)
        {
            return Mode == other.Mode && ShowBeforeAfter == other.ShowBeforeAfter && PreviewSize == other.PreviewSize;
        }

        public override bool Equals(object obj)
        {
            return obj is PreviewSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (int)Mode;
                hash = hash * 23 + ShowBeforeAfter.GetHashCode();
                hash = hash * 23 + PreviewSize.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(PreviewSettings left, PreviewSettings right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PreviewSettings left, PreviewSettings right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// プレビューモード列挙型
    /// </summary>
    public enum PreviewMode
    {
        RGB,    // RGB表示
        Alpha,  // A表示
        Normal, // 法線ビジュアライズ
        R,      // Rチャンネルのみ
        G,      // Gチャンネルのみ
        B       // Bチャンネルのみ
    }
}