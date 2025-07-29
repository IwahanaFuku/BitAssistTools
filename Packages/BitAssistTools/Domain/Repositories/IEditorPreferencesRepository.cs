using System;
using System.Threading.Tasks;

namespace BitAssistTools.Domain.Repositories
{
    /// <summary>
    /// エディター設定の永続化を担当するリポジトリインターフェース
    /// </summary>
    public interface IEditorPreferencesRepository
    {
        /// <summary>
        /// 文字列値を保存
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>保存が成功したかどうか</returns>
        Task<bool> SetStringAsync(string key, string value);

        /// <summary>
        /// 文字列値を取得
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>取得された値</returns>
        Task<string> GetStringAsync(string key, string defaultValue = "");

        /// <summary>
        /// 整数値を保存
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>保存が成功したかどうか</returns>
        Task<bool> SetIntAsync(string key, int value);

        /// <summary>
        /// 整数値を取得
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>取得された値</returns>
        Task<int> GetIntAsync(string key, int defaultValue = 0);

        /// <summary>
        /// 浮動小数点値を保存
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>保存が成功したかどうか</returns>
        Task<bool> SetFloatAsync(string key, float value);

        /// <summary>
        /// 浮動小数点値を取得
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>取得された値</returns>
        Task<float> GetFloatAsync(string key, float defaultValue = 0f);

        /// <summary>
        /// ブール値を保存
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>保存が成功したかどうか</returns>
        Task<bool> SetBoolAsync(string key, bool value);

        /// <summary>
        /// ブール値を取得
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>取得された値</returns>
        Task<bool> GetBoolAsync(string key, bool defaultValue = false);

        /// <summary>
        /// JSONシリアライズ可能なオブジェクトを保存
        /// </summary>
        /// <typeparam name="T">オブジェクトの型</typeparam>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <returns>保存が成功したかどうか</returns>
        Task<bool> SetObjectAsync<T>(string key, T value) where T : class;

        /// <summary>
        /// JSONデシリアライズ可能なオブジェクトを取得
        /// </summary>
        /// <typeparam name="T">オブジェクトの型</typeparam>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>取得された値</returns>
        Task<T> GetObjectAsync<T>(string key, T defaultValue = default) where T : class;

        /// <summary>
        /// キーが存在するかチェック
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>存在するかどうか</returns>
        Task<bool> HasKeyAsync(string key);

        /// <summary>
        /// キーを削除
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>削除が成功したかどうか</returns>
        Task<bool> DeleteKeyAsync(string key);

        /// <summary>
        /// 指定したプレフィックスを持つ全てのキーを削除
        /// </summary>
        /// <param name="prefix">プレフィックス</param>
        /// <returns>削除されたキーの数</returns>
        Task<int> DeleteKeysWithPrefixAsync(string prefix);

        /// <summary>
        /// 全ての設定をクリア
        /// </summary>
        /// <returns>クリアが成功したかどうか</returns>
        Task<bool> ClearAllAsync();

        /// <summary>
        /// 設定のバックアップを作成
        /// </summary>
        /// <param name="backupName">バックアップ名</param>
        /// <returns>バックアップが成功したかどうか</returns>
        Task<bool> CreateBackupAsync(string backupName);

        /// <summary>
        /// バックアップから復元
        /// </summary>
        /// <param name="backupName">バックアップ名</param>
        /// <returns>復元が成功したかどうか</returns>
        Task<bool> RestoreFromBackupAsync(string backupName);

        /// <summary>
        /// 設定の統計情報を取得
        /// </summary>
        /// <returns>統計情報</returns>
        Task<PreferencesStats> GetStatsAsync();
    }

    /// <summary>
    /// 設定統計情報を表現する値オブジェクト
    /// </summary>
    public readonly struct PreferencesStats
    {
        public readonly int TotalKeys;
        public readonly long TotalStorageSize;
        public readonly DateTime LastModified;
        public readonly int BackupCount;

        public PreferencesStats(int totalKeys, long totalStorageSize, DateTime lastModified, int backupCount)
        {
            TotalKeys = totalKeys;
            TotalStorageSize = totalStorageSize;
            LastModified = lastModified;
            BackupCount = backupCount;
        }

        public override string ToString()
        {
            return $"PreferencesStats(Keys: {TotalKeys}, Size: {TotalStorageSize} bytes, LastModified: {LastModified:yyyy-MM-dd HH:mm:ss}, Backups: {BackupCount})";
        }
    }
}