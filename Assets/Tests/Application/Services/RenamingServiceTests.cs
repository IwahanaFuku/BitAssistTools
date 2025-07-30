using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Renaming;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class RenamingServiceTests
    {
        private GameObject _testObject1;
        private GameObject _testObject2;
        private GameObject _testObject3;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _testObject1 = new GameObject("TestObject");
            _testObject2 = new GameObject("AnotherTest");
            _testObject3 = new GameObject("ThirdObject (1)");
        }

        [TearDown]
        public void TearDown()
        {
            // テスト用オブジェクトを削除
            if (_testObject1 != null) UnityEngine.Object.DestroyImmediate(_testObject1);
            if (_testObject2 != null) UnityEngine.Object.DestroyImmediate(_testObject2);
            if (_testObject3 != null) UnityEngine.Object.DestroyImmediate(_testObject3);

            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void AddPrefixAndSuffix_WithValidRule_RenamesSelectedObjects()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Pre", "Suf", "_");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            RenamingService.AddPrefixAndSuffix(rule);

            // Assert
            Assert.AreEqual("Pre_TestObject_Suf", _testObject1.name);
            Assert.AreEqual("Pre_AnotherTest_Suf", _testObject2.name);
        }

        [Test]
        public void AddPrefixAndSuffix_WithInvalidRule_LogsError()
        {
            // Arrange
            var rule = new PrefixSuffixRule("", "", "_");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act & Assert - ログメッセージを期待
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("無効な接頭辞・接尾辞ルールです"));
            RenamingService.AddPrefixAndSuffix(rule);
            
            // 無効なルールでは名前が変更されないことを確認
            Assert.AreEqual("TestObject", _testObject1.name);
        }

        [Test]
        public void AddPrefixAndSuffix_WithNoSelection_LogsError()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Pre", "Suf", "_");
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert - ログメッセージを期待
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("リネームするGameObjectを選択してください"));
            RenamingService.AddPrefixAndSuffix(rule);
        }

        [Test]
        public void ReplaceString_WithValidRule_ReplacesStringsInSelectedObjects()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "Modified");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            RenamingService.ReplaceString(rule);

            // Assert
            Assert.AreEqual("ModifiedObject", _testObject1.name);
            Assert.AreEqual("AnotherModified", _testObject2.name);
        }

        [Test]
        public void ReplaceString_WithInvalidRule_LogsError()
        {
            // Arrange
            var rule = new StringReplaceRule("", "Replacement");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act & Assert - ログメッセージを期待
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("無効な文字列置換ルールです"));
            RenamingService.ReplaceString(rule);
            
            // 無効なルールでは名前が変更されないことを確認
            Assert.AreEqual("TestObject", _testObject1.name);
        }

        [Test]
        public void ReplaceString_WithNoMatchingStrings_DoesNotChangeNames()
        {
            // Arrange
            var rule = new StringReplaceRule("NonExistent", "Replacement");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            RenamingService.ReplaceString(rule);

            // Assert
            Assert.AreEqual("TestObject", _testObject1.name);
            Assert.AreEqual("AnotherTest", _testObject2.name);
        }

        [Test]
        public void AddObjectNumber_WithSelectedObjects_AddsNumbersToNames()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            RenamingService.AddObjectNumber();

            // Assert
            Assert.AreEqual("TestObject (1)", _testObject1.name);
            Assert.AreEqual("AnotherTest (1)", _testObject2.name);
        }

        [Test]
        public void AddObjectNumber_WithDuplicateNames_AddsIncrementalNumbers()
        {
            // Arrange
            _testObject2.name = "TestObject"; // 同じ名前にする
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            RenamingService.AddObjectNumber();

            // Assert
            Assert.AreEqual("TestObject (1)", _testObject1.name);
            Assert.AreEqual("TestObject (2)", _testObject2.name);
        }

        [Test]
        public void RemoveObjectNumber_WithNumberedObjects_RemovesNumbers()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject3 };

            // Act
            RenamingService.RemoveObjectNumber();

            // Assert
            Assert.AreEqual("ThirdObject", _testObject3.name);
        }

        [Test]
        public void RemoveObjectNumber_WithoutNumbers_DoesNotChangeNames()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            RenamingService.RemoveObjectNumber();

            // Assert
            Assert.AreEqual("TestObject", _testObject1.name);
        }

        [Test]
        public void CanExecuteOperation_WithGameObjectsSelected_ReturnsTrue()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act & Assert
            Assert.IsTrue(RenamingService.CanExecuteOperation(RenameOperationType.PrefixSuffix));
            Assert.IsTrue(RenamingService.CanExecuteOperation(RenameOperationType.StringReplace));
            Assert.IsTrue(RenamingService.CanExecuteOperation(RenameOperationType.AddNumber));
            Assert.IsTrue(RenamingService.CanExecuteOperation(RenameOperationType.RemoveNumber));
        }

        [Test]
        public void CanExecuteOperation_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.IsFalse(RenamingService.CanExecuteOperation(RenameOperationType.PrefixSuffix));
            Assert.IsFalse(RenamingService.CanExecuteOperation(RenameOperationType.StringReplace));
            Assert.IsFalse(RenamingService.CanExecuteOperation(RenameOperationType.AddNumber));
            Assert.IsFalse(RenamingService.CanExecuteOperation(RenameOperationType.RemoveNumber));
        }

        [Test]
        public void GetRenamingStats_WithGameObjectsSelected_ReturnsCorrectStats()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2 };

            // Act
            var stats = RenamingService.GetRenamingStats();

            // Assert
            Assert.AreEqual(2, stats.GameObjectCount);
            Assert.AreEqual(0, stats.MaterialCount);
            Assert.AreEqual(0, stats.PrefabCount);
            Assert.AreEqual(0, stats.RendererObjectCount);
            Assert.IsTrue(stats.HasSelection);
        }

        [Test]
        public void GetRenamingStats_WithNoSelection_ReturnsEmptyStats()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act
            var stats = RenamingService.GetRenamingStats();

            // Assert
            Assert.AreEqual(0, stats.GameObjectCount);
            Assert.AreEqual(0, stats.MaterialCount);
            Assert.AreEqual(0, stats.PrefabCount);
            Assert.AreEqual(0, stats.RendererObjectCount);
            Assert.IsFalse(stats.HasSelection);
        }

        [Test]
        public void RenamingStats_ToString_WithSelection_ReturnsFormattedString()
        {
            // Arrange
            var stats = new RenamingService.RenamingStats(2, 1, 0, 1, 0);

            // Act
            var result = stats.ToString();

            // Assert
            Assert.IsTrue(result.Contains("GameObject: 2個"));
            Assert.IsTrue(result.Contains("Material: 1個"));
            Assert.IsTrue(result.Contains("Renderer付き: 1個"));
            Assert.IsFalse(result.Contains("Prefab"));
        }

        [Test]
        public void RenamingStats_ToString_WithNoSelection_ReturnsNoSelectionMessage()
        {
            // Arrange
            var stats = new RenamingService.RenamingStats(0, 0, 0, 0, 0);

            // Act
            var result = stats.ToString();

            // Assert
            Assert.AreEqual("何も選択されていません", result);
        }


        [Test]
        public void CanExecuteOperation_WithInvalidOperationType_ReturnsFalse()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act & Assert
            Assert.IsFalse(RenamingService.CanExecuteOperation((RenameOperationType)999));
        }
    }
}