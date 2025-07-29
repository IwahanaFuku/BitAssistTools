using System;
using UnityEngine;
using BitAssistTools.Application.Services.TextureTools;
using BitAssistTools.Infrastructure.Unity.ImageParsers;

namespace BitAssistTools.Application.ViewModels.TextureTools
{
    /// <summary>
    /// テクスチャ選択・管理ViewModel
    /// </summary>
    public class TextureSelectionViewModel
    {
        #region Events
        public event Action OnTextureChanged;
        public event Action<string> OnNotification;
        public event Action<string> OnError;
        #endregion

        #region Properties
        private Texture2D _targetTexture;
        public Texture2D TargetTexture
        {
            get => _targetTexture;
            set
            {
                if (_targetTexture != value)
                {
                    _targetTexture = value;
                    ClearDisplayTextureCache();
                    OnTextureChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 表示用のターゲットテクスチャ（NormalMapの場合は変換済み）
        /// </summary>
        public Texture2D DisplayTargetTexture
        {
            get
            {
                if (_displayTextureCache == null || _lastCachedTexture != _targetTexture)
                {
                    ClearDisplayTextureCache();
                    
                    // 自動でプレビュー準備（Read/Write Enabled + Texture Type調整）
                    if (_targetTexture != null)
                    {
                        TextureDisplayService.EnsureTexturePreviewReady(_targetTexture);
                    }
                    
                    _displayTextureCache = TextureDisplayService.CreateDisplayableTexture(_targetTexture);
                    _lastCachedTexture = _targetTexture;
                }
                return _displayTextureCache;
            }
        }
        #endregion

        #region Private Fields
        private Texture2D _displayTextureCache;
        private Texture2D _lastCachedTexture;
        #endregion

        #region Public Methods
        /// <summary>
        /// 選択中のテクスチャを自動設定
        /// </summary>
        public void UpdateFromSelection()
        {
            var selected = UnityEditor.Selection.activeObject as Texture2D;
            if (selected != null)
            {
                TargetTexture = selected;
            }
        }

        /// <summary>
        /// テクスチャの元解像度を取得
        /// </summary>
        public string GetRawTextureSize()
        {
            if (TargetTexture == null) return "不明";
            return GetOriginalTextureSize(TargetTexture);
        }

        /// <summary>
        /// テクスチャのリサイズ後解像度を取得
        /// </summary>
        public string GetResizeTextureSize()
        {
            if (TargetTexture == null) return "不明";
            return $"{TargetTexture.width}×{TargetTexture.height}";
        }

        /// <summary>
        /// テクスチャの元の解像度を取得
        /// </summary>
        private string GetOriginalTextureSize(UnityEngine.Texture2D texture)
        {
            if (texture == null) return "不明";
            
            var path = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return $"{texture.width}×{texture.height}";
            
            try
            {
                // ファクトリーを使用してファイルサイズを解析
                var originalSize = ImageSizeParserFactory.ParseImageSize(path);
                if (originalSize.HasValue)
                {
                    return $"{originalSize.Value.width}×{originalSize.Value.height}";
                }
            }
            catch (System.Exception)
            {
                // エラー時は現在サイズを使用
            }
            
            return $"{texture.width}×{texture.height}";
        }

        /// <summary>
        /// テクスチャが読み取り可能かチェック
        /// </summary>
        public bool IsTargetTextureReadable()
        {
            return TextureDisplayService.IsTextureReadable(TargetTexture);
        }
        #endregion

        #region Private Methods
        private void ClearDisplayTextureCache()
        {
            if (_displayTextureCache != null && _displayTextureCache != _lastCachedTexture)
            {
                // NormalMapから変換されたテクスチャの場合は破棄
                var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_lastCachedTexture);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
                    if (importer?.textureType == UnityEditor.TextureImporterType.NormalMap)
                    {
                        UnityEngine.Object.DestroyImmediate(_displayTextureCache);
                    }
                }
            }
            
            _displayTextureCache = null;
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            ClearDisplayTextureCache();
        }
        #endregion
    }
}