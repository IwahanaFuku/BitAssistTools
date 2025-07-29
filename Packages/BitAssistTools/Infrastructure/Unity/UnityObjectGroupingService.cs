using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Services;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// Unity Editor APIを使用したオブジェクトグループ化サービスの実装
    /// 元のCommonFunctions.ParentCenterAligner()と同等の機能を提供
    /// </summary>
    public class UnityObjectGroupingService : IObjectGroupingService
    {
        /// <summary>
        /// 選択されたTransform配列を親の中央に配置するグループを作成
        /// </summary>
        /// <param name="transforms">グループ化するTransform配列</param>
        public void CreateParentCenterGroup(Transform[] transforms)
        {
            if (transforms == null || transforms.Length == 0) return;

            // null要素を除外
            var validTransforms = transforms.Where(t => t != null).ToArray();
            if (validTransforms.Length == 0) return;

            // 元実装のCommonFunctions.ParentCenterAligner()ロジック
            int firstIndex = validTransforms.Min(t => t.GetSiblingIndex());

            // 中心点計算
            Vector3 center = Vector3.zero;
            foreach (Transform transform in validTransforms)
            {
                center += transform.position;
            }
            center /= validTransforms.Length;

            // 親の座標系に変換
            var parent = Selection.activeTransform?.parent;
            if (parent != null)
            {
                center = parent.InverseTransformPoint(center);
            }

            // グループ作成
            var group = new GameObject("Group");
            group.transform.position = center;
            Undo.RegisterCreatedObjectUndo(group, "Group Selected");
            group.transform.SetSiblingIndex(firstIndex);

            // 親設定とグループ化
            group.transform.SetParent(parent, false);
            foreach (var transform in validTransforms.OrderBy(t => t.GetSiblingIndex()))
            {
                Undo.SetTransformParent(transform, group.transform, "Group Selected");
            }
            Selection.activeGameObject = group;
        }
    }
}