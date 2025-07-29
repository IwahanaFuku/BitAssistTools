using System;
using System.Collections.Generic;
using UnityEngine;
using BitAssistTools.Domain.Models.TextureTools;

namespace BitAssistTools.Application.ViewModels.TextureTools
{
    /// <summary>
    /// レシピ機能ViewModel
    /// </summary>
    public class RecipeViewModel
    {
        #region Private Fields
        private readonly Func<UnityEngine.Texture2D> _getCurrentTexture;
        #endregion

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

        #region Constructor
        public RecipeViewModel(Func<UnityEngine.Texture2D> getCurrentTexture = null)
        {
            _getCurrentTexture = getCurrentTexture;
        }
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

            var sourceTexture = _getCurrentTexture?.Invoke();
            if (sourceTexture == null)
            {
                OnError?.Invoke("対象テクスチャが選択されていません");
                return;
            }

            try
            {
                // TexturePreviewServiceを使用してレシピを実行
                var resultTexture = Services.TextureTools.TexturePreviewService.GenerateRecipePreview(sourceTexture, _selectedRecipe);
                
                if (resultTexture != null)
                {
                    // 結果テクスチャをファイルとして保存
                    SaveResultTexture(resultTexture, sourceTexture);
                    OnNotification?.Invoke($"レシピ「{GetSelectedRecipeDescription()}」を実行しました");
                }
                else
                {
                    OnError?.Invoke("レシピの実行に失敗しました");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"レシピ実行に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 結果テクスチャをファイルとして保存
        /// </summary>
        private void SaveResultTexture(UnityEngine.Texture2D resultTexture, UnityEngine.Texture2D sourceTexture)
        {
            var sourcePath = UnityEditor.AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new InvalidOperationException("ソーステクスチャのパスが取得できません");
            }

            var directory = System.IO.Path.GetDirectoryName(sourcePath);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
            var recipeName = GetRecipeSuffix(_selectedRecipe);
            var outputPath = System.IO.Path.Combine(directory, $"{fileName}_{recipeName}.png");

            // PNG形式で保存 - Unity 2019.4対応
            try
            {
                var pngData = resultTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(outputPath, pngData);
            }
            catch (UnityException)
            {
                // テクスチャが読み取り可能でない場合の対処
                var readableTexture = CreateReadableTexture(resultTexture);
                var pngData = readableTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(outputPath, pngData);
                UnityEngine.Object.DestroyImmediate(readableTexture);
            }
            
            // Unityにアセットを再インポートさせる
            UnityEditor.AssetDatabase.ImportAsset(outputPath);
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// 読み取り可能なテクスチャを作成
        /// </summary>
        private UnityEngine.Texture2D CreateReadableTexture(UnityEngine.Texture2D source)
        {
            var renderTexture = UnityEngine.RenderTexture.GetTemporary(source.width, source.height, 0, UnityEngine.RenderTextureFormat.ARGB32);
            var oldRT = UnityEngine.RenderTexture.active;
            
            try
            {
                UnityEngine.RenderTexture.active = renderTexture;
                UnityEngine.Graphics.Blit(source, renderTexture);
                
                var readableTexture = new UnityEngine.Texture2D(source.width, source.height, UnityEngine.TextureFormat.RGBA32, false);
                readableTexture.ReadPixels(new UnityEngine.Rect(0, 0, source.width, source.height), 0, 0);
                readableTexture.Apply();
                
                return readableTexture;
            }
            finally
            {
                UnityEngine.RenderTexture.active = oldRT;
                UnityEngine.RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        /// <summary>
        /// レシピタイプに応じたファイル名サフィックスを取得
        /// </summary>
        private string GetRecipeSuffix(RecipeType recipeType)
        {
            switch (recipeType)
            {
                case RecipeType.PngConversion: return "png";
                case RecipeType.InvertAll: return "inverted";
                case RecipeType.NormalGInvert: return "normal_ginvert";
                case RecipeType.NormalStrength15: return "normal_15x";
                case RecipeType.RMAToUnity: return "unity_standard";
                default: return "processed";
            }
        }
        #endregion
    }
}