using System;
using System.Collections.Generic;
using BitAssistTools.Domain.Models.Selection;
using UnityEngine;

namespace BitAssistTools.Domain.Services
{
    /// <summary>
    /// 選択管理を担当するドメインサービス
    /// </summary>
    public interface ISelectionManagementService
    {
        /// <summary>
        /// 現在の選択を履歴に追加
        /// </summary>
        /// <param name="history">対象の選択履歴</param>
        /// <param name="currentSelection">現在の選択オブジェクト</param>
        /// <param name="selectionName">選択セットの名前（省略時は自動生成）</param>
        /// <returns>更新された選択履歴</returns>
        SelectionHistory AddCurrentSelection(
            SelectionHistory history, 
            IEnumerable<UnityEngine.Object> currentSelection,
            string selectionName = null);

        /// <summary>
        /// 履歴から選択を復元
        /// </summary>
        /// <param name="history">選択履歴</param>
        /// <param name="includeInactive">非アクティブな選択も含めるか</param>
        /// <returns>復元されたオブジェクト配列</returns>
        UnityEngine.Object[] RestoreSelection(SelectionHistory history, bool includeInactive = false);

        /// <summary>
        /// 階層的な選択を実行
        /// </summary>
        /// <param name="rootObjects">ルートオブジェクト配列</param>
        /// <param name="selectionMode">選択モード</param>
        /// <param name="includeInactive">非アクティブオブジェクトも含めるか</param>
        /// <returns>選択されたオブジェクト配列</returns>
        GameObject[] SelectHierarchically(
            GameObject[] rootObjects, 
            HierarchicalSelectionMode selectionMode,
            bool includeInactive = false);

        /// <summary>
        /// 条件に基づいてオブジェクトをフィルタリング
        /// </summary>
        /// <param name="objects">フィルタリング対象オブジェクト</param>
        /// <param name="filter">フィルター条件</param>
        /// <returns>フィルタリング結果</returns>
        UnityEngine.Object[] FilterObjects(UnityEngine.Object[] objects, ISelectionFilter filter);

        /// <summary>
        /// 選択履歴のマージ
        /// </summary>
        /// <param name="primaryHistory">主要な履歴</param>
        /// <param name="secondaryHistory">統合する履歴</param>
        /// <param name="mergeStrategy">マージ戦略</param>
        /// <returns>マージされた履歴</returns>
        SelectionHistory MergeHistories(
            SelectionHistory primaryHistory, 
            SelectionHistory secondaryHistory,
            HistoryMergeStrategy mergeStrategy);

        /// <summary>
        /// 無効な参照をクリーンアップ
        /// </summary>
        /// <param name="history">対象の選択履歴</param>
        /// <returns>クリーンアップ結果</returns>
        SelectionCleanupResult CleanupInvalidReferences(SelectionHistory history);

        /// <summary>
        /// 選択パターンの分析
        /// </summary>
        /// <param name="history">分析対象の履歴</param>
        /// <returns>選択パターンの分析結果</returns>
        SelectionPatternAnalysis AnalyzeSelectionPatterns(SelectionHistory history);
    }

    /// <summary>
    /// 階層的選択のモードを表現する列挙型
    /// </summary>
    public enum HierarchicalSelectionMode
    {
        /// <summary>子オブジェクトのみ</summary>
        ChildrenOnly,
        /// <summary>全ての子孫オブジェクト</summary>
        AllDescendants,
        /// <summary>親オブジェクトのみ</summary>
        ParentOnly,
        /// <summary>兄弟オブジェクト</summary>
        Siblings,
        /// <summary>同一階層レベル</summary>
        SameLevel
    }

    /// <summary>
    /// 履歴マージ戦略を表現する列挙型
    /// </summary>
    public enum HistoryMergeStrategy
    {
        /// <summary>時系列順でマージ</summary>
        Chronological,
        /// <summary>主要履歴を優先</summary>
        PrimaryFirst,
        /// <summary>重複を除去してマージ</summary>
        DeduplicateObjects,
        /// <summary>アクティブな選択のみ</summary>
        ActiveOnly
    }

    /// <summary>
    /// 選択フィルターのインターフェース
    /// </summary>
    public interface ISelectionFilter
    {
        bool ShouldInclude(UnityEngine.Object obj);
        string FilterDescription { get; }
    }

    /// <summary>
    /// 選択クリーンアップの結果を表現する値オブジェクト
    /// </summary>
    public readonly struct SelectionCleanupResult
    {
        public readonly int RemovedSelections;
        public readonly int CleanedReferences;
        public readonly int TotalProcessed;
        public readonly TimeSpan ProcessingTime;

        public SelectionCleanupResult(int removedSelections, int cleanedReferences, int totalProcessed, TimeSpan processingTime)
        {
            RemovedSelections = removedSelections;
            CleanedReferences = cleanedReferences;
            TotalProcessed = totalProcessed;
            ProcessingTime = processingTime;
        }

        public override string ToString()
        {
            return $"SelectionCleanupResult(Removed: {RemovedSelections}, Cleaned: {CleanedReferences}, Total: {TotalProcessed}, Time: {ProcessingTime.TotalMilliseconds:F2}ms)";
        }
    }

    /// <summary>
    /// 選択パターンの分析結果を表現する値オブジェクト
    /// </summary>
    public readonly struct SelectionPatternAnalysis
    {
        public readonly int TotalSelections;
        public readonly int UniqueObjects;
        public readonly UnityEngine.Object MostSelectedObject;
        public readonly int MostSelectedCount;
        public readonly TimeSpan AverageSelectionInterval;
        public readonly IReadOnlyDictionary<Type, int> ObjectTypeDistribution;

        public SelectionPatternAnalysis(
            int totalSelections,
            int uniqueObjects,
            UnityEngine.Object mostSelectedObject,
            int mostSelectedCount,
            TimeSpan averageSelectionInterval,
            IReadOnlyDictionary<Type, int> objectTypeDistribution)
        {
            TotalSelections = totalSelections;
            UniqueObjects = uniqueObjects;
            MostSelectedObject = mostSelectedObject;
            MostSelectedCount = mostSelectedCount;
            AverageSelectionInterval = averageSelectionInterval;
            ObjectTypeDistribution = objectTypeDistribution;
        }

        public override string ToString()
        {
            return $"SelectionPatternAnalysis(Total: {TotalSelections}, Unique: {UniqueObjects}, MostSelected: {MostSelectedObject?.name} ({MostSelectedCount}x))";
        }
    }
}