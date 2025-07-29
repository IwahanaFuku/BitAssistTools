using System;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Texture
{
    /// <summary>
    /// マスクテクスチャのソース情報を表現する値オブジェクト
    /// </summary>
    [Serializable]
    public sealed class MaskSource : IEquatable<MaskSource>
    {
        /// <summary>ソーステクスチャ</summary>
        public Texture2D Source;
        
        /// <summary>ソースチャンネル</summary>
        public Channel SourceChannel = Channel.R;
        
        /// <summary>チャンネルを反転するかどうか</summary>
        public bool Invert = false;

        public MaskSource()
        {
        }

        public MaskSource(Texture2D source, Channel sourceChannel, bool invert = false)
        {
            Source = source;
            SourceChannel = sourceChannel;
            Invert = invert;
        }

        /// <summary>
        /// このマスクソースが有効かどうかを判定
        /// </summary>
        public bool IsValid => Source != null;

        #region IEquatable<MaskSource> Implementation

        public bool Equals(MaskSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Source, other.Source) && 
                   SourceChannel == other.SourceChannel && 
                   Invert == other.Invert;
        }

        public override bool Equals(object obj)
        {
            return obj is MaskSource other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)SourceChannel;
                hashCode = (hashCode * 397) ^ Invert.GetHashCode();
                return hashCode;
            }
        }

        #endregion

        public override string ToString()
        {
            var sourceName = Source ? Source.name : "None";
            var invertText = Invert ? " (Inverted)" : "";
            return $"MaskSource({sourceName}.{SourceChannel}{invertText})";
        }
    }

    /// <summary>
    /// RGBAマスクテクスチャのマッピング設定を表現する値オブジェクト
    /// </summary>
    [Serializable]
    public sealed class MaskMapping : IEquatable<MaskMapping>
    {
        /// <summary>Rチャンネルの出力設定</summary>
        public MaskSource OutR;
        
        /// <summary>Gチャンネルの出力設定</summary>
        public MaskSource OutG;
        
        /// <summary>Bチャンネルの出力設定</summary>
        public MaskSource OutB;
        
        /// <summary>Aチャンネルの出力設定</summary>
        public MaskSource OutA;

        public MaskMapping()
        {
        }

        public MaskMapping(MaskSource outR, MaskSource outG, MaskSource outB, MaskSource outA)
        {
            OutR = outR;
            OutG = outG;
            OutB = outB;
            OutA = outA;
        }

        /// <summary>
        /// 少なくとも1つの有効なマスクソースがあるかどうかを判定
        /// </summary>
        public bool HasValidSources =>
            (OutR?.IsValid == true) || 
            (OutG?.IsValid == true) || 
            (OutB?.IsValid == true) || 
            (OutA?.IsValid == true);

        /// <summary>
        /// 有効なマスクソースの数を取得
        /// </summary>
        public int ValidSourceCount
        {
            get
            {
                int count = 0;
                if (OutR?.IsValid == true) count++;
                if (OutG?.IsValid == true) count++;
                if (OutB?.IsValid == true) count++;
                if (OutA?.IsValid == true) count++;
                return count;
            }
        }

        #region IEquatable<MaskMapping> Implementation

        public bool Equals(MaskMapping other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(OutR, other.OutR) && 
                   Equals(OutG, other.OutG) && 
                   Equals(OutB, other.OutB) && 
                   Equals(OutA, other.OutA);
        }

        public override bool Equals(object obj)
        {
            return obj is MaskMapping other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (OutR != null ? OutR.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutG != null ? OutG.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutB != null ? OutB.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutA != null ? OutA.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion

        public override string ToString()
        {
            return $"MaskMapping(R:{OutR}, G:{OutG}, B:{OutB}, A:{OutA})";
        }
    }
}