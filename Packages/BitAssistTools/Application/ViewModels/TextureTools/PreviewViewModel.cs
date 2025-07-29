using System;
using UnityEngine;
using BitAssistTools.Domain.Models.TextureTools;
using BitAssistTools.Application.Services.TextureTools;

namespace BitAssistTools.Application.ViewModels.TextureTools
{
    /// <summary>
    /// プレビュー表示・管理ViewModel
    /// </summary>
    public class PreviewViewModel
    {
        #region Events
        public event Action OnPreviewChanged;
        #endregion

        #region Properties
        public PreviewSettings Settings { get; private set; } = new PreviewSettings(PreviewMode.RGB, false);
        
        private Texture2D _previewTexture;
        public Texture2D PreviewTexture
        {
            get => _previewTexture;
            private set
            {
                if (_previewTexture != value)
                {
                    _previewTexture = value;
                    OnPreviewChanged?.Invoke();
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// プレビュー設定を更新
        /// </summary>
        public void UpdateSettings(PreviewMode mode, bool showBeforeAfter)
        {
            Settings = new PreviewSettings(mode, showBeforeAfter);
            OnPreviewChanged?.Invoke();
        }

        /// <summary>
        /// レシピプレビューを生成
        /// </summary>
        public void GenerateRecipePreview(Texture2D sourceTexture, RecipeType recipe)
        {
            PreviewTexture = TexturePreviewService.GenerateRecipePreview(sourceTexture, recipe);
        }

        /// <summary>
        /// 詳細機能プレビューを生成
        /// </summary>
        public void GenerateDetailPreview(Texture2D sourceTexture, DetailFunction function, object parameters = null)
        {
            PreviewTexture = TexturePreviewService.GenerateDetailPreview(sourceTexture, function, parameters);
        }

        /// <summary>
        /// プレビューをクリア
        /// </summary>
        public void ClearPreview()
        {
            PreviewTexture = null;
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            if (_previewTexture != null)
            {
                // 生成されたプレビューテクスチャを破棄
                UnityEngine.Object.DestroyImmediate(_previewTexture);
                _previewTexture = null;
            }
        }
        #endregion
    }
}