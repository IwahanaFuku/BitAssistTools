using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Domain.Services;
using BitAssistTools.Infrastructure.Unity;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// オブジェクト配置・複製・入れ替えのアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class ObjectPlacementService
    {
        // 依存性注入：グループ化サービス
        private static readonly IObjectGroupingService _groupingService = new UnityObjectGroupingService();
        /// <summary>
        /// 特殊複製を実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteSpecialDuplicate(CoordinateSystem coordinate, TransformData offset, int copyCount)
        {
            if (copyCount < 1) return;

            try
            {
                Undo.SetCurrentGroupName("Special Duplicate");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedTransforms = Selection.transforms;
                foreach (var selectedTransform in selectedTransforms)
                {
                    // 複利効果のため、累積的に更新するTransformData
                    var currentTransformData = TransformData.FromTransform(selectedTransform);
                    var copyTransforms = new List<Transform> { selectedTransform };

                    for (int i = 0; i < copyCount; i++)
                    {
                        // 複利効果：前回の結果を基準として次のoffsetを適用
                        currentTransformData = coordinate.IsGlobal 
                            ? ApplyGlobalOffsetCompound(currentTransformData, offset)
                            : ApplyLocalOffsetCompound(selectedTransform, currentTransformData, offset);

                        // オブジェクト複製
                        var newObject = DuplicateGameObject(selectedTransform.gameObject);
                        
                        // Transform適用
                        currentTransformData.ApplyToTransform(newObject.transform);
                        newObject.name = selectedTransform.name;

                        Undo.RegisterCreatedObjectUndo(newObject, "Create Copy");
                        copyTransforms.Add(newObject.transform);
                    }

                    // 親中央配置
                    AlignToParentCenter(copyTransforms.ToArray());
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"特殊複製中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// オブジェクト入れ替えを実行（DDDベース、Undo対応）
        /// </summary>
        public static void ExecuteObjectSwapper(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is not set.");
                return;
            }

            try
            {
                Undo.SetCurrentGroupName("Object Swap");
                int undoGroup = Undo.GetCurrentGroup();

                foreach (var obj in Selection.gameObjects)
                {
                    var originalData = TransformData.FromTransform(obj.transform);
                    var originalParent = obj.transform.parent;

                    // Prefab インスタンス化（Prefabアセットの場合はPrefabUtility、そうでなければInstantiate）
                    GameObject newObject;
                    if (PrefabUtility.IsPartOfPrefabAsset(prefab))
                    {
                        newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    }
                    else
                    {
                        newObject = GameObject.Instantiate(prefab);
                    }
                    
                    if (newObject == null)
                    {
                        Debug.LogError("Error instantiating prefab.");
                        continue;
                    }

                    // Transform復元
                    originalData.ApplyToTransform(newObject.transform);
                    newObject.transform.SetParent(originalParent, true);

                    Undo.RegisterCreatedObjectUndo(newObject, "Swap Prefab");
                    Undo.DestroyObjectImmediate(obj);
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"オブジェクト入れ替え中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        // プライベートヘルパーメソッド（複利効果用）
        private static TransformData ApplyGlobalOffsetCompound(TransformData current, TransformData offset)
        {
            // 元実装通り：前回の結果に対してoffsetを単純適用（複利効果）
            var newPosition = current.Position + offset.Position;
            
            // 元実装通り軸別回転適用
            var newRotation = current.Rotation;
            var rotationOffset = offset.Rotation.eulerAngles;
            newRotation = Quaternion.AngleAxis(rotationOffset.x, Vector3.right) * newRotation;
            newRotation = Quaternion.AngleAxis(rotationOffset.y, Vector3.up) * newRotation;
            newRotation = Quaternion.AngleAxis(rotationOffset.z, Vector3.forward) * newRotation;
            
            // 複利効果：前回のScaleに対してoffsetScaleを掛ける
            var newScale = Vector3.Scale(current.LocalScale, offset.LocalScale);

            return new TransformData(newPosition, newRotation, newScale);
        }

        private static TransformData ApplyLocalOffsetCompound(Transform referenceTransform, TransformData current, TransformData offset)
        {
            // 元実装通り：ローカル座標でのoffset適用（複利効果）
            var newPosition = current.Position + referenceTransform.TransformDirection(offset.Position);
            var newRotation = current.Rotation * Quaternion.Euler(referenceTransform.TransformDirection(offset.Rotation.eulerAngles));
            
            // 複利効果：前回のScaleに対してoffsetScaleを掛ける
            var newScale = Vector3.Scale(current.LocalScale, offset.LocalScale);

            return new TransformData(newPosition, newRotation, newScale);
        }

        // 元のメソッド（既存の呼び出し元のために保持）
        private static TransformData ApplyGlobalOffset(TransformData source, TransformData offset, int multiplier)
        {
            var newPosition = source.Position + (offset.Position * multiplier);
            
            // 元実装通り軸別回転適用
            var newRotation = source.Rotation;
            var rotationOffset = offset.Rotation.eulerAngles * multiplier;
            newRotation = Quaternion.AngleAxis(rotationOffset.x, Vector3.right) * newRotation;
            newRotation = Quaternion.AngleAxis(rotationOffset.y, Vector3.up) * newRotation;
            newRotation = Quaternion.AngleAxis(rotationOffset.z, Vector3.forward) * newRotation;
            
            var newScale = Vector3.Scale(source.LocalScale, offset.LocalScale);

            return new TransformData(newPosition, newRotation, newScale);
        }

        private static TransformData ApplyLocalOffset(Transform sourceTransform, TransformData source, TransformData offset, int multiplier)
        {
            var newPosition = source.Position + sourceTransform.TransformDirection(offset.Position * multiplier);
            var newRotation = source.Rotation * Quaternion.Euler(sourceTransform.TransformDirection(offset.Rotation.eulerAngles * multiplier));
            var newScale = Vector3.Scale(source.LocalScale, offset.LocalScale);

            return new TransformData(newPosition, newRotation, newScale);
        }

        private static GameObject DuplicateGameObject(GameObject original)
        {
            var prefabType = PrefabUtility.GetPrefabAssetType(original);
            
            if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
            {
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(original);
                return PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            }
            else
            {
                return UnityEngine.Object.Instantiate(original);
            }
        }

        private static void AlignToParentCenter(Transform[] transforms)
        {
            // DDDベースの抽象化されたグループ化サービスを使用
            _groupingService.CreateParentCenterGroup(transforms);
        }
    }
}