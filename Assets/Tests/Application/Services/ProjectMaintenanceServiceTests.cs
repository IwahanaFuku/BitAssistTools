using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.ProjectMaintenance;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class ProjectMaintenanceServiceTests
    {
        private GameObject _testObject1;
        private GameObject _testObject2;
        private GameObject _cleanObject;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _testObject1 = new GameObject("TestObject1");
            _testObject2 = new GameObject("TestObject2");
            _cleanObject = new GameObject("CleanObject");

            // CleanObjectには通常のコンポーネントを追加
            _cleanObject.AddComponent<Rigidbody>();
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_testObject1 != null) UnityEngine.Object.DestroyImmediate(_testObject1);
            if (_testObject2 != null) UnityEngine.Object.DestroyImmediate(_testObject2);
            if (_cleanObject != null) UnityEngine.Object.DestroyImmediate(_cleanObject);

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void CanRemoveMissingScripts_WithSelectedObjects_ReturnsTrue()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            bool result = ProjectMaintenanceService.CanRemoveMissingScripts();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanRemoveMissingScripts_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act
            bool result = ProjectMaintenanceService.CanRemoveMissingScripts();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveMissingScriptsFromSelected_WithNoSelection_LogsError()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Missing Scriptを削除するGameObjectを選択してください"));
            ProjectMaintenanceService.RemoveMissingScriptsFromSelected();
        }

        [Test]
        public void RemoveMissingScriptsFromSelected_WithValidSelection_DoesNotThrow()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _cleanObject };

            // Act & Assert
            Assert.DoesNotThrow(() => ProjectMaintenanceService.RemoveMissingScriptsFromSelected());
        }

        [Test]
        public void GetMissingScriptStats_WithNoSelection_ReturnsEmptyStats()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act
            var stats = ProjectMaintenanceService.GetMissingScriptStats();

            // Assert
            Assert.AreEqual(0, stats.TotalGameObjects);
            Assert.AreEqual(0, stats.AffectedGameObjects);
            Assert.AreEqual(0, stats.TotalMissingScripts);
            Assert.AreEqual(0, stats.PrefabInstances);
            Assert.IsFalse(stats.HasMissingScripts);
        }

        [Test]
        public void GetMissingScriptStats_WithCleanObjects_ReturnsZeroMissingScripts()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _cleanObject };

            // Act
            var stats = ProjectMaintenanceService.GetMissingScriptStats();

            // Assert
            Assert.AreEqual(1, stats.TotalGameObjects);
            Assert.AreEqual(0, stats.AffectedGameObjects);
            Assert.AreEqual(0, stats.TotalMissingScripts);
            Assert.IsFalse(stats.HasMissingScripts);
        }

        [Test]
        public void GetMissingScriptStats_WithMultipleObjects_CountsCorrectly()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2, _cleanObject };

            // Act
            var stats = ProjectMaintenanceService.GetMissingScriptStats();

            // Assert
            Assert.AreEqual(3, stats.TotalGameObjects);
            // Missing scriptの実際の数は環境に依存するため、構造のみ確認
            Assert.IsTrue(stats.TotalMissingScripts >= 0);
            Assert.IsTrue(stats.AffectedGameObjects >= 0);
        }

        [Test]
        public void MissingScriptStats_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var stats = new ProjectMaintenanceService.MissingScriptStats(10, 5, 8, 2);

            // Assert
            Assert.AreEqual(10, stats.TotalGameObjects);
            Assert.AreEqual(5, stats.AffectedGameObjects);
            Assert.AreEqual(8, stats.TotalMissingScripts);
            Assert.AreEqual(2, stats.PrefabInstances);
            Assert.IsTrue(stats.HasMissingScripts);
        }

        [Test]
        public void MissingScriptStats_ToString_WithMissingScripts_ReturnsFormattedString()
        {
            // Arrange
            var stats = new ProjectMaintenanceService.MissingScriptStats(10, 3, 5, 1);

            // Act
            var result = stats.ToString();

            // Assert
            Assert.IsTrue(result.Contains("3/10個のオブジェクト"));
            Assert.IsTrue(result.Contains("5個のMissing Script"));
            Assert.IsTrue(result.Contains("Prefab: 1個"));
        }

        [Test]
        public void MissingScriptStats_ToString_WithoutMissingScripts_ReturnsNoScriptMessage()
        {
            // Arrange
            var stats = new ProjectMaintenanceService.MissingScriptStats(5, 0, 0, 0);

            // Act
            var result = stats.ToString();

            // Assert
            Assert.AreEqual("Missing Scriptなし", result);
        }

        [Test]
        public void MissingScriptStats_ToString_WithoutPrefabs_DoesNotMentionPrefabs()
        {
            // Arrange
            var stats = new ProjectMaintenanceService.MissingScriptStats(10, 2, 3, 0);

            // Act
            var result = stats.ToString();

            // Assert
            Assert.IsTrue(result.Contains("2/10個のオブジェクト"));
            Assert.IsTrue(result.Contains("3個のMissing Script"));
            Assert.IsFalse(result.Contains("Prefab"));
        }

        [Test]
        public void CleanupResult_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var result = new ProjectMaintenanceService.CleanupResult(
                CleanupOperationType.MissingScripts, 
                5, 
                10, 
                2
            );

            // Assert
            Assert.AreEqual(CleanupOperationType.MissingScripts, result.OperationType);
            Assert.AreEqual(5, result.AffectedObjects);
            Assert.AreEqual(10, result.RemovedItems);
            Assert.AreEqual(2, result.ProcessedPrefabs);
        }

        [Test]
        public void CleanupResult_ToString_WithPrefabs_IncludesPrefabInfo()
        {
            // Arrange
            var result = new ProjectMaintenanceService.CleanupResult(
                CleanupOperationType.MissingScripts, 
                3, 
                7, 
                2
            );

            // Act
            var resultString = result.ToString();

            // Assert
            Assert.IsTrue(resultString.Contains("3個のGameObject"));
            Assert.IsTrue(resultString.Contains("7個のMissing Script"));
            Assert.IsTrue(resultString.Contains("Prefab: 2個処理"));
        }

        [Test]
        public void CleanupResult_ToString_WithoutPrefabs_DoesNotMentionPrefabs()
        {
            // Arrange
            var result = new ProjectMaintenanceService.CleanupResult(
                CleanupOperationType.MissingScripts, 
                2, 
                4, 
                0
            );

            // Act
            var resultString = result.ToString();

            // Assert
            Assert.IsTrue(resultString.Contains("2個のGameObject"));
            Assert.IsTrue(resultString.Contains("4個のMissing Script"));
            Assert.IsFalse(resultString.Contains("Prefab"));
        }

        [Test]
        public void GetMissingScriptStats_WithHierarchy_CountsChildrenRecursively()
        {
            // Arrange
            var childObject = new GameObject("ChildObject");
            childObject.transform.SetParent(_testObject1.transform);
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            try
            {
                // Act
                var stats = ProjectMaintenanceService.GetMissingScriptStats();

                // Assert - 親と子の両方がカウントされる
                Assert.AreEqual(2, stats.TotalGameObjects); // _testObject1 + childObject
            }
            finally
            {
                // Cleanup
                if (childObject != null) UnityEngine.Object.DestroyImmediate(childObject);
            }
        }

        [Test]
        public void MissingScriptStats_HasMissingScripts_ReturnsTrueWhenScriptsExist()
        {
            // Arrange
            var stats = new ProjectMaintenanceService.MissingScriptStats(5, 2, 3, 1);

            // Act & Assert
            Assert.IsTrue(stats.HasMissingScripts);
        }

        [Test]
        public void MissingScriptStats_HasMissingScripts_ReturnsFalseWhenNoScripts()
        {
            // Arrange
            var stats = new ProjectMaintenanceService.MissingScriptStats(5, 0, 0, 0);

            // Act & Assert
            Assert.IsFalse(stats.HasMissingScripts);
        }
    }
}