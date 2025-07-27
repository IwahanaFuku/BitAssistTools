namespace BitAssistTools.Domain.Models.ProjectMaintenance
{
    /// <summary>
    /// プロジェクトクリーンアップ操作の種類を表現する列挙型
    /// </summary>
    public enum CleanupOperationType
    {
        /// <summary>Missing Scriptコンポーネントの削除</summary>
        MissingScripts,
        
        /// <summary>空のGameObjectの削除</summary>
        EmptyGameObjects,
        
        /// <summary>未使用アセットの削除</summary>
        UnusedAssets
    }
}