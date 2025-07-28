using System;
using UnityEngine;

namespace BitAssistTools.Application.ViewModels.TextureTools
{
    /// <summary>
    /// テクスチャツールメイン統合ViewModel - 軽量な統合クラス
    /// </summary>
    public class TextureToolsMainViewModel
    {
        #region Events
        public event Action OnStateChanged;
        public event Action<string> OnNotification;
        public event Action<string> OnError;
        #endregion

        #region Sub ViewModels
        public TextureSelectionViewModel Selection { get; }
        public RecipeViewModel Recipe { get; }
        public DetailFunctionViewModel DetailFunction { get; }
        public PreviewViewModel Preview { get; }
        #endregion

        #region Properties
        public int SelectedTab { get; set; } = 0; // 0=レシピ, 1=詳細
        #endregion

        #region Constructor
        public TextureToolsMainViewModel()
        {
            // サブViewModelを初期化
            Selection = new TextureSelectionViewModel();
            Recipe = new RecipeViewModel();
            DetailFunction = new DetailFunctionViewModel();
            Preview = new PreviewViewModel();

            // イベントハンドラーを設定
            SetupEventHandlers();
        }
        #endregion

        #region Private Methods
        private void SetupEventHandlers()
        {
            // テクスチャ選択変更時の処理
            Selection.OnTextureChanged += OnTextureChanged;
            Selection.OnNotification += (msg) => OnNotification?.Invoke(msg);
            Selection.OnError += (msg) => OnError?.Invoke(msg);

            // レシピ変更時の処理
            Recipe.OnRecipeChanged += OnRecipeChanged;
            Recipe.OnNotification += (msg) => OnNotification?.Invoke(msg);
            Recipe.OnError += (msg) => OnError?.Invoke(msg);

            // 詳細機能変更時の処理
            DetailFunction.OnFunctionChanged += OnDetailFunctionChanged;
            DetailFunction.OnSettingsChanged += OnDetailSettingsChanged;
            DetailFunction.OnNotification += (msg) => OnNotification?.Invoke(msg);
            DetailFunction.OnError += (msg) => OnError?.Invoke(msg);

            // プレビュー変更時の処理
            Preview.OnPreviewChanged += () => OnStateChanged?.Invoke();
        }

        private void OnTextureChanged()
        {
            // テクスチャが変更されたら、プレビューを更新
            UpdatePreview();
            OnStateChanged?.Invoke();
        }

        private void OnRecipeChanged()
        {
            // レシピが変更されたら、詳細機能をクリアしてプレビューを更新
            DetailFunction.SelectedFunction = Domain.Models.TextureTools.DetailFunction.None;
            UpdatePreview();
            OnStateChanged?.Invoke();
        }

        private void OnDetailFunctionChanged()
        {
            // 詳細機能が変更されたら、レシピをクリアしてプレビューを更新
            Recipe.SelectedRecipe = Domain.Models.TextureTools.RecipeType.None;
            UpdatePreview();
            OnStateChanged?.Invoke();
        }

        private void OnDetailSettingsChanged()
        {
            // 詳細機能の設定が変更されたら、プレビューを更新
            UpdatePreview();
            OnStateChanged?.Invoke();
        }

        private void UpdatePreview()
        {
            var sourceTexture = Selection.DisplayTargetTexture;
            if (sourceTexture == null)
            {
                Preview.ClearPreview();
                return;
            }

            // レシピまたは詳細機能のプレビューを生成
            if (Recipe.SelectedRecipe != Domain.Models.TextureTools.RecipeType.None)
            {
                Preview.GenerateRecipePreview(sourceTexture, Recipe.SelectedRecipe);
            }
            else if (DetailFunction.SelectedFunction != Domain.Models.TextureTools.DetailFunction.None)
            {
                Preview.GenerateDetailPreview(sourceTexture, DetailFunction.SelectedFunction, DetailFunction.GetFunctionParameters());
            }
            else
            {
                Preview.ClearPreview();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 選択中のテクスチャを更新
        /// </summary>
        public void UpdateSelectedTexture()
        {
            Selection.UpdateFromSelection();
        }

        /// <summary>
        /// 状態変更を通知
        /// </summary>
        public void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
        }
        #endregion

        #region Read/Write Enable Management
        /// <summary>
        /// 管理中のテクスチャがあるかチェック
        /// </summary>
        public bool HasManagedTextures()
        {
            return Services.TextureTools.TextureDisplayService.HasManagedTextures();
        }

        /// <summary>
        /// 管理中のテクスチャ情報を取得
        /// </summary>
        public System.Collections.Generic.List<string> GetManagedTextureInfo()
        {
            return Services.TextureTools.TextureDisplayService.GetManagedTextureInfo();
        }

        /// <summary>
        /// 全てのテクスチャのRead/Write設定を元に戻す
        /// </summary>
        public void RestoreAllTextureReadable()
        {
            Services.TextureTools.TextureDisplayService.RestoreAllTextureReadable();
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            // ツール終了時に自動でRead/Write設定を復元
            RestoreAllTextureReadable();
            
            Selection?.Dispose();
            Preview?.Dispose();
        }
        #endregion
    }
}