using System;
using NUnit.Framework;
using BitAssistTools.Domain.Models.TextureTools;

namespace BitAssistTools.Tests.Domain.Models.TextureTools
{
    /// <summary>
    /// TextureRecipeのテストクラス
    /// </summary>
    [TestFixture]
    public class TextureRecipeTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var type = RecipeType.InvertAll;
            var displayName = "全チャンネル反転";
            var description = "RGB全チャンネルを反転します";

            // Act
            var recipe = new TextureRecipe(type, displayName, description);

            // Assert
            Assert.AreEqual(type, recipe.Type);
            Assert.AreEqual(displayName, recipe.DisplayName);
            Assert.AreEqual(description, recipe.Description);
        }

        [Test]
        public void Constructor_NullDisplayName_ThrowsArgumentNullException()
        {
            // Arrange
            var type = RecipeType.InvertAll;
            string displayName = null;
            var description = "テスト説明";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TextureRecipe(type, displayName, description));
        }

        [Test]
        public void Constructor_NullDescription_ThrowsArgumentNullException()
        {
            // Arrange
            var type = RecipeType.InvertAll;
            var displayName = "テスト名";
            string description = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TextureRecipe(type, displayName, description));
        }

        #endregion

        #region Equality Tests

        [Test]
        public void Equals_SameType_ReturnsTrue()
        {
            // Arrange
            var recipe1 = new TextureRecipe(RecipeType.InvertAll, "名前1", "説明1");
            var recipe2 = new TextureRecipe(RecipeType.InvertAll, "名前2", "説明2");

            // Act & Assert
            Assert.IsTrue(recipe1.Equals(recipe2));
            Assert.IsTrue(recipe1 == recipe2);
            Assert.IsFalse(recipe1 != recipe2);
        }

        [Test]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var recipe1 = new TextureRecipe(RecipeType.InvertAll, "名前", "説明");
            var recipe2 = new TextureRecipe(RecipeType.NormalGInvert, "名前", "説明");

            // Act & Assert
            Assert.IsFalse(recipe1.Equals(recipe2));
            Assert.IsFalse(recipe1 == recipe2);
            Assert.IsTrue(recipe1 != recipe2);
        }

        [Test]
        public void Equals_WithObject_SameType_ReturnsTrue()
        {
            // Arrange
            var recipe1 = new TextureRecipe(RecipeType.RMAToUnity, "名前", "説明");
            object recipe2 = new TextureRecipe(RecipeType.RMAToUnity, "別の名前", "別の説明");

            // Act & Assert
            Assert.IsTrue(recipe1.Equals(recipe2));
        }

        [Test]
        public void Equals_WithObject_DifferentType_ReturnsFalse()
        {
            // Arrange
            var recipe = new TextureRecipe(RecipeType.InvertAll, "名前", "説明");
            object other = "文字列";

            // Act & Assert
            Assert.IsFalse(recipe.Equals(other));
        }

        [Test]
        public void Equals_WithObject_Null_ReturnsFalse()
        {
            // Arrange
            var recipe = new TextureRecipe(RecipeType.InvertAll, "名前", "説明");
            object other = null;

            // Act & Assert
            Assert.IsFalse(recipe.Equals(other));
        }

        #endregion

        #region GetHashCode Tests

        [Test]
        public void GetHashCode_SameType_ReturnsSameHashCode()
        {
            // Arrange
            var recipe1 = new TextureRecipe(RecipeType.NormalStrength15, "名前1", "説明1");
            var recipe2 = new TextureRecipe(RecipeType.NormalStrength15, "名前2", "説明2");

            // Act
            var hash1 = recipe1.GetHashCode();
            var hash2 = recipe2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_DifferentType_ReturnsDifferentHashCode()
        {
            // Arrange
            var recipe1 = new TextureRecipe(RecipeType.InvertAll, "名前", "説明");
            var recipe2 = new TextureRecipe(RecipeType.PngConversion, "名前", "説明");

            // Act
            var hash1 = recipe1.GetHashCode();
            var hash2 = recipe2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        #endregion

        #region RecipeType Enum Tests

        [Test]
        public void RecipeType_AllValues_HaveValidNumbers()
        {
            // すべてのRecipeType値が定義されていることを確認
            var values = Enum.GetValues(typeof(RecipeType));
            
            Assert.Contains(RecipeType.None, values);
            Assert.Contains(RecipeType.PngConversion, values);
            Assert.Contains(RecipeType.InvertAll, values);
            Assert.Contains(RecipeType.NormalGInvert, values);
            Assert.Contains(RecipeType.NormalStrength15, values);
            Assert.Contains(RecipeType.RMAToUnity, values);
        }

        [Test]
        public void RecipeType_None_IsZero()
        {
            // None値が0であることを確認（デフォルト値として重要）
            Assert.AreEqual(0, (int)RecipeType.None);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void TextureRecipe_AllRecipeTypes_CanBeCreated()
        {
            // すべてのRecipeTypeでTextureRecipeが作成できることを確認
            var recipeTypes = Enum.GetValues(typeof(RecipeType));
            
            foreach (RecipeType type in recipeTypes)
            {
                Assert.DoesNotThrow(() => 
                {
                    var recipe = new TextureRecipe(type, $"名前_{type}", $"説明_{type}");
                    Assert.AreEqual(type, recipe.Type);
                });
            }
        }

        #endregion
    }
}