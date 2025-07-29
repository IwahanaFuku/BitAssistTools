using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.AssetNavigation;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class AssetNavigationServiceTests
    {
        private string _testAssetPath;
        private Material _testAsset;

        [SetUp]
        public void SetUp()
        {
            // テスト用のアセットを作成
            _testAssetPath = "Assets/TestMaterial.mat";
            _testAsset = new Material(Shader.Find("Standard"));
            
            // ディレクトリが存在しない場合は作成
            var directory = Path.GetDirectoryName(_testAssetPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            AssetDatabase.CreateAsset(_testAsset, _testAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用アセットを削除
            if (File.Exists(_testAssetPath))
            {
                AssetDatabase.DeleteAsset(_testAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void SelectAsset_WithValidPath_SelectsAsset()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);

            // Act
            AssetNavigationService.SelectAsset(assetPath);

            // Assert
            var selectedAsset = UnityEditor.Selection.activeObject;
            Assert.IsNotNull(selectedAsset);
            Assert.AreEqual(_testAsset.name, selectedAsset.name);
        }

        [Test]
        public void SelectAsset_WithInvalidPath_LogsError()
        {
            // Arrange
            var invalidPath = new AssetPath("Assets/NonExistent.asset");

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("指定されたパスにアセットが見つかりません"));
            AssetNavigationService.SelectAsset(invalidPath);
        }

        [Test]
        public void SelectAsset_WithEmptyPath_LogsAssetNotFoundError()
        {
            // Arrange
            var emptyPath = new AssetPath("");

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("指定されたパスにアセットが見つかりません"));
            AssetNavigationService.SelectAsset(emptyPath);
        }

        [Test]
        public void SelectAsset_WithNormalizedInvalidPath_LogsAssetNotFoundError()
        {
            // Arrange
            var invalidPath = new AssetPath("C:/InvalidPath/test.asset");

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("指定されたパスにアセットが見つかりません"));
            AssetNavigationService.SelectAsset(invalidPath);
        }

        [Test]
        public void AssetExists_WithValidPath_ReturnsTrue()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);

            // Act
            bool exists = AssetNavigationService.AssetExists(assetPath);

            // Assert
            Assert.IsTrue(exists);
        }

        [Test]
        public void AssetExists_WithInvalidPath_ReturnsFalse()
        {
            // Arrange
            var invalidPath = new AssetPath("Assets/NonExistent.asset");

            // Act
            bool exists = AssetNavigationService.AssetExists(invalidPath);

            // Assert
            Assert.IsFalse(exists);
        }

        [Test]
        public void AssetExists_WithEmptyPath_ReturnsFalse()
        {
            // Arrange
            var emptyPath = new AssetPath("");

            // Act
            bool exists = AssetNavigationService.AssetExists(emptyPath);

            // Assert
            Assert.IsFalse(exists);
        }

        [Test]
        public void CanSelectAsset_WithValidExistingPath_ReturnsTrue()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);

            // Act
            bool canSelect = AssetNavigationService.CanSelectAsset(assetPath);

            // Assert
            Assert.IsTrue(canSelect);
        }

        [Test]
        public void CanSelectAsset_WithInvalidPath_ReturnsFalse()
        {
            // Arrange
            var invalidPath = new AssetPath("Assets/NonExistent.asset");

            // Act
            bool canSelect = AssetNavigationService.CanSelectAsset(invalidPath);

            // Assert
            Assert.IsFalse(canSelect);
        }

        [Test]
        public void CanSelectAsset_WithEmptyPath_ReturnsFalse()
        {
            // Arrange
            var emptyPath = new AssetPath("");

            // Act
            bool canSelect = AssetNavigationService.CanSelectAsset(emptyPath);

            // Assert
            Assert.IsFalse(canSelect);
        }

        [Test]
        public void GetSelectedAssetPath_WithSelectedAsset_ReturnsCorrectPath()
        {
            // Arrange
            UnityEditor.Selection.activeObject = _testAsset;

            // Act
            var selectedPath = AssetNavigationService.GetSelectedAssetPath();

            // Assert
            Assert.AreEqual(_testAssetPath, selectedPath.ToString());
        }

        [Test]
        public void GetSelectedAssetPath_WithNoSelection_ReturnsDefaultPath()
        {
            // Arrange
            UnityEditor.Selection.activeObject = null;

            // Act
            var selectedPath = AssetNavigationService.GetSelectedAssetPath();

            // Assert
            Assert.AreEqual(AssetPath.Default, selectedPath);
        }

        [Test]
        public void GetSelectedAssetPath_WithNonAssetSelection_ReturnsDefaultPath()
        {
            // Arrange
            var sceneObject = new GameObject("SceneObject");
            UnityEditor.Selection.activeObject = sceneObject;

            try
            {
                // Act
                var selectedPath = AssetNavigationService.GetSelectedAssetPath();

                // Assert
                Assert.AreEqual(AssetPath.Default, selectedPath);
            }
            finally
            {
                // Cleanup
                UnityEngine.Object.DestroyImmediate(sceneObject);
            }
        }

        [Test]
        public void GetAssetInfo_WithValidAsset_ReturnsCorrectInfo()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);

            // Act
            var assetInfo = AssetNavigationService.GetAssetInfo(assetPath);

            // Assert
            Assert.IsTrue(assetInfo.Exists);
            Assert.IsTrue(assetInfo.IsValid);
            Assert.AreEqual("Material", assetInfo.TypeOrError);
            Assert.IsNotNull(assetInfo.Asset);
            Assert.AreEqual(_testAsset.name, assetInfo.Asset.name);
        }

        [Test]
        public void GetAssetInfo_WithInvalidPath_ReturnsErrorInfo()
        {
            // Arrange
            var invalidPath = new AssetPath("Assets/NonExistent.asset");

            // Act
            var assetInfo = AssetNavigationService.GetAssetInfo(invalidPath);

            // Assert
            Assert.IsFalse(assetInfo.Exists);
            Assert.IsFalse(assetInfo.IsValid);
            Assert.AreEqual("アセットが見つかりません", assetInfo.TypeOrError);
            Assert.IsNull(assetInfo.Asset);
        }

        [Test]
        public void GetAssetInfo_WithEmptyPath_ReturnsAssetNotFoundInfo()
        {
            // Arrange
            var emptyPath = new AssetPath("");

            // Act
            var assetInfo = AssetNavigationService.GetAssetInfo(emptyPath);

            // Assert
            Assert.IsFalse(assetInfo.Exists);
            Assert.IsFalse(assetInfo.IsValid);
            Assert.AreEqual("アセットが見つかりません", assetInfo.TypeOrError); // 空のパスは"Assets/"に正規化されるため
            Assert.IsNull(assetInfo.Asset);
        }

        [Test]
        public void GetAssetInfo_WithNormalizedInvalidPath_ReturnsAssetNotFoundInfo()
        {
            // Arrange - どんなパスも正規化されて"Assets/"で始まるパスになるため
            var invalidPath = new AssetPath("C:/InvalidPath/test.asset");

            // Act
            var assetInfo = AssetNavigationService.GetAssetInfo(invalidPath);

            // Assert
            Assert.IsFalse(assetInfo.Exists);
            Assert.IsFalse(assetInfo.IsValid);
            Assert.AreEqual("アセットが見つかりません", assetInfo.TypeOrError); // 正規化されるため
            Assert.IsNull(assetInfo.Asset);
        }

        [Test]
        public void AssetInfo_ToString_WithValidAsset_ReturnsFormattedString()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);
            var assetInfo = new AssetNavigationService.AssetInfo(assetPath, true, "Material", _testAsset);

            // Act
            var result = assetInfo.ToString();

            // Assert
            Assert.IsTrue(result.Contains(_testAssetPath));
            Assert.IsTrue(result.Contains("Material"));
        }

        [Test]
        public void AssetInfo_ToString_WithInvalidAsset_ReturnsErrorString()
        {
            // Arrange
            var invalidPath = new AssetPath("Assets/Invalid.asset");
            var assetInfo = new AssetNavigationService.AssetInfo(invalidPath, false, "エラーメッセージ", null);

            // Act
            var result = assetInfo.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Assets/Invalid.asset"));
            Assert.IsTrue(result.Contains("エラーメッセージ"));
        }

        [Test]
        public void AssetInfo_IsValid_WithValidAsset_ReturnsTrue()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);
            var assetInfo = new AssetNavigationService.AssetInfo(assetPath, true, "Material", _testAsset);

            // Act & Assert
            Assert.IsTrue(assetInfo.IsValid);
        }

        [Test]
        public void AssetInfo_IsValid_WithNullAsset_ReturnsFalse()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);
            var assetInfo = new AssetNavigationService.AssetInfo(assetPath, true, "Material", null);

            // Act & Assert
            Assert.IsFalse(assetInfo.IsValid);
        }

        [Test]
        public void AssetInfo_IsValid_WithNonExistentAsset_ReturnsFalse()
        {
            // Arrange
            var invalidPath = new AssetPath("Assets/Invalid.asset");
            var assetInfo = new AssetNavigationService.AssetInfo(invalidPath, false, "エラー", null);

            // Act & Assert
            Assert.IsFalse(assetInfo.IsValid);
        }

        [Test]
        public void AssetPath_Normalize_WithRelativePath_WorksCorrectly()
        {
            // Arrange - 相対パスの正規化をテスト
            var relativePath = new AssetPath("TestMaterial.mat");

            // Act
            var normalizedPath = relativePath.Normalize();

            // Assert
            Assert.AreEqual("Assets/TestMaterial.mat", normalizedPath.ToString());
            Assert.IsTrue(normalizedPath.IsValid());
        }

        [Test]
        public void GetAssetInfo_Constructor_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var assetPath = new AssetPath(_testAssetPath);
            var typeInfo = "TestType";

            // Act
            var assetInfo = new AssetNavigationService.AssetInfo(assetPath, true, typeInfo, _testAsset);

            // Assert
            Assert.AreEqual(assetPath, assetInfo.Path);
            Assert.IsTrue(assetInfo.Exists);
            Assert.AreEqual(typeInfo, assetInfo.TypeOrError);
            Assert.AreEqual(_testAsset, assetInfo.Asset);
        }
    }
}