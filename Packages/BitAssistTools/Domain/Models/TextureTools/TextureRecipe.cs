using System;

namespace BitAssistTools.Domain.Models.TextureTools
{
    /// <summary>
    /// テクスチャレシピのドメインモデル
    /// </summary>
    public readonly struct TextureRecipe : IEquatable<TextureRecipe>
    {
        public readonly RecipeType Type;
        public readonly string DisplayName;
        public readonly string Description;

        public TextureRecipe(RecipeType type, string displayName, string description)
        {
            Type = type;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public bool Equals(TextureRecipe other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is TextureRecipe other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Type;
        }

        public static bool operator ==(TextureRecipe left, TextureRecipe right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextureRecipe left, TextureRecipe right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// レシピタイプ列挙型
    /// </summary>
    public enum RecipeType
    {
        None,           // 未選択
        PngConversion,  // PNG変換+元削除
        InvertAll,      // 全チャンネル反転
        NormalGInvert,  // 法線G反転
        NormalStrength15, // 法線強度1.5
        ORMMerge,       // ORM統合
        MRAMerge        // MRA統合
    }
}