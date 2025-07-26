using System;
using System.Collections.Generic;
using System.Linq;

namespace BitAssistTools.Domain.Models.Selection
{
    /// <summary>
    /// 選択履歴を管理するエンティティ
    /// 一意性を持ち、ライフサイクルを管理する
    /// </summary>
    public class SelectionHistory
    {
        private readonly List<SelectionSet> _selections;
        private readonly int _maxHistoryCount;

        public Guid Id { get; }
        public IReadOnlyList<SelectionSet> Selections => _selections.AsReadOnly();
        public int MaxHistoryCount => _maxHistoryCount;
        public int CurrentCount => _selections.Count;
        public DateTime LastModified { get; private set; }

        public SelectionHistory(int maxHistoryCount = 10)
        {
            if (maxHistoryCount <= 0)
                throw new ArgumentException("Max history count must be positive", nameof(maxHistoryCount));

            Id = Guid.NewGuid();
            _maxHistoryCount = maxHistoryCount;
            _selections = new List<SelectionSet>();
            LastModified = DateTime.Now;
        }

        /// <summary>
        /// 新しい選択を履歴に追加
        /// </summary>
        public void AddSelection(SelectionSet selectionSet)
        {
            if (selectionSet.Objects == null || !selectionSet.Objects.Any())
                throw new ArgumentException("Selection set must contain at least one object", nameof(selectionSet));

            // 最大数を超えた場合は古いものを削除
            if (_selections.Count >= _maxHistoryCount)
            {
                _selections.RemoveAt(0);
            }

            _selections.Add(selectionSet);
            LastModified = DateTime.Now;
        }

        /// <summary>
        /// 現在の選択から自動的に名前付きの選択セットを作成して追加
        /// </summary>
        public void AddCurrentSelection(IEnumerable<UnityEngine.Object> currentObjects)
        {
            if (currentObjects == null || !currentObjects.Any())
                throw new ArgumentException("Current selection must contain at least one object", nameof(currentObjects));

            var name = $"Selection {_selections.Count + 1}";
            var selectionSet = new SelectionSet(name, currentObjects);
            AddSelection(selectionSet);
        }

        /// <summary>
        /// 指定したインデックスの選択を削除
        /// </summary>
        public bool RemoveSelection(int index)
        {
            if (index < 0 || index >= _selections.Count)
                return false;

            _selections.RemoveAt(index);
            LastModified = DateTime.Now;
            return true;
        }

        /// <summary>
        /// 指定した選択セットを削除
        /// </summary>
        public bool RemoveSelection(SelectionSet selectionSet)
        {
            var removed = _selections.Remove(selectionSet);
            if (removed)
            {
                LastModified = DateTime.Now;
            }
            return removed;
        }

        /// <summary>
        /// チェックされた（アクティブな）選択セットのみを取得
        /// </summary>
        public IEnumerable<SelectionSet> GetActiveSelections()
        {
            return _selections.Where(s => s.IsActive);
        }

        /// <summary>
        /// アクティブな選択セットに含まれる全オブジェクトを取得
        /// </summary>
        public IEnumerable<UnityEngine.Object> GetAllActiveObjects()
        {
            return GetActiveSelections()
                .SelectMany(s => s.Objects)
                .Where(obj => obj != null)
                .Distinct();
        }

        /// <summary>
        /// 指定した選択セットのアクティブ状態を切り替え
        /// </summary>
        public bool ToggleSelectionActive(int index)
        {
            if (index < 0 || index >= _selections.Count)
                return false;

            var selection = _selections[index];
            var updatedSelection = selection.WithActiveState(!selection.IsActive);
            _selections[index] = updatedSelection;
            LastModified = DateTime.Now;
            return true;
        }

        /// <summary>
        /// 履歴をクリア
        /// </summary>
        public void Clear()
        {
            _selections.Clear();
            LastModified = DateTime.Now;
        }

        /// <summary>
        /// 無効なオブジェクト参照をクリーンアップ
        /// </summary>
        public int CleanupInvalidReferences()
        {
            int removedCount = 0;
            for (int i = _selections.Count - 1; i >= 0; i--)
            {
                var selection = _selections[i];
                if (!selection.HasValidObjects)
                {
                    _selections.RemoveAt(i);
                    removedCount++;
                }
                else
                {
                    var cleanedSelection = selection.GetValidObjectsOnly();
                    if (cleanedSelection.Count != selection.Count)
                    {
                        _selections[i] = cleanedSelection;
                    }
                }
            }

            if (removedCount > 0)
            {
                LastModified = DateTime.Now;
            }

            return removedCount;
        }

        /// <summary>
        /// 履歴の統計情報を取得
        /// </summary>
        public SelectionHistoryStats GetStats()
        {
            return new SelectionHistoryStats(
                totalSelections: _selections.Count,
                activeSelections: GetActiveSelections().Count(),
                totalObjects: _selections.SelectMany(s => s.Objects).Count(),
                uniqueObjects: _selections.SelectMany(s => s.Objects).Distinct().Count(),
                lastModified: LastModified
            );
        }
    }

    /// <summary>
    /// 選択履歴の統計情報を表現する値オブジェクト
    /// </summary>
    public readonly struct SelectionHistoryStats
    {
        public readonly int TotalSelections;
        public readonly int ActiveSelections;
        public readonly int TotalObjects;
        public readonly int UniqueObjects;
        public readonly DateTime LastModified;

        public SelectionHistoryStats(int totalSelections, int activeSelections, int totalObjects, int uniqueObjects, DateTime lastModified)
        {
            TotalSelections = totalSelections;
            ActiveSelections = activeSelections;
            TotalObjects = totalObjects;
            UniqueObjects = uniqueObjects;
            LastModified = lastModified;
        }

        public override string ToString()
        {
            return $"SelectionHistoryStats(Selections: {TotalSelections}, Active: {ActiveSelections}, Objects: {TotalObjects}, Unique: {UniqueObjects})";
        }
    }
}