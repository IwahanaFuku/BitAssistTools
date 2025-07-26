using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Selection
{
    /// <summary>
    /// 選択されたオブジェクトの集合を表現する値オブジェクト
    /// </summary>
    [Serializable]
    public readonly struct SelectionSet : IEquatable<SelectionSet>
    {
        public readonly string Name;
        public readonly DateTime CreatedAt;
        public readonly IReadOnlyList<UnityEngine.Object> Objects;
        public readonly bool IsActive;

        public SelectionSet(string name, IEnumerable<UnityEngine.Object> objects, bool isActive = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CreatedAt = DateTime.Now;
            Objects = objects?.Where(obj => obj != null).ToList().AsReadOnly() ?? 
                     throw new ArgumentNullException(nameof(objects));
            IsActive = isActive;
        }

        public SelectionSet(string name, DateTime createdAt, IEnumerable<UnityEngine.Object> objects, bool isActive)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CreatedAt = createdAt;
            Objects = objects?.Where(obj => obj != null).ToList().AsReadOnly() ?? 
                     throw new ArgumentNullException(nameof(objects));
            IsActive = isActive;
        }

        /// <summary>
        /// 空の選択セット
        /// </summary>
        public static SelectionSet Empty => new SelectionSet("Empty", Array.Empty<UnityEngine.Object>(), false);

        /// <summary>
        /// 選択されているオブジェクトの数
        /// </summary>
        public int Count => Objects.Count;

        /// <summary>
        /// 有効なオブジェクトが含まれているか
        /// </summary>
        public bool HasValidObjects => Objects.Any(obj => obj != null);

        /// <summary>
        /// アクティブ状態を変更した新しいSelectionSetを返す
        /// </summary>
        public SelectionSet WithActiveState(bool isActive)
        {
            return new SelectionSet(Name, CreatedAt, Objects, isActive);
        }

        /// <summary>
        /// 名前を変更した新しいSelectionSetを返す
        /// </summary>
        public SelectionSet WithName(string newName)
        {
            return new SelectionSet(newName, CreatedAt, Objects, IsActive);
        }

        /// <summary>
        /// 有効なオブジェクトのみを含む新しいSelectionSetを返す
        /// </summary>
        public SelectionSet GetValidObjectsOnly()
        {
            var validObjects = Objects.Where(obj => obj != null);
            return new SelectionSet(Name, CreatedAt, validObjects, IsActive);
        }

        #region IEquatable<SelectionSet> Implementation

        public bool Equals(SelectionSet other)
        {
            return Name == other.Name &&
                   CreatedAt.Equals(other.CreatedAt) &&
                   Objects.SequenceEqual(other.Objects) &&
                   IsActive == other.IsActive;
        }

        public override bool Equals(object obj)
        {
            return obj is SelectionSet other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name?.GetHashCode() ?? 0);
                hash = hash * 23 + CreatedAt.GetHashCode();
                hash = hash * 23 + Objects.Count.GetHashCode();
                hash = hash * 23 + IsActive.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(SelectionSet left, SelectionSet right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SelectionSet left, SelectionSet right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"SelectionSet(Name: {Name}, Count: {Count}, IsActive: {IsActive}, CreatedAt: {CreatedAt:yyyy-MM-dd HH:mm:ss})";
        }
    }
}