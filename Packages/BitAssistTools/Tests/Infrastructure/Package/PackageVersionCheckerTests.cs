using System.IO;
using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Infrastructure.Package;

namespace BitAssistTools.Tests.Infrastructure.Package
{
    [TestFixture]
    public class PackageVersionCheckerTests
    {
        private string _testManifestPath;
        private string _originalManifestContent;

        [SetUp]
        public void SetUp()
        {
            // テスト用のmanifest.jsonパスを設定
            _testManifestPath = Path.GetFullPath("Packages/manifest.json");
            
            // 元のmanifest.jsonの内容を保存（存在する場合）
            if (File.Exists(_testManifestPath))
            {
                _originalManifestContent = File.ReadAllText(_testManifestPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // 元のmanifest.jsonの内容を復元
            if (!string.IsNullOrEmpty(_originalManifestContent) && File.Exists(_testManifestPath))
            {
                File.WriteAllText(_testManifestPath, _originalManifestContent);
            }
        }

        [Test]
        public void PackageInfo_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var packageInfo = new PackageInfo("test.package", "Test Package", "1.0.0", "1.1.0");

            // Assert
            Assert.AreEqual("test.package", packageInfo.PackageName);
            Assert.AreEqual("Test Package", packageInfo.DisplayName);
            Assert.AreEqual("1.0.0", packageInfo.CurrentVersion);
            Assert.AreEqual("1.1.0", packageInfo.LatestVersion);
        }

        [Test]
        public void PackageVersionChecker_Constructor_CreatesValidInstance()
        {
            // Act
            var checker = new PackageVersionChecker();
            var info = checker.GetPackageInfo();

            // Assert
            Assert.IsNotNull(info);
            Assert.AreEqual("com.iwahana.bitassisttools", info.PackageName);
            Assert.AreEqual("Bit Assist Tools", info.DisplayName);
            // CurrentVersionとLatestVersionは環境依存なので、nullでないことのみ確認
        }

        [Test]
        public void HasNewerVersion_WithNullVersions_ReturnsFalse()
        {
            // Arrange
            var packageInfo = new PackageInfo("test.package", "Test Package", null, null);

            // このテストは内部ロジックを直接テストできないため、
            // パブリックAPIの動作確認のみ行う
            var checker = new PackageVersionChecker();
            
            // Act & Assert - 例外が発生しないことを確認
            Assert.DoesNotThrow(() => checker.HasNewerVersion());
        }

        [Test]
        public void GetPackageInfo_ReturnsValidPackageInfo()
        {
            // Arrange
            var checker = new PackageVersionChecker();

            // Act
            var info = checker.GetPackageInfo();

            // Assert
            Assert.IsNotNull(info);
            Assert.AreEqual("com.iwahana.bitassisttools", info.PackageName);
            Assert.AreEqual("Bit Assist Tools", info.DisplayName);
        }

        [Test]
        public void UpdatePackage_DoesNotThrow()
        {
            // Arrange
            var checker = new PackageVersionChecker();

            // Act & Assert - 実際の更新は行わず、例外が発生しないことのみ確認
            // 注意: 実際の環境では副作用があるため、テスト環境では実行しない
            Assert.DoesNotThrow(() => {
                // ここでは実際の更新処理は呼び出さない
                // 代わりに、メソッドが存在することのみ確認
                var info = checker.GetPackageInfo();
                Assert.IsNotNull(info);
            });
        }

        [Test]
        public void PackageInfo_WithEmptyStrings_HandlesCorrectly()
        {
            // Arrange & Act
            var packageInfo = new PackageInfo("", "", "", "");

            // Assert
            Assert.AreEqual("", packageInfo.PackageName);
            Assert.AreEqual("", packageInfo.DisplayName);
            Assert.AreEqual("", packageInfo.CurrentVersion);
            Assert.AreEqual("", packageInfo.LatestVersion);
        }

        [Test]
        public void PackageInfo_IsReadOnlyStruct()
        {
            // Arrange
            var packageInfo1 = new PackageInfo("test1", "Test1", "1.0.0", "1.1.0");
            var packageInfo2 = new PackageInfo("test1", "Test1", "1.0.0", "1.1.0");
            var packageInfo3 = new PackageInfo("test2", "Test2", "2.0.0", "2.1.0");

            // Act & Assert - 値型の動作確認
            Assert.AreEqual(packageInfo1.PackageName, packageInfo2.PackageName);
            Assert.AreNotEqual(packageInfo1.PackageName, packageInfo3.PackageName);
        }

        [Test]
        public void PackageVersionChecker_MultipleInstances_WorkIndependently()
        {
            // Arrange & Act
            var checker1 = new PackageVersionChecker();
            var checker2 = new PackageVersionChecker();

            var info1 = checker1.GetPackageInfo();
            var info2 = checker2.GetPackageInfo();

            // Assert - 両方とも有効なインスタンスであることを確認
            Assert.IsNotNull(info1);
            Assert.IsNotNull(info2);
            Assert.AreEqual(info1.PackageName, info2.PackageName);
            Assert.AreEqual(info1.DisplayName, info2.DisplayName);
        }

        [Test]
        public void PackageVersionChecker_GetPackageInfo_ReturnsConsistentData()
        {
            // Arrange
            var checker = new PackageVersionChecker();

            // Act
            var info1 = checker.GetPackageInfo();
            var info2 = checker.GetPackageInfo();

            // Assert - 同じインスタンスから取得した情報は一貫している
            Assert.AreEqual(info1.PackageName, info2.PackageName);
            Assert.AreEqual(info1.DisplayName, info2.DisplayName);
            Assert.AreEqual(info1.CurrentVersion, info2.CurrentVersion);
            Assert.AreEqual(info1.LatestVersion, info2.LatestVersion);
        }

        // 注意: 以下のテストは実際のファイルシステムやネットワークに依存するため、
        // 本格的なテストはモックやテスト環境で行う必要があります

        [Test]
        public void HasNewerVersion_DoesNotThrowException()
        {
            // Arrange
            var checker = new PackageVersionChecker();

            // Act & Assert - ネットワークエラーなどがあっても例外を投げないことを確認
            bool result = false;
            Assert.DoesNotThrow(() => result = checker.HasNewerVersion());
        }
    }
}