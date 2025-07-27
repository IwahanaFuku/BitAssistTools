using UnityEngine;

namespace BitAssistTools.Domain.Services
{
    /// <summary>
    /// オブジェクトのグループ化を担当するドメインサービス
    /// Unity固有の実装から抽象化されたインターフェース
    /// </summary>
    public interface IObjectGroupingService
    {
        /// <summary>
        /// 選択されたTransform配列を親の中央に配置するグループを作成
        /// 元実装のCommonFunctions.ParentCenterAligner()と同等の機能
        /// </summary>
        /// <param name="transforms">グループ化するTransform配列</param>
        void CreateParentCenterGroup(Transform[] transforms);
    }
}