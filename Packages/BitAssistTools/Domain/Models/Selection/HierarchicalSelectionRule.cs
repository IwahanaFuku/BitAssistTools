using System;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Selection
{
    /// <summary>
    /// 階層選択のルールを表現する値オブジェクト
    /// </summary>
    public readonly struct HierarchicalSelectionRule : IEquatable<HierarchicalSelectionRule>
    {
        /// <summary>基準とする親の階層数</summary>
        public readonly int ParentDepth;
        
        /// <summary>選択したオブジェクトと同じ順番のみを選択するかどうか</summary>
        public readonly bool OnlyOrder;
        
        /// <summary>子オブジェクトをすべて選択するかどうか</summary>
        public readonly bool SelectChildren;

        public HierarchicalSelectionRule(int parentDepth, bool onlyOrder, bool selectChildren)
        {
            if (parentDepth < 1 || parentDepth > 5)
                throw new ArgumentOutOfRangeException(nameof(parentDepth), "Parent depth must be between 1 and 5");
                
            ParentDepth = parentDepth;
            OnlyOrder = onlyOrder;
            SelectChildren = selectChildren;
        }

        /// <summary>デフォルトのルール</summary>
        public static HierarchicalSelectionRule Default => new HierarchicalSelectionRule(1, true, false);

        /// <summary>
        /// 指定したGameObjectに対して階層選択を実行可能かどうかを判定
        /// </summary>
        public bool CanApplyTo(GameObject gameObject)
        {
            if (gameObject == null) return false;
            
            // 親の階層数分上位に親が存在するかチェック
            var parent = gameObject.transform;
            for (int i = 0; i < ParentDepth; i++)
            {
                parent = parent.parent;
                if (parent == null && i < ParentDepth - 1)
                {
                    return false; // 指定した階層数分の親が存在しない
                }
            }
            
            return true;
        }

        /// <summary>
        /// 指定したGameObjectの基準となる親オブジェクトを取得
        /// </summary>
        public GameObject GetParentObject(GameObject gameObject)
        {
            if (!CanApplyTo(gameObject))
                throw new ArgumentException("Cannot apply hierarchical selection to this object", nameof(gameObject));

            var parent = gameObject.transform;
            for (int i = 0; i < ParentDepth; i++)
            {
                if (parent.parent != null)
                {
                    parent = parent.parent;
                }
            }
            
            return parent.gameObject;
        }

        /// <summary>
        /// 指定したGameObjectの階層の深さを取得
        /// </summary>
        public int GetDepth(GameObject gameObject)
        {
            if (gameObject == null) return 0;
            
            int depth = 0;
            UnityEngine.Transform parent = gameObject.transform.parent;
            while (parent != null)
            {
                depth++;
                parent = parent.parent;
            }
            return depth;
        }

        #region IEquatable<HierarchicalSelectionRule> Implementation

        public bool Equals(HierarchicalSelectionRule other)
        {
            return ParentDepth == other.ParentDepth &&
                   OnlyOrder == other.OnlyOrder &&
                   SelectChildren == other.SelectChildren;
        }

        public override bool Equals(object obj)
        {
            return obj is HierarchicalSelectionRule other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ParentDepth.GetHashCode();
                hash = hash * 23 + OnlyOrder.GetHashCode();
                hash = hash * 23 + SelectChildren.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(HierarchicalSelectionRule left, HierarchicalSelectionRule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HierarchicalSelectionRule left, HierarchicalSelectionRule right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"HierarchicalSelectionRule(ParentDepth: {ParentDepth}, OnlyOrder: {OnlyOrder}, SelectChildren: {SelectChildren})";
        }
    }
}