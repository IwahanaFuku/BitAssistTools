using System;
using System.Collections.Generic;
using BitAssistTools.Domain.Models.TextureTools;

namespace BitAssistTools.Application.ViewModels.TextureTools
{
    /// <summary>
    /// レシピ機能ViewModel
    /// </summary>
    public class RecipeViewModel
    {
        #region Events
        public event Action OnRecipeChanged;
        public event Action<string> OnNotification;
        public event Action<string> OnError;
        #endregion

        #region Properties
        private RecipeType _selectedRecipe = RecipeType.None;
        public RecipeType SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                if (_selectedRecipe != value)
                {
                    _selectedRecipe = value;
                    OnRecipeChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 利用可能なレシピ一覧
        /// </summary>
        public IReadOnlyList<TextureRecipe> AvailableRecipes { get; } = new[]
        {
            new TextureRecipe(RecipeType.PngConversion, "PNG変換＋元削除", "PNG形式に変換し、元ファイルを削除します"),
            new TextureRecipe(RecipeType.InvertAll, "全チャンネル反転", "RGB全チャンネルを反転します"),
            new TextureRecipe(RecipeType.NormalGInvert, "G反転（OpenGL→DirectX）", "法線マップのGチャンネルを反転します"),
            new TextureRecipe(RecipeType.NormalStrength15, "強度1.5倍", "法線マップの強度を1.5倍にします"),
            new TextureRecipe(RecipeType.RMAToUnity, "RMA → Unity Standard", "UE4形式RMAテクスチャをUnity Standardシェーダ用に変換します（R=Metallic, G=AO, Alpha=Smoothness）")
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// 選択中のレシピの説明を取得
        /// </summary>
        public string GetSelectedRecipeDescription()
        {
            if (_selectedRecipe == RecipeType.None)
                return "レシピが選択されていません";

            foreach (var recipe in AvailableRecipes)
            {
                if (recipe.Type == _selectedRecipe)
                    return recipe.Description;
            }

            return "不明なレシピです";
        }

        /// <summary>
        /// レシピを実行
        /// </summary>
        public void ExecuteSelectedRecipe()
        {
            if (_selectedRecipe == RecipeType.None)
            {
                OnError?.Invoke("レシピが選択されていません");
                return;
            }

            try
            {
                // TODO: 実際のレシピ実行処理を実装
                OnNotification?.Invoke($"レシピ「{GetSelectedRecipeDescription()}」を実行しました");
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"レシピ実行に失敗しました: {ex.Message}");
            }
        }
        #endregion
    }
}