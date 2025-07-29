using System;
using NUnit.Framework;
using BitAssistTools.Domain.Models.TextureTools;

namespace BitAssistTools.Tests.Domain.Models.TextureTools
{
    /// <summary>
    /// DetailFunctionTypeのテストクラス
    /// </summary>
    [TestFixture]
    public class DetailFunctionTypeTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var function = DetailFunction.ChannelInvert;
            var displayName = "チャンネル反転";
            var description = "指定したチャンネルを反転します";

            // Act
            var detailFunction = new DetailFunctionType(function, displayName, description);

            // Assert
            Assert.AreEqual(function, detailFunction.Function);
            Assert.AreEqual(displayName, detailFunction.DisplayName);
            Assert.AreEqual(description, detailFunction.Description);
        }

        [Test]
        public void Constructor_NullDisplayName_ThrowsArgumentNullException()
        {
            // Arrange
            var function = DetailFunction.PngConvert;
            string displayName = null;
            var description = "テスト説明";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DetailFunctionType(function, displayName, description));
        }

        [Test]
        public void Constructor_NullDescription_ThrowsArgumentNullException()
        {
            // Arrange
            var function = DetailFunction.NormalMap;
            var displayName = "法線マップ";
            string description = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DetailFunctionType(function, displayName, description));
        }

        [Test]
        public void Constructor_AllDetailFunctions_CreatesInstance()
        {
            // すべてのDetailFunctionでインスタンスが作成できることを確認
            var functions = Enum.GetValues(typeof(DetailFunction));
            
            foreach (DetailFunction function in functions)
            {
                Assert.DoesNotThrow(() =>
                {
                    var detailFunction = new DetailFunctionType(function, $"名前_{function}", $"説明_{function}");
                    Assert.AreEqual(function, detailFunction.Function);
                });
            }
        }

        #endregion

        #region Equality Tests

        [Test]
        public void Equals_SameFunction_ReturnsTrue()
        {
            // Arrange
            var detailFunction1 = new DetailFunctionType(DetailFunction.MaskMerge, "名前1", "説明1");
            var detailFunction2 = new DetailFunctionType(DetailFunction.MaskMerge, "名前2", "説明2");

            // Act & Assert
            Assert.IsTrue(detailFunction1.Equals(detailFunction2));
            Assert.IsTrue(detailFunction1 == detailFunction2);
            Assert.IsFalse(detailFunction1 != detailFunction2);
        }

        [Test]
        public void Equals_DifferentFunction_ReturnsFalse()
        {
            // Arrange
            var detailFunction1 = new DetailFunctionType(DetailFunction.PngConvert, "名前", "説明");
            var detailFunction2 = new DetailFunctionType(DetailFunction.NormalMap, "名前", "説明");

            // Act & Assert
            Assert.IsFalse(detailFunction1.Equals(detailFunction2));
            Assert.IsFalse(detailFunction1 == detailFunction2);
            Assert.IsTrue(detailFunction1 != detailFunction2);
        }

        [Test]
        public void Equals_WithObject_SameFunction_ReturnsTrue()
        {
            // Arrange
            var detailFunction1 = new DetailFunctionType(DetailFunction.ChannelInvert, "名前", "説明");
            object detailFunction2 = new DetailFunctionType(DetailFunction.ChannelInvert, "別の名前", "別の説明");

            // Act & Assert
            Assert.IsTrue(detailFunction1.Equals(detailFunction2));
        }

        [Test]
        public void Equals_WithObject_DifferentType_ReturnsFalse()
        {
            // Arrange
            var detailFunction = new DetailFunctionType(DetailFunction.None, "名前", "説明");
            object other = "文字列";

            // Act & Assert
            Assert.IsFalse(detailFunction.Equals(other));
        }

        [Test]
        public void Equals_WithObject_Null_ReturnsFalse()
        {
            // Arrange
            var detailFunction = new DetailFunctionType(DetailFunction.MaskMerge, "名前", "説明");
            object other = null;

            // Act & Assert
            Assert.IsFalse(detailFunction.Equals(other));
        }

        #endregion

        #region GetHashCode Tests

        [Test]
        public void GetHashCode_SameFunction_ReturnsSameHashCode()
        {
            // Arrange
            var detailFunction1 = new DetailFunctionType(DetailFunction.NormalMap, "名前1", "説明1");
            var detailFunction2 = new DetailFunctionType(DetailFunction.NormalMap, "名前2", "説明2");

            // Act
            var hash1 = detailFunction1.GetHashCode();
            var hash2 = detailFunction2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_DifferentFunction_ReturnsDifferentHashCode()
        {
            // Arrange
            var detailFunction1 = new DetailFunctionType(DetailFunction.PngConvert, "名前", "説明");
            var detailFunction2 = new DetailFunctionType(DetailFunction.ChannelInvert, "名前", "説明");

            // Act
            var hash1 = detailFunction1.GetHashCode();
            var hash2 = detailFunction2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        #endregion

        #region DetailFunction Enum Tests

        [Test]
        public void DetailFunction_AllValues_HaveValidNumbers()
        {
            // すべてのDetailFunction値が定義されていることを確認
            var values = Enum.GetValues(typeof(DetailFunction));
            
            Assert.Contains(DetailFunction.None, values);
            Assert.Contains(DetailFunction.PngConvert, values);
            Assert.Contains(DetailFunction.ChannelInvert, values);
            Assert.Contains(DetailFunction.NormalMap, values);
            Assert.Contains(DetailFunction.MaskMerge, values);
        }

        [Test]
        public void DetailFunction_None_IsZero()
        {
            // None値が0であることを確認（デフォルト値として重要）
            Assert.AreEqual(0, (int)DetailFunction.None);
        }

        [Test]
        public void DetailFunction_Values_HaveCorrectOrder()
        {
            // 機能の値が適切な順序であることを確認
            Assert.IsTrue((int)DetailFunction.PngConvert > (int)DetailFunction.None);
            Assert.IsTrue((int)DetailFunction.ChannelInvert > (int)DetailFunction.PngConvert);
            Assert.IsTrue((int)DetailFunction.NormalMap > (int)DetailFunction.ChannelInvert);
            Assert.IsTrue((int)DetailFunction.MaskMerge > (int)DetailFunction.NormalMap);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void DetailFunctionType_DefaultConstructor_HasDefaultValues()
        {
            // デフォルトコンストラクタの動作確認
            var detailFunction = default(DetailFunctionType);
            
            Assert.AreEqual(DetailFunction.None, detailFunction.Function);
            Assert.IsNull(detailFunction.DisplayName);
            Assert.IsNull(detailFunction.Description);
        }

        [Test]
        public void DetailFunctionType_AllDetailFunctions_CanBeCreated()
        {
            // すべてのDetailFunctionでDetailFunctionTypeが作成できることを確認
            var functions = Enum.GetValues(typeof(DetailFunction));
            
            foreach (DetailFunction function in functions)
            {
                Assert.DoesNotThrow(() => 
                {
                    var detailFunction = new DetailFunctionType(function, $"表示名_{function}", $"説明_{function}");
                    Assert.AreEqual(function, detailFunction.Function);
                    Assert.AreEqual($"表示名_{function}", detailFunction.DisplayName);
                    Assert.AreEqual($"説明_{function}", detailFunction.Description);
                });
            }
        }

        [TestCase(DetailFunction.None, "未選択", "機能未選択状態")]
        [TestCase(DetailFunction.PngConvert, "PNG変換", "PNG形式への変換")]
        [TestCase(DetailFunction.ChannelInvert, "チャンネル反転", "指定チャンネルの反転")]
        [TestCase(DetailFunction.NormalMap, "法線マップ", "法線マップの調整")]
        [TestCase(DetailFunction.MaskMerge, "マスク統合", "複数マスクの統合")]
        public void DetailFunctionType_ValidCombinations_CreatesCorrectInstance(DetailFunction function, string displayName, string description)
        {
            // Act
            var detailFunction = new DetailFunctionType(function, displayName, description);

            // Assert
            Assert.AreEqual(function, detailFunction.Function);
            Assert.AreEqual(displayName, detailFunction.DisplayName);
            Assert.AreEqual(description, detailFunction.Description);
        }

        #endregion
    }
}