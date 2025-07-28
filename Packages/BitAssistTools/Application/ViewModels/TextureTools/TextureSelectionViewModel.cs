using System;
using UnityEngine;
using BitAssistTools.Application.Services.TextureTools;

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
                OnNotification?.Invoke($"テクスチャを設定: {selected.name}");
            }
        }

        /// <summary>
        /// テクスチャ情報を取得
        /// </summary>
        public string GetTextureInfo()
        {
            if (TargetTexture == null) return "テクスチャが選択されていません";
            
            return $"{TargetTexture.width}×{TargetTexture.height}, {TargetTexture.format}";
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