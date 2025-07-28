using System;

namespace BitAssistTools.Domain.Models.TextureTools
{
    /// <summary>
    /// 詳細機能タイプのドメインモデル
    /// </summary>
    public readonly struct DetailFunctionType : IEquatable<DetailFunctionType>
    {
        public readonly DetailFunction Function;
        public readonly string DisplayName;
        public readonly string Description;

        public DetailFunctionType(DetailFunction function, string displayName, string description)
        {
            Function = function;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public bool Equals(DetailFunctionType other)
        {
            return Function == other.Function;
        }

        public override bool Equals(object obj)
        {
            return obj is DetailFunctionType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Function;
        }

        public static bool operator ==(DetailFunctionType left, DetailFunctionType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DetailFunctionType left, DetailFunctionType right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// 詳細機能列挙型
    /// </summary>
    public enum DetailFunction
    {
        None,           // 未選択
        PngConvert,     // PNG変換
        ChannelInvert,  // チャンネル反転
        NormalMap,      // 法線マップ編集
        MaskMerge       // マスク統合
    }
}