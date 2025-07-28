using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BitAssistTools.Application.Services.TextureTools
{
    /// <summary>
    /// テクスチャ表示サービス - NormalMap等の表示用変換を担当
    /// </summary>
    public static class TextureDisplayService
    {
        /// <summary>
        /// NormalMapタイプのテクスチャを通常表示用のテクスチャに変換
        /// </summary>
        public static Texture2D CreateDisplayableTexture(Texture2D originalTexture)
        {
            if (originalTexture == null) return null;
            
            // テクスチャタイプを確認
            var assetPath = AssetDatabase.GetAssetPath(originalTexture);
            if (string.IsNullOrEmpty(assetPath))
            {
                return originalTexture; // ランタイム生成テクスチャはそのまま返す
            }
            
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer?.textureType != TextureImporterType.NormalMap)
            {
                return originalTexture; // NormalMapタイプでなければそのまま返す
            }
            
            // NormalMapタイプの場合は、RenderTextureを使用して互換性のある形式に変換
            try
            {
                return ConvertNormalMapToDisplayTexture(originalTexture);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TextureDisplayService] 表示用テクスチャの作成に失敗しました: {ex.Message}");
                return originalTexture;
            }
        }

        /// <summary>
        /// NormalMapをRenderTextureを使用して表示用に変換
        /// </summary>
        private static Texture2D ConvertNormalMapToDisplayTexture(Texture2D normalMap)
        {
            // RenderTextureを使用してテクスチャを変換
            var renderTexture = RenderTexture.GetTemporary(normalMap.width, normalMap.height, 0, RenderTextureFormat.ARGB32);
            var oldRT = RenderTexture.active;
            
            try
            {
                RenderTexture.active = renderTexture;
                Graphics.Blit(normalMap, renderTexture);
                
                // 新しいテクスチャを作成（RGBA32フォーマットで確実に読み書き可能）
                var newTexture = new Texture2D(normalMap.width, normalMap.height, TextureFormat.RGBA32, false);
                newTexture.ReadPixels(new Rect(0, 0, normalMap.width, normalMap.height), 0, 0);
                newTexture.Apply();
                
                return newTexture;
            }
            finally
            {
                // リソースクリーンアップ
                RenderTexture.active = oldRT;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        /// <summary>
        /// テクスチャが読み取り可能かチェック
        /// </summary>
        public static bool IsTextureReadable(Texture2D texture)
        {
            if (texture == null) return false;
            
            try
            {
                texture.GetPixel(0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Read/Write Enable Auto Management

        /// <summary>
        /// 元のRead/Write設定を記録する辞書（パス -> 元の設定）
        /// </summary>
        private static readonly Dictionary<string, bool> _originalReadableStates = new Dictionary<string, bool>();

        /// <summary>
        /// テクスチャのRead/Write設定を自動管理してプレビューに必要な状態にする
        /// </summary>
        public static bool EnsureTextureReadable(Texture2D texture)
        {
            if (texture == null) return false;
            
            var path = GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return false;
            
            var importer = GetTextureImporter(texture);
            if (importer == null) return false;
            
            // 元の状態を記録（初回のみ）
            if (!_originalReadableStates.ContainsKey(path))
            {
                _originalReadableStates[path] = importer.isReadable;
            }
            
            // 既に読み取り可能な場合は何もしない
            if (importer.isReadable) return true;
            
            // Read/Write Enabledをオンにする
            importer.isReadable = true;
            importer.SaveAndReimport();
            
            Debug.Log($"[TextureDisplayService] テクスチャ '{texture.name}' を一時的にRead/Write Enabledに設定しました。");
            return true;
        }

        /// <summary>
        /// 指定されたテクスチャのRead/Write設定を元に戻す
        /// </summary>
        public static void RestoreTextureReadable(Texture2D texture)
        {
            if (texture == null) return;
            
            var path = GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;
            
            // 元の状態が記録されていない場合は何もしない
            if (!_originalReadableStates.ContainsKey(path)) return;
            
            var originalState = _originalReadableStates[path];
            var importer = GetTextureImporter(texture);
            if (importer == null) return;
            
            // 元の状態と現在の状態が異なる場合のみ復元
            if (importer.isReadable != originalState)
            {
                importer.isReadable = originalState;
                importer.SaveAndReimport();
                
                var stateText = originalState ? "Read/Write Enabled" : "Read/Write Disabled";
                Debug.Log($"[TextureDisplayService] テクスチャ '{texture.name}' を{stateText}に復元しました。");
            }
            
            // 記録から削除
            _originalReadableStates.Remove(path);
        }

        /// <summary>
        /// 全てのテクスチャのRead/Write設定を元に戻す
        /// </summary>
        public static void RestoreAllTextureReadable()
        {
            var pathsToRestore = new List<string>(_originalReadableStates.Keys);
            
            foreach (var path in pathsToRestore)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    RestoreTextureReadable(texture);
                }
                else
                {
                    // テクスチャが見つからない場合は記録だけ削除
                    _originalReadableStates.Remove(path);
                }
            }
        }

        /// <summary>
        /// 管理中のテクスチャがあるかチェック
        /// </summary>
        public static bool HasManagedTextures()
        {
            return _originalReadableStates.Count > 0;
        }

        /// <summary>
        /// 管理中のテクスチャ情報を取得
        /// </summary>
        public static List<string> GetManagedTextureInfo()
        {
            var info = new List<string>();
            
            foreach (var kvp in _originalReadableStates)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(kvp.Key);
                if (texture != null)
                {
                    var originalStateText = kvp.Value ? "Enabled" : "Disabled";
                    info.Add($"• {texture.name} (元: {originalStateText})");
                }
            }
            
            return info;
        }

        private static string GetAssetPath(Texture2D texture)
        {
            return AssetDatabase.GetAssetPath(texture);
        }

        private static TextureImporter GetTextureImporter(Texture2D texture)
        {
            var path = GetAssetPath(texture);
            return AssetImporter.GetAtPath(path) as TextureImporter;
        }

        #endregion
    }
}