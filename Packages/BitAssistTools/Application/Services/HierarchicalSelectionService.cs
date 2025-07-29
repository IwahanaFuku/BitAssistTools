using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Selection;
using UnitySelection = UnityEditor.Selection;

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
        /// 複数選択に対応
        /// </summary>
        public static void ExecuteHierarchicalSelection(HierarchicalSelectionRule rule)
        {
            try
            {
                Undo.SetCurrentGroupName("Hierarchical Selection");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedGameObjects = UnitySelection.gameObjects;
                if (selectedGameObjects == null || selectedGameObjects.Length == 0)
                {
                    Debug.LogError("GameObjectを選択してください。");
                    return;
                }

                var allTargetObjects = new List<GameObject>();

                // 各選択オブジェクトに対して階層選択を実行
                foreach (var selectedObject in selectedGameObjects)
                {
                    if (!rule.CanApplyTo(selectedObject))
                    {
                        Debug.LogWarning($"オブジェクト '{selectedObject.name}' に階層選択を適用できません。親階層数: {rule.ParentDepth}");
                        continue;
                    }

                    var targetObjects = GetHierarchicalSelection(selectedObject, rule);
                    allTargetObjects.AddRange(targetObjects);
                }

                // 重複を除去
                var uniqueTargetObjects = allTargetObjects.Distinct().ToArray();

                if (uniqueTargetObjects.Length == 0)
                {
                    Debug.LogWarning("階層選択の条件に一致するオブジェクトがありません。");
                    return;
                }

                UnitySelection.objects = uniqueTargetObjects;

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
            // 基準となる親オブジェクトを取得（N階層上）
            var baseParentObject = rule.GetParentObject(baseObject);
            
            // 基準親の兄弟オブジェクトを取得（基準親と同じ階層のオブジェクト）
            var baseParentSiblings = new List<GameObject>();
            
            if (baseParentObject.transform.parent != null)
            {
                // 基準親の兄弟オブジェクトをすべて取得
                foreach (Transform sibling in baseParentObject.transform.parent)
                {
                    baseParentSiblings.Add(sibling.gameObject);
                }
            }
            else
            {
                // 基準親がルートの場合、そのオブジェクト自身のみが対象
                baseParentSiblings.Add(baseParentObject);
            }

            // 「同じ順番」オプションの処理
            GameObject[] targetObjects;
            if (rule.OnlyOrder)
            {
                // 選択したオブジェクトと同じシーン上の順番の子オブジェクトを各兄弟から選択
                targetObjects = GetSameOrderDescendants(baseObject, baseParentSiblings.ToArray(), rule.ParentDepth);
            }
            else
            {
                // 全ての兄弟オブジェクトを選択
                targetObjects = baseParentSiblings.ToArray();
            }

            // 「子オブジェクトをすべて選択」オプションの処理
            if (rule.SelectChildren)
            {
                // ターゲットの子オブジェクトを選択（ターゲット自体は選択しない）
                return GetAllChildren(targetObjects);
            }
            else
            {
                // ターゲット自体を選択
                return targetObjects;
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
        /// 基準オブジェクトと同じシーン上の順番を持つ子孫オブジェクトを各基準親兄弟から取得
        /// </summary>
        private static GameObject[] GetSameOrderDescendants(GameObject baseObject, GameObject[] baseParentSiblings, int parentDepth)
        {
            var targetObjects = new List<GameObject>();
            
            // 基準オブジェクトから基準親までのsibling indexのパスを取得
            var siblingIndexPath = GetSiblingIndexPath(baseObject, parentDepth);
            
            // 各基準親兄弟に対して、同じsibling indexパスを辿って対応する子孫を取得
            foreach (var parentSibling in baseParentSiblings)
            {
                var descendant = FindDescendantBySiblingPath(parentSibling, siblingIndexPath);
                if (descendant != null)
                {
                    targetObjects.Add(descendant);
                }
            }
            
            return targetObjects.ToArray();
        }
        
        /// <summary>
        /// オブジェクトから指定階層数上の親までのsibling indexのパスを取得
        /// </summary>
        private static int[] GetSiblingIndexPath(GameObject obj, int parentDepth)
        {
            var path = new int[parentDepth];
            var current = obj.transform;
            
            // 下位から上位へのパスを作成（逆順）
            for (int i = parentDepth - 1; i >= 0; i--)
            {
                if (current != null)
                {
                    path[i] = current.GetSiblingIndex();
                    current = current.parent;
                }
                else
                {
                    path[i] = 0; // パスが途切れた場合は0とする
                }
            }
            
            return path;
        }
        
        /// <summary>
        /// 指定されたsibling indexパスを辿って子孫オブジェクトを取得
        /// </summary>
        private static GameObject FindDescendantBySiblingPath(GameObject root, int[] siblingIndexPath)
        {
            var current = root.transform;
            
            // パスを辿って子孫を探索
            foreach (var siblingIndex in siblingIndexPath)
            {
                if (current.childCount <= siblingIndex)
                {
                    return null; // 指定されたindexの子が存在しない
                }
                current = current.GetChild(siblingIndex);
            }
            
            return current.gameObject;
        }

        /// <summary>
        /// 同じ順番（sibling index）のオブジェクトのみをフィルタリング
        /// </summary>
        private static GameObject[] FilterBySameOrder(GameObject[] objects, int targetIndex)
        {
            var filteredObjects = new List<GameObject>();

            foreach (var obj in objects)
            {
                if (obj.transform.GetSiblingIndex() == targetIndex)
                {
                    filteredObjects.Add(obj);
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
        /// 複数選択に対応
        /// </summary>
        public static bool CanExecuteHierarchicalSelection(HierarchicalSelectionRule rule)
        {
            var selectedGameObjects = UnitySelection.gameObjects;
            if (selectedGameObjects == null || selectedGameObjects.Length == 0)
                return false;

            // 少なくとも1つのオブジェクトが条件を満たす場合は実行可能
            return selectedGameObjects.Any(obj => rule.CanApplyTo(obj));
        }

        /// <summary>
        /// 現在の選択に対する階層選択の結果をプレビュー（実際の選択は変更しない）
        /// </summary>
        public static GameObject[] PreviewHierarchicalSelection(HierarchicalSelectionRule rule)
        {
            var activeGameObject = UnitySelection.activeGameObject;
            if (activeGameObject == null || !rule.CanApplyTo(activeGameObject))
            {
                return new GameObject[0];
            }

            return GetHierarchicalSelection(activeGameObject, rule);
        }

        /// <summary>
        /// 詳細なプレビュー情報を取得（実際の選択は変更しない）
        /// 複数選択に対応
        /// </summary>
        public static HierarchicalSelectionPreview GetDetailedPreview(HierarchicalSelectionRule rule)
        {
            try
            {
                var selectedGameObjects = UnitySelection.gameObjects;
                if (selectedGameObjects == null || selectedGameObjects.Length == 0)
                {
                    return HierarchicalSelectionPreview.Invalid("GameObjectを選択してください。");
                }

                var allBaseParents = new List<GameObject>();
                var allParentSiblings = new List<GameObject>();
                var allTargetObjects = new List<GameObject>();
                var validSelectionCount = 0;

                // 各選択オブジェクトに対してプレビューを生成
                foreach (var selectedObject in selectedGameObjects)
                {
                    if (!rule.CanApplyTo(selectedObject))
                    {
                        continue;
                    }

                    validSelectionCount++;

                    // 基準親とその兄弟を取得
                    var baseParent = rule.GetParentObject(selectedObject);
                    var parentSiblings = GetParentSiblings(baseParent);
                    
                    // 選択対象を取得
                    var targetObjects = GetHierarchicalSelection(selectedObject, rule);

                    // リストに追加（重複は後で除去）
                    if (!allBaseParents.Contains(baseParent))
                        allBaseParents.Add(baseParent);
                    
                    allParentSiblings.AddRange(parentSiblings);
                    allTargetObjects.AddRange(targetObjects);
                }

                if (validSelectionCount == 0)
                {
                    return HierarchicalSelectionPreview.Invalid(
                        $"選択されたオブジェクトに階層選択を適用できません。{rule.ParentDepth}階層上の親が必要です。");
                }

                // 重複を除去
                var uniqueParentSiblings = allParentSiblings.Distinct().ToArray();
                var uniqueTargetObjects = allTargetObjects.Distinct().ToArray();
                
                // 選択理由を生成（複数選択対応）
                var selectionReason = GenerateMultiSelectionReason(selectedGameObjects, rule, validSelectionCount, uniqueTargetObjects);

                // 複数の基準親がある場合は最初のものを代表として使用
                var representativeBaseParent = allBaseParents.FirstOrDefault();

                return HierarchicalSelectionPreview.Valid(
                    representativeBaseParent,
                    uniqueParentSiblings,
                    uniqueTargetObjects,
                    selectionReason
                );
            }
            catch (Exception ex)
            {
                return HierarchicalSelectionPreview.Invalid($"プレビュー生成中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 基準親の兄弟オブジェクトを取得
        /// </summary>
        private static GameObject[] GetParentSiblings(GameObject baseParent)
        {
            var siblings = new List<GameObject>();
            
            if (baseParent.transform.parent != null)
            {
                // 基準親の兄弟オブジェクトをすべて取得
                foreach (Transform sibling in baseParent.transform.parent)
                {
                    siblings.Add(sibling.gameObject);
                }
            }
            else
            {
                // 基準親がルートの場合、そのオブジェクト自身のみが対象
                siblings.Add(baseParent);
            }
            
            return siblings.ToArray();
        }

        /// <summary>
        /// 選択理由の説明文を生成
        /// </summary>
        private static string GenerateSelectionReason(GameObject baseObject, HierarchicalSelectionRule rule, GameObject[] parentSiblings, GameObject[] targetObjects)
        {
            var reasonBuilder = new System.Text.StringBuilder();
            
            // 基本情報
            reasonBuilder.Append($"{rule.ParentDepth}階層上の親『{rule.GetParentObject(baseObject).name}』の");
            
            if (rule.OnlyOrder)
            {
                // 同じ順番の場合
                var siblingIndex = baseObject.transform.GetSiblingIndex();
                reasonBuilder.Append($"各兄弟から同じ順番（{siblingIndex}番目）の");
                
                if (rule.ParentDepth > 1)
                {
                    reasonBuilder.Append("子孫オブジェクト");
                }
                else
                {
                    reasonBuilder.Append("子オブジェクト");
                }
            }
            else
            {
                // 全選択の場合
                reasonBuilder.Append("全ての兄弟オブジェクト");
            }
            
            if (rule.SelectChildren)
            {
                reasonBuilder.Append("の子オブジェクト");
            }
            
            reasonBuilder.Append("を選択");

            // 詳細情報を追加
            if (targetObjects.Length == 0)
            {
                reasonBuilder.Append("（該当するオブジェクトが見つかりませんでした）");
            }
            else if (targetObjects.Length != parentSiblings.Length && rule.OnlyOrder)
            {
                var missingCount = parentSiblings.Length - targetObjects.Length;
                reasonBuilder.Append($"（{missingCount}個のオブジェクトで該当する子が見つかりませんでした）");
            }

            return reasonBuilder.ToString();
        }

        /// <summary>
        /// 複数選択に対応した選択理由の説明文を生成
        /// </summary>
        private static string GenerateMultiSelectionReason(GameObject[] selectedObjects, HierarchicalSelectionRule rule, int validSelectionCount, GameObject[] targetObjects)
        {
            var reasonBuilder = new System.Text.StringBuilder();
            
            // 複数選択の基本情報
            if (selectedObjects.Length > 1)
            {
                reasonBuilder.Append($"{selectedObjects.Length}個のオブジェクト選択中（");
                
                if (validSelectionCount == selectedObjects.Length)
                {
                    reasonBuilder.Append("全て適用可能");
                }
                else
                {
                    reasonBuilder.Append($"{validSelectionCount}個が適用可能");
                }
                
                reasonBuilder.Append("）: ");
            }
            
            // 基本的な選択ロジック
            reasonBuilder.Append($"{rule.ParentDepth}階層上の親の");
            
            if (rule.OnlyOrder)
            {
                reasonBuilder.Append("各兄弟から同じ順番の");
                
                if (rule.ParentDepth > 1)
                {
                    reasonBuilder.Append("子孫オブジェクト");
                }
                else
                {
                    reasonBuilder.Append("子オブジェクト");
                }
            }
            else
            {
                reasonBuilder.Append("全ての兄弟オブジェクト");
            }
            
            if (rule.SelectChildren)
            {
                reasonBuilder.Append("の子オブジェクト");
            }
            
            reasonBuilder.Append("を選択");

            // 結果の詳細情報
            if (targetObjects.Length == 0)
            {
                reasonBuilder.Append("（該当するオブジェクトが見つかりませんでした）");
            }
            else if (selectedObjects.Length > 1)
            {
                reasonBuilder.Append($"（合計{targetObjects.Length}個のオブジェクト）");
                
                if (validSelectionCount < selectedObjects.Length)
                {
                    var skippedCount = selectedObjects.Length - validSelectionCount;
                    reasonBuilder.Append($"（{skippedCount}個のオブジェクトは親階層不足のためスキップ）");
                }
            }

            return reasonBuilder.ToString();
        }
    }
}