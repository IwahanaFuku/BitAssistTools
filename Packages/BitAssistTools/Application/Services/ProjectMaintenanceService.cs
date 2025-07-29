using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.ProjectMaintenance;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// プロジェクトメンテナンス操作のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class ProjectMaintenanceService
    {
        /// <summary>
        /// 選択されたオブジェクトからMissing Scriptを削除（DDDベース、Undo対応）
        /// </summary>
        public static void RemoveMissingScriptsFromSelected()
        {
            try
            {
                Undo.SetCurrentGroupName("Remove Missing Scripts");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedGameObjects = UnityEditor.Selection.gameObjects;
                if (selectedGameObjects == null || selectedGameObjects.Length == 0)
                {
                    Debug.LogError("Missing Scriptを削除するGameObjectを選択してください。");
                    return;
                }

                var allChildren = selectedGameObjects
                    .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
                    .Select(t => t.gameObject);

                var processedPrefabs = new HashSet<UnityEngine.Object>();
                int removedComponentCount = 0;
                int affectedGameObjectCount = 0;

                foreach (var gameObject in allChildren)
                {
                    int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

                    if (missingScriptCount > 0)
                    {
                        // Prefabの場合はソースも処理
                        if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
                        {
                            ProcessMissingScriptsInPrefabSource(
                                gameObject, 
                                processedPrefabs, 
                                ref removedComponentCount, 
                                ref affectedGameObjectCount
                            );
                            
                            // 再チェック（Prefabソース処理で解決された可能性）
                            missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                            if (missingScriptCount == 0) continue;
                        }

                        // インスタンス自体の処理
                        Undo.RegisterCompleteObjectUndo(gameObject, "Remove Missing Scripts");
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                        removedComponentCount += missingScriptCount;
                        affectedGameObjectCount++;
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
                
                var result = new CleanupResult(
                    CleanupOperationType.MissingScripts,
                    affectedGameObjectCount,
                    removedComponentCount,
                    processedPrefabs.Count
                );

            }
            catch (Exception ex)
            {
                Debug.LogError($"Missing Script削除中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Missing Script削除が実行可能かどうかを判定
        /// </summary>
        public static bool CanRemoveMissingScripts()
        {
            var selectedObjects = UnityEditor.Selection.gameObjects;
            return selectedObjects != null && selectedObjects.Length > 0;
        }

        /// <summary>
        /// 選択されたオブジェクトのMissing Script統計を取得
        /// </summary>
        public static MissingScriptStats GetMissingScriptStats()
        {
            var selectedGameObjects = UnityEditor.Selection.gameObjects;
            if (selectedGameObjects == null || selectedGameObjects.Length == 0)
            {
                return new MissingScriptStats(0, 0, 0);
            }

            var allChildren = selectedGameObjects
                .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToArray();

            int totalMissingScripts = 0;
            int affectedGameObjects = 0;
            int prefabInstances = 0;

            foreach (var gameObject in allChildren)
            {
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (missingCount > 0)
                {
                    totalMissingScripts += missingCount;
                    affectedGameObjects++;
                    
                    if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
                    {
                        prefabInstances++;
                    }
                }
            }

            return new MissingScriptStats(
                allChildren.Length,
                affectedGameObjects,
                totalMissingScripts,
                prefabInstances
            );
        }

        /// <summary>
        /// Prefabソース内のMissing Scriptを処理
        /// </summary>
        private static void ProcessMissingScriptsInPrefabSource(
            GameObject instance,
            HashSet<UnityEngine.Object> processedPrefabs,
            ref int componentCount,
            ref int gameObjectCount)
        {
            var sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            if (sourcePrefab == null || !processedPrefabs.Add(sourcePrefab))
                return;

            // 再帰的にソースPrefabも処理
            ProcessMissingScriptsInPrefabSource(sourcePrefab, processedPrefabs, ref componentCount, ref gameObjectCount);

            int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(sourcePrefab);
            if (missingScriptCount > 0)
            {
                Undo.RegisterCompleteObjectUndo(sourcePrefab, "Remove Missing Scripts From Prefab");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(sourcePrefab);
                componentCount += missingScriptCount;
                gameObjectCount++;
            }
        }

        /// <summary>
        /// クリーンアップ結果
        /// </summary>
        public readonly struct CleanupResult
        {
            public readonly CleanupOperationType OperationType;
            public readonly int AffectedObjects;
            public readonly int RemovedItems;
            public readonly int ProcessedPrefabs;

            public CleanupResult(CleanupOperationType operationType, int affectedObjects, int removedItems, int processedPrefabs = 0)
            {
                OperationType = operationType;
                AffectedObjects = affectedObjects;
                RemovedItems = removedItems;
                ProcessedPrefabs = processedPrefabs;
            }

            public override string ToString()
            {
                return $"{AffectedObjects}個のGameObjectから{RemovedItems}個のMissing Scriptを削除" +
                       (ProcessedPrefabs > 0 ? $" (Prefab: {ProcessedPrefabs}個処理)" : "");
            }
        }

        /// <summary>
        /// Missing Script統計情報
        /// </summary>
        public readonly struct MissingScriptStats
        {
            public readonly int TotalGameObjects;
            public readonly int AffectedGameObjects;
            public readonly int TotalMissingScripts;
            public readonly int PrefabInstances;

            public MissingScriptStats(int totalGameObjects, int affectedGameObjects, int totalMissingScripts, int prefabInstances = 0)
            {
                TotalGameObjects = totalGameObjects;
                AffectedGameObjects = affectedGameObjects;
                TotalMissingScripts = totalMissingScripts;
                PrefabInstances = prefabInstances;
            }

            public bool HasMissingScripts => TotalMissingScripts > 0;

            public override string ToString()
            {
                if (!HasMissingScripts) return "Missing Scriptなし";
                
                string result = $"{AffectedGameObjects}/{TotalGameObjects}個のオブジェクトに{TotalMissingScripts}個のMissing Script";
                if (PrefabInstances > 0) result += $" (Prefab: {PrefabInstances}個)";
                return result;
            }
        }
    }
}