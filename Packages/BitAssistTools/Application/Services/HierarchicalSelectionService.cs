using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// 階層選択のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class HierarchicalSelectionService
    {
        /// <summary>
        /// 階層選択を実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteHierarchicalSelection(HierarchicalSelectionRule rule)
        {
            try
            {
                Undo.SetCurrentGroupName("Hierarchical Selection");
                int undoGroup = Undo.GetCurrentGroup();

                var activeGameObject = Selection.activeGameObject;
                if (activeGameObject == null)
                {
                    Debug.LogError("アクティブなGameObjectを選択してください。");
                    return;
                }

                if (!rule.CanApplyTo(activeGameObject))
                {
                    Debug.LogError($"選択されたオブジェクトに階層選択を適用できません。親階層数: {rule.ParentDepth}");
                    return;
                }

                // 階層選択の実行
                var targetObjects = GetHierarchicalSelection(activeGameObject, rule);

                if (targetObjects.Length == 0)
                {
                    Debug.LogWarning("階層選択の条件に一致するオブジェクトがありません。");
                    return;
                }

                Selection.objects = targetObjects;

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"階層選択中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 階層選択ルールに基づいてオブジェクトを取得
        /// </summary>
        private static GameObject[] GetHierarchicalSelection(GameObject baseObject, HierarchicalSelectionRule rule)
        {
            // 基準オブジェクトの情報を取得
            int targetDepth = rule.GetDepth(baseObject);
            int objectIndex = baseObject.transform.GetSiblingIndex();
            var parentObject = rule.GetParentObject(baseObject);

            // 同階層のオブジェクトを取得
            var sameDepthObjects = GetSameDepthObjects(parentObject, rule.ParentDepth + 1, targetDepth);

            // 順番フィルタリング
            GameObject[] filteredObjects;
            if (rule.OnlyOrder)
            {
                filteredObjects = FilterBySameOrder(sameDepthObjects, objectIndex);
            }
            else
            {
                filteredObjects = sameDepthObjects;
            }

            // 子選択フィルタリング
            if (rule.SelectChildren)
            {
                return GetAllChildren(filteredObjects);
            }
            else
            {
                return filteredObjects;
            }
        }

        /// <summary>
        /// 指定した親オブジェクトから指定した深度のオブジェクトを取得
        /// </summary>
        private static GameObject[] GetSameDepthObjects(GameObject parentObject, int childDepth, int targetDepth)
        {
            if (childDepth == 0)
            {
                return new GameObject[] { parentObject };
            }

            var childObjects = new List<GameObject>();
            GetChildObjectsRecursive(parentObject, 0, childDepth, ref childObjects);

            return childObjects
                .Where(obj => GetDepth(obj) == targetDepth)
                .ToArray();
        }

        /// <summary>
        /// 再帰的に子オブジェクトを取得
        /// </summary>
        private static void GetChildObjectsRecursive(GameObject parentObject, int currentDepth, int maxDepth, ref List<GameObject> childObjects)
        {
            foreach (Transform childTransform in parentObject.transform)
            {
                var childObject = childTransform.gameObject;
                childObjects.Add(childObject);

                if (currentDepth < maxDepth)
                {
                    GetChildObjectsRecursive(childObject, currentDepth + 1, maxDepth, ref childObjects);
                }
            }
        }

        /// <summary>
        /// 同じ順番（sibling index）のオブジェクトのみをフィルタリング
        /// </summary>
        private static GameObject[] FilterBySameOrder(GameObject[] objects, int targetIndex)
        {
            var filteredObjects = new List<GameObject>();

            foreach (var obj in objects)
            {
                if (obj.transform.parent != null && obj.transform.parent.childCount > targetIndex)
                {
                    var siblingAtIndex = obj.transform.parent.GetChild(targetIndex);
                    if (siblingAtIndex != null)
                    {
                        filteredObjects.Add(siblingAtIndex.gameObject);
                    }
                }
            }

            return filteredObjects.ToArray();
        }

        /// <summary>
        /// 指定したオブジェクト配列の全ての子オブジェクトを取得
        /// </summary>
        private static GameObject[] GetAllChildren(GameObject[] parentObjects)
        {
            var allChildren = new List<GameObject>();

            foreach (var parentObj in parentObjects)
            {
                foreach (Transform childTransform in parentObj.transform)
                {
                    allChildren.Add(childTransform.gameObject);
                }
            }

            return allChildren.ToArray();
        }

        /// <summary>
        /// GameObjectの階層の深さを取得
        /// </summary>
        private static int GetDepth(GameObject obj)
        {
            if (obj == null) return 0;

            int depth = 0;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                depth++;
                parent = parent.parent;
            }
            return depth;
        }

        /// <summary>
        /// 階層選択が可能かどうかを事前チェック
        /// </summary>
        public static bool CanExecuteHierarchicalSelection(HierarchicalSelectionRule rule)
        {
            var activeGameObject = Selection.activeGameObject;
            return activeGameObject != null && rule.CanApplyTo(activeGameObject);
        }

        /// <summary>
        /// 現在の選択に対する階層選択の結果をプレビュー（実際の選択は変更しない）
        /// </summary>
        public static GameObject[] PreviewHierarchicalSelection(HierarchicalSelectionRule rule)
        {
            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject == null || !rule.CanApplyTo(activeGameObject))
            {
                return new GameObject[0];
            }

            return GetHierarchicalSelection(activeGameObject, rule);
        }
    }
}