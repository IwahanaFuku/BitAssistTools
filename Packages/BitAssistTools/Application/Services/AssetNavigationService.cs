using System;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.AssetNavigation;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// アセットナビゲーション操作のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class AssetNavigationService
    {
        /// <summary>
        /// 指定したパスのアセットを選択・表示（DDDベース）
        /// </summary>
        public static void SelectAsset(AssetPath assetPath)
        {
            try
            {
                var normalizedPath = assetPath.Normalize();
                
                if (!normalizedPath.IsValid())
                {
                    Debug.LogError($"無効なアセットパスです: {assetPath}");
                    return;
                }

                var asset = AssetDatabase.LoadAssetAtPath(normalizedPath, typeof(UnityEngine.Object));
                if (asset == null)
                {
                    Debug.LogError($"指定されたパスにアセットが見つかりません: {normalizedPath}");
                    return;
                }

                ProjectWindowUtil.ShowCreatedAsset(asset);
            }
            catch (Exception ex)
            {
                Debug.LogError($"アセット選択中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 指定したパスにアセットが存在するかを判定
        /// </summary>
        public static bool AssetExists(AssetPath assetPath)
        {
            try
            {
                var normalizedPath = assetPath.Normalize();
                if (!normalizedPath.IsValid()) return false;

                var asset = AssetDatabase.LoadAssetAtPath(normalizedPath, typeof(UnityEngine.Object));
                return asset != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// アセット選択が実行可能かどうかを判定
        /// </summary>
        public static bool CanSelectAsset(AssetPath assetPath)
        {
            return assetPath.IsValid() && AssetExists(assetPath);
        }

        /// <summary>
        /// 現在のProjectビューで選択されているアセットのパスを取得
        /// </summary>
        public static AssetPath GetSelectedAssetPath()
        {
            var selectedObject = UnityEditor.Selection.activeObject;
            if (selectedObject == null) return AssetPath.Default;

            string path = AssetDatabase.GetAssetPath(selectedObject);
            return string.IsNullOrEmpty(path) ? AssetPath.Default : new AssetPath(path);
        }

        /// <summary>
        /// アセット情報の取得
        /// </summary>
        public static AssetInfo GetAssetInfo(AssetPath assetPath)
        {
            try
            {
                var normalizedPath = assetPath.Normalize();
                if (!normalizedPath.IsValid())
                {
                    return new AssetInfo(normalizedPath, false, "無効なパス", null);
                }

                var asset = AssetDatabase.LoadAssetAtPath(normalizedPath, typeof(UnityEngine.Object));
                if (asset == null)
                {
                    return new AssetInfo(normalizedPath, false, "アセットが見つかりません", null);
                }

                string assetType = asset.GetType().Name;
                return new AssetInfo(normalizedPath, true, assetType, asset);
            }
            catch (Exception ex)
            {
                return new AssetInfo(assetPath, false, $"エラー: {ex.Message}", null);
            }
        }

        /// <summary>
        /// アセット情報
        /// </summary>
        public readonly struct AssetInfo
        {
            public readonly AssetPath Path;
            public readonly bool Exists;
            public readonly string TypeOrError;
            public readonly UnityEngine.Object Asset;

            public AssetInfo(AssetPath path, bool exists, string typeOrError, UnityEngine.Object asset)
            {
                Path = path;
                Exists = exists;
                TypeOrError = typeOrError;
                Asset = asset;
            }

            public bool IsValid => Exists && Asset != null;

            public override string ToString()
            {
                if (!Exists) return $"{Path} - {TypeOrError}";
                return $"{Path} ({TypeOrError})";
            }
        }
    }
}