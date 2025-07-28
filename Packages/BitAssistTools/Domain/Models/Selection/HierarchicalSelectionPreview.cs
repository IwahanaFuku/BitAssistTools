using System;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Selection
{
    /// <summary>
    /// 階層選択のプレビュー結果を表現する値オブジェクト
    /// </summary>
    public readonly struct HierarchicalSelectionPreview : IEquatable<HierarchicalSelectionPreview>
    {
        /// <summary>基準となる親オブジェクト</summary>
        public readonly GameObject BaseParent;
        
        /// <summary>基準親の兄弟オブジェクト配列</summary>
        public readonly GameObject[] ParentSiblings;
        
        /// <summary>実際に選択されるターゲットオブジェクト配列</summary>
        public readonly GameObject[] TargetObjects;
        
        /// <summary>選択理由の説明文</summary>
        public readonly string SelectionReason;
        
        /// <summary>プレビューが有効かどうか</summary>
        public readonly bool IsValid;
        
        /// <summary>エラーメッセージ（無効な場合）</summary>
        public readonly string ErrorMessage;

        public HierarchicalSelectionPreview(
            GameObject baseParent,
            GameObject[] parentSiblings,
            GameObject[] targetObjects,
            string selectionReason,
            bool isValid = true,
            string errorMessage = null)
        {
            BaseParent = baseParent;
            ParentSiblings = parentSiblings ?? new GameObject[0];
            TargetObjects = targetObjects ?? new GameObject[0];
            SelectionReason = selectionReason ?? string.Empty;
            IsValid = isValid;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        /// <summary>無効なプレビューを作成</summary>
        public static HierarchicalSelectionPreview Invalid(string errorMessage)
        {
            return new HierarchicalSelectionPreview(
                null, 
                new GameObject[0], 
                new GameObject[0], 
                string.Empty, 
                false, 
                errorMessage
            );
        }

        /// <summary>有効なプレビューを作成</summary>
        public static HierarchicalSelectionPreview Valid(
            GameObject baseParent,
            GameObject[] parentSiblings,
            GameObject[] targetObjects,
            string selectionReason)
        {
            return new HierarchicalSelectionPreview(
                baseParent,
                parentSiblings,
                targetObjects,
                selectionReason,
                true,
                null
            );
        }

        /// <summary>選択されるオブジェクト数</summary>
        public int TargetCount => TargetObjects?.Length ?? 0;

        /// <summary>基準親の兄弟数</summary>
        public int ParentSiblingCount => ParentSiblings?.Length ?? 0;

        /// <summary>プレビュー情報の要約テキストを取得</summary>
        public string GetSummaryText()
        {
            if (!IsValid)
            {
                return $"エラー: {ErrorMessage}";
            }

            var baseParentName = BaseParent != null ? BaseParent.name : "Unknown";
            return $"基準親: {baseParentName}, 兄弟: {ParentSiblingCount}個, 選択対象: {TargetCount}個";
        }

        /// <summary>詳細なプレビュー情報を取得</summary>
        public string GetDetailedText()
        {
            if (!IsValid)
            {
                return $"エラー: {ErrorMessage}";
            }

            var result = new System.Text.StringBuilder();
            result.AppendLine($"基準親: {BaseParent?.name ?? "Unknown"}");
            
            if (ParentSiblings != null && ParentSiblings.Length > 0)
            {
                result.AppendLine($"兄弟オブジェクト: {string.Join(", ", System.Array.ConvertAll(ParentSiblings, obj => obj?.name ?? "null"))}");
            }
            
            if (TargetObjects != null && TargetObjects.Length > 0)
            {
                result.AppendLine($"選択対象: {string.Join(", ", System.Array.ConvertAll(TargetObjects, obj => obj?.name ?? "null"))}");
            }
            
            result.AppendLine($"選択理由: {SelectionReason}");
            
            return result.ToString();
        }

        #region IEquatable<HierarchicalSelectionPreview> Implementation

        public bool Equals(HierarchicalSelectionPreview other)
        {
            if (BaseParent != other.BaseParent) return false;
            if (IsValid != other.IsValid) return false;
            if (SelectionReason != other.SelectionReason) return false;
            if (ErrorMessage != other.ErrorMessage) return false;
            
            // 配列の比較
            if (!ArrayEquals(ParentSiblings, other.ParentSiblings)) return false;
            if (!ArrayEquals(TargetObjects, other.TargetObjects)) return false;
            
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is HierarchicalSelectionPreview other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BaseParent?.GetHashCode() ?? 0);
                hash = hash * 23 + IsValid.GetHashCode();
                hash = hash * 23 + (SelectionReason?.GetHashCode() ?? 0);
                hash = hash * 23 + (ErrorMessage?.GetHashCode() ?? 0);
                
                if (ParentSiblings != null)
                {
                    for (int i = 0; i < ParentSiblings.Length; i++)
                    {
                        hash = hash * 23 + (ParentSiblings[i]?.GetHashCode() ?? 0);
                    }
                }
                
                if (TargetObjects != null)
                {
                    for (int i = 0; i < TargetObjects.Length; i++)
                    {
                        hash = hash * 23 + (TargetObjects[i]?.GetHashCode() ?? 0);
                    }
                }
                
                return hash;
            }
        }

        public static bool operator ==(HierarchicalSelectionPreview left, HierarchicalSelectionPreview right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HierarchicalSelectionPreview left, HierarchicalSelectionPreview right)
        {
            return !left.Equals(right);
        }

        #endregion

        /// <summary>配列の要素が等しいかチェック</summary>
        private static bool ArrayEquals(GameObject[] array1, GameObject[] array2)
        {
            if (array1 == null && array2 == null) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;
            
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }
            
            return true;
        }

        public override string ToString()
        {
            return IsValid 
                ? $"HierarchicalSelectionPreview(Valid: {GetSummaryText()})"
                : $"HierarchicalSelectionPreview(Invalid: {ErrorMessage})";
        }
    }
}