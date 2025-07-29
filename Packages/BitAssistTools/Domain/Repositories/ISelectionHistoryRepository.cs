using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Domain.Repositories
{
    /// <summary>
    /// 選択履歴の永続化を担当するリポジトリインターフェース
    /// </summary>
    public interface ISelectionHistoryRepository
    {
        /// <summary>
        /// 選択履歴を保存
        /// </summary>
        /// <param name="history">保存する選択履歴</param>
        /// <returns>保存が成功したかどうか</returns>
        Task<bool> SaveAsync(SelectionHistory history);

        /// <summary>
        /// 選択履歴を読み込み
        /// </summary>
        /// <param name="historyId">履歴ID</param>
        /// <returns>読み込まれた選択履歴</returns>
        Task<SelectionHistory> LoadAsync(Guid historyId);

        /// <summary>
        /// デフォルトの選択履歴を読み込み
        /// </summary>
        /// <returns>デフォルトの選択履歴</returns>
        Task<SelectionHistory> LoadDefaultAsync();

        /// <summary>
        /// 全ての選択履歴IDを取得
        /// </summary>
        /// <returns>履歴ID一覧</returns>
        Task<IEnumerable<Guid>> GetAllHistoryIdsAsync();

        /// <summary>
        /// 選択履歴を削除
        /// </summary>
        /// <param name="historyId">削除する履歴ID</param>
        /// <returns>削除が成功したかどうか</returns>
        Task<bool> DeleteAsync(Guid historyId);

        /// <summary>
        /// 選択履歴が存在するかチェック
        /// </summary>
        /// <param name="historyId">確認する履歴ID</param>
        /// <returns>存在するかどうか</returns>
        Task<bool> ExistsAsync(Guid historyId);

        /// <summary>
        /// 古い履歴をクリーンアップ
        /// </summary>
        /// <param name="cutoffDate">カットオフ日時</param>
        /// <returns>削除された履歴の数</returns>
        Task<int> CleanupOldHistoriesAsync(DateTime cutoffDate);

        /// <summary>
        /// 履歴の統計情報を取得
        /// </summary>
        /// <returns>統計情報</returns>
        Task<HistoryRepositoryStats> GetStatsAsync();
    }

    /// <summary>
    /// 履歴リポジトリの統計情報を表現する値オブジェクト
    /// </summary>
    public readonly struct HistoryRepositoryStats
    {
        public readonly int TotalHistories;
        public readonly long TotalStorageSize;
        public readonly DateTime OldestHistoryDate;
        public readonly DateTime NewestHistoryDate;
        public readonly int AverageSelectionsPerHistory;

        public HistoryRepositoryStats(
            int totalHistories,
            long totalStorageSize,
            DateTime oldestHistoryDate,
            DateTime newestHistoryDate,
            int averageSelectionsPerHistory)
        {
            TotalHistories = totalHistories;
            TotalStorageSize = totalStorageSize;
            OldestHistoryDate = oldestHistoryDate;
            NewestHistoryDate = newestHistoryDate;
            AverageSelectionsPerHistory = averageSelectionsPerHistory;
        }

        public override string ToString()
        {
            return $"HistoryRepositoryStats(Histories: {TotalHistories}, Size: {TotalStorageSize} bytes, Range: {OldestHistoryDate:yyyy-MM-dd} - {NewestHistoryDate:yyyy-MM-dd})";
        }
    }
}