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

        #region Single Texture Management

        /// <summary>
        /// 現在管理中のテクスチャ
        /// </summary>
        private static Texture2D _currentManagedTexture = null;

        /// <summary>
        /// 元のRead/Write設定
        /// </summary>
        private static bool _originalReadableState = false;

        /// <summary>
        /// 元のTexture Type設定
        /// </summary>
        private static TextureImporterType _originalTextureType = TextureImporterType.Default;

        /// <summary>
        /// テクスチャを一時的にDefaultタイプに変更してプレビューに適した状態にする
        /// </summary>
        public static bool EnsureTexturePreviewReady(Texture2D texture)
        {
            if (texture == null) 
            {
                Debug.LogWarning("[TextureDisplayService] EnsureTexturePreviewReady: texture is null");
                return false;
            }
            
            // 前のテクスチャがある場合は復元
            if (_currentManagedTexture != null && _currentManagedTexture != texture)
            {
                RestoreCurrentTexture();
            }
            
            var path = GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) 
            {
                Debug.LogWarning($"[TextureDisplayService] EnsureTexturePreviewReady: path is empty for texture '{texture.name}'");
                return false;
            }
            
            var importer = GetTextureImporter(texture);
            if (importer == null) 
            {
                Debug.LogWarning($"[TextureDisplayService] EnsureTexturePreviewReady: importer is null for texture '{texture.name}'");
                return false;
            }
            
            
            // 新しいテクスチャを管理対象にする
            _currentManagedTexture = texture;
            _originalReadableState = importer.isReadable;
            _originalTextureType = importer.textureType;
            
            
            bool needsReimport = false;
            
            // NormalMapの場合はDefaultに変更
            if (importer.textureType == TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.Default;
                needsReimport = true;
            }
            
            // Read/Write Enabledをオンにする
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                needsReimport = true;
            }
            
            // 変更があった場合のみ再インポート
            if (needsReimport)
            {
                importer.SaveAndReimport();
            }
            
            return true;
        }

        /// <summary>
        /// 現在管理中のテクスチャを元の状態に復元
        /// </summary>
        public static void RestoreCurrentTexture()
        {
            if (_currentManagedTexture == null) return;
            
            var path = GetAssetPath(_currentManagedTexture);
            if (string.IsNullOrEmpty(path)) 
            {
                _currentManagedTexture = null;
                return;
            }
            
            var importer = GetTextureImporter(_currentManagedTexture);
            if (importer == null) 
            {
                _currentManagedTexture = null;
                return;
            }
            
            bool needsReimport = false;
            
            // Texture Typeの復元
            if (importer.textureType != _originalTextureType)
            {
                importer.textureType = _originalTextureType;
                needsReimport = true;
            }
            
            // Read/Write設定の復元
            if (importer.isReadable != _originalReadableState)
            {
                importer.isReadable = _originalReadableState;
                needsReimport = true;
                var stateText = _originalReadableState ? "Read/Write Enabled" : "Read/Write Disabled";
            }
            
            // 変更があった場合のみ再インポート
            if (needsReimport)
            {
                importer.SaveAndReimport();
            }
            
            // 管理対象クリア
            _currentManagedTexture = null;
        }

        /// <summary>
        /// 管理中のテクスチャがあるかチェック
        /// </summary>
        public static bool HasManagedTextures()
        {
            return _currentManagedTexture != null;
        }

        /// <summary>
        /// 管理中のテクスチャ情報を取得
        /// </summary>
        public static List<string> GetManagedTextureInfo()
        {
            var info = new List<string>();
            
            if (_currentManagedTexture != null)
            {
                var details = new List<string>();
                details.Add($"Read/Write元: {(_originalReadableState ? "Enabled" : "Disabled")}");
                details.Add($"Type元: {_originalTextureType}");
                info.Add($"• {_currentManagedTexture.name} ({string.Join(", ", details)})");
            }
            
            return info;
        }

        /// <summary>
        /// 全てのテクスチャ設定を元に戻す（現在管理中のテクスチャのみ）
        /// </summary>
        public static void RestoreAllTextureSettings()
        {
            RestoreCurrentTexture();
        }

        /// <summary>
        /// 従来のRestoreAllTextureReadableメソッドとの互換性維持
        /// </summary>
        public static void RestoreAllTextureReadable()
        {
            RestoreAllTextureSettings();
        }

        /// <summary>
        /// ヘルパーメソッド: アセットパス取得
        /// </summary>
        private static string GetAssetPath(Texture2D texture)
        {
            return AssetDatabase.GetAssetPath(texture);
        }

        /// <summary>
        /// ヘルパーメソッド: TextureImporter取得
        /// </summary>
        private static TextureImporter GetTextureImporter(Texture2D texture)
        {
            var path = GetAssetPath(texture);
            return AssetImporter.GetAtPath(path) as TextureImporter;
        }

        #endregion
    }
}