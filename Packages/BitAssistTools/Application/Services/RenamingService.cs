using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Renaming;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// リネーミング操作のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class RenamingService
    {
        /// <summary>
        /// 接頭辞・接尾辞を追加（DDDベース、Undo対応）
        /// </summary>
        public static void AddPrefixAndSuffix(PrefixSuffixRule rule)
        {
            try
            {
                if (!rule.IsValid())
                {
                    Debug.LogError($"無効な接頭辞・接尾辞ルールです: {rule}");
                    return;
                }

                Undo.SetCurrentGroupName("Add Prefix And Suffix");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("リネームするGameObjectを選択してください。");
                    return;
                }

                foreach (var obj in selectedObjects)
                {
                    Undo.RecordObject(obj, "Add Prefix And Suffix");
                    obj.name = rule.ApplyTo(obj.name);
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"接頭辞・接尾辞追加中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 文字列を置換（DDDベース、Undo対応）
        /// </summary>
        public static void ReplaceString(StringReplaceRule rule)
        {
            try
            {
                if (!rule.IsValid())
                {
                    Debug.LogError($"無効な文字列置換ルールです: {rule}");
                    return;
                }

                Undo.SetCurrentGroupName("Replace String");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("リネームするGameObjectを選択してください。");
                    return;
                }

                int changedCount = 0;
                foreach (var obj in selectedObjects)
                {
                    string originalName = obj.name;
                    string newName = rule.ApplyTo(originalName);
                    
                    if (originalName != newName)
                    {
                        Undo.RecordObject(obj, "Replace String");
                        obj.name = newName;
                        changedCount++;
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"文字列置換中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// マテリアルをテクスチャ名でリネーム（DDDベース、Undo対応）
        /// </summary>
        public static void RenameToTextureName()
        {
            try
            {
                Undo.SetCurrentGroupName("Rename To Texture Name");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.objects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("リネームするMaterialを選択してください。");
                    return;
                }

                int renamedCount = 0;
                foreach (var obj in selectedObjects)
                {
                    if (obj is Material material)
                    {
                        var albedoTexture = material.GetTexture("_MainTex");
                        if (albedoTexture != null)
                        {
                            string textureName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(albedoTexture));
                            if (!string.IsNullOrEmpty(textureName))
                            {
                                string materialPath = AssetDatabase.GetAssetPath(material);
                                AssetDatabase.RenameAsset(materialPath, textureName);
                                renamedCount++;
                            }
                        }
                    }
                }

                if (renamedCount > 0)
                {
                    AssetDatabase.SaveAssets();
                    Undo.CollapseUndoOperations(undoGroup);
                }
                else
                {
                    Debug.LogWarning("リネーム可能なマテリアルが見つかりませんでした。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"テクスチャ名リネーム中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// オブジェクトをマテリアル名でリネーム（DDDベース、Undo対応）
        /// </summary>
        public static void RenameToMaterialName()
        {
            try
            {
                Undo.SetCurrentGroupName("Rename To Material Name");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("リネームするGameObjectを選択してください。");
                    return;
                }

                int renamedCount = 0;
                foreach (var obj in selectedObjects)
                {
                    var renderer = obj.GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        Undo.RecordObject(obj, "Rename To Material Name");
                        obj.name = renderer.sharedMaterial.name;
                        renamedCount++;
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"マテリアル名リネーム中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// オブジェクトに番号を付与（DDDベース、Undo対応）
        /// </summary>
        public static void AddObjectNumber()
        {
            try
            {
                Undo.SetCurrentGroupName("Add Object Number");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("番号を付与するGameObjectを選択してください。");
                    return;
                }

                // 深度とシブリングインデックスでソート
                System.Array.Sort(selectedObjects, (x, y) =>
                {
                    int depthDifference = GetDepth(x.transform) - GetDepth(y.transform);
                    if (depthDifference != 0)
                        return depthDifference;
                    return x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex());
                });

                var nameCounters = new Dictionary<string, int>();
                foreach (var obj in selectedObjects)
                {
                    Undo.RecordObject(obj, "Add Object Number");
                    
                    string baseName = obj.name;
                    if (nameCounters.ContainsKey(baseName))
                    {
                        nameCounters[baseName]++;
                        obj.name = $"{baseName} ({nameCounters[baseName]})";
                    }
                    else
                    {
                        nameCounters[baseName] = 1;
                        obj.name = $"{baseName} (1)";
                    }
                }

                UnityEditor.Selection.objects = selectedObjects;
                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"番号付与中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// オブジェクトから番号を除去（DDDベース、Undo対応）
        /// </summary>
        public static void RemoveObjectNumber()
        {
            try
            {
                Undo.SetCurrentGroupName("Remove Object Number");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("番号を除去するGameObjectを選択してください。");
                    return;
                }

                int removedCount = 0;
                foreach (var obj in selectedObjects)
                {
                    string originalName = obj.name;
                    string newName = Regex.Replace(originalName, @"\s*\(\d+\)$", "");
                    
                    if (originalName != newName)
                    {
                        Undo.RecordObject(obj, "Remove Object Number");
                        obj.name = newName;
                        removedCount++;
                    }
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"番号除去中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Prefabアセットをリネーム（DDDベース、Undo対応）
        /// </summary>
        public static void RenameSelectedPrefabAsset()
        {
            try
            {
                Undo.SetCurrentGroupName("Rename Prefab Asset");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("Prefabインスタンスを選択してください。");
                    return;
                }

                int renamedCount = 0;
                foreach (var obj in selectedObjects)
                {
                    string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        AssetDatabase.RenameAsset(assetPath, obj.name);
                        renamedCount++;
                    }
                }

                if (renamedCount > 0)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Undo.CollapseUndoOperations(undoGroup);
                }
                else
                {
                    Debug.LogWarning("リネーム可能なPrefabが見つかりませんでした。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Prefabアセットリネーム中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 各操作が実行可能かどうかを判定
        /// </summary>
        public static bool CanExecuteOperation(RenameOperationType operationType)
        {
            var selectedObjects = UnityEditor.Selection.objects;
            if (selectedObjects == null || selectedObjects.Length == 0)
                return false;

            switch (operationType)
            {
                case RenameOperationType.PrefixSuffix:
                case RenameOperationType.StringReplace:
                case RenameOperationType.AddNumber:
                case RenameOperationType.RemoveNumber:
                    return UnityEditor.Selection.gameObjects.Length > 0;
                
                case RenameOperationType.TextureName:
                    return selectedObjects.OfType<Material>().Any();
                
                case RenameOperationType.MaterialName:
                    return UnityEditor.Selection.gameObjects.Any(obj => {
                        var renderer = obj.GetComponent<Renderer>();
                        return renderer != null && renderer.sharedMaterial != null;
                    });
                
                case RenameOperationType.PrefabAsset:
                    return UnityEditor.Selection.gameObjects.Any(obj => !string.IsNullOrEmpty(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj)));
                
                default:
                    return false;
            }
        }

        /// <summary>
        /// 選択されたオブジェクトの統計を取得
        /// </summary>
        public static RenamingStats GetRenamingStats()
        {
            var gameObjects = UnityEditor.Selection.gameObjects;
            var materials = UnityEditor.Selection.objects.OfType<Material>().ToArray();
            var prefabs = gameObjects.Where(obj => !string.IsNullOrEmpty(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj))).ToArray();
            var objectsWithRenderer = gameObjects.Where(obj => {
                var renderer = obj.GetComponent<Renderer>();
                return renderer != null && renderer.sharedMaterial != null;
            }).ToArray();

            return new RenamingStats(
                gameObjects.Length,
                materials.Length,
                prefabs.Length,
                objectsWithRenderer.Length
            );
        }

        /// <summary>
        /// Transform の深度を取得
        /// </summary>
        private static int GetDepth(Transform transform)
        {
            int depth = 0;
            while (transform.parent != null)
            {
                depth++;
                transform = transform.parent;
            }
            return depth;
        }

        /// <summary>
        /// リネーミング統計情報
        /// </summary>
        public readonly struct RenamingStats
        {
            public readonly int GameObjectCount;
            public readonly int MaterialCount;
            public readonly int PrefabCount;
            public readonly int RendererObjectCount;

            public RenamingStats(int gameObjectCount, int materialCount, int prefabCount, int rendererObjectCount)
            {
                GameObjectCount = gameObjectCount;
                MaterialCount = materialCount;
                PrefabCount = prefabCount;
                RendererObjectCount = rendererObjectCount;
            }

            public bool HasSelection => GameObjectCount > 0 || MaterialCount > 0;

            public override string ToString()
            {
                if (!HasSelection) return "何も選択されていません";
                
                var parts = new List<string>();
                if (GameObjectCount > 0) parts.Add($"GameObject: {GameObjectCount}個");
                if (MaterialCount > 0) parts.Add($"Material: {MaterialCount}個");
                if (PrefabCount > 0) parts.Add($"Prefab: {PrefabCount}個");
                if (RendererObjectCount > 0) parts.Add($"Renderer付き: {RendererObjectCount}個");
                
                return string.Join(", ", parts);
            }
        }
    }
}