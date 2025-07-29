using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Tests.Application.Services
{
    [TestFixture]
    public class SelectionHistoryServiceTests
    {
        private GameObject _testObject1;
        private GameObject _testObject2;
        private GameObject _testObject3;
        private SelectionHistory _emptyHistory;
        private SelectionHistory _historyWithItems;

        [SetUp]
        public void SetUp()
        {
            // テスト用のGameObjectを作成
            _testObject1 = new GameObject("TestObject1");
            _testObject2 = new GameObject("TestObject2");
            _testObject3 = new GameObject("TestObject3");

            // 履歴を初期化
            _emptyHistory = new SelectionHistory(5);
            _historyWithItems = new SelectionHistory(5);
            
            // テスト用の履歴項目を追加
            var selection1 = new SelectionSet("Test Selection 1", new UnityEngine.Object[] { _testObject1 });
            var selection2 = new SelectionSet("Test Selection 2", new UnityEngine.Object[] { _testObject2, _testObject3 });
            _historyWithItems.AddSelection(selection1);
            _historyWithItems.AddSelection(selection2);
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
        public void AddCurrentSelection_WithValidSelection_AddsToHistory()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1 };

            // Act
            var newHistory = SelectionHistoryService.AddCurrentSelection(_emptyHistory, 5);

            // Assert
            Assert.AreEqual(1, newHistory.Selections.Count);
            Assert.AreEqual("Selection 1", newHistory.Selections[0].Name);
            Assert.AreEqual(1, newHistory.Selections[0].Objects.Count);
            Assert.AreEqual(_testObject1, newHistory.Selections[0].Objects[0]);
        }

        [Test]
        public void AddCurrentSelection_WithNoSelection_LogsErrorAndReturnsOriginal()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Select one or more objects"));
            var newHistory = SelectionHistoryService.AddCurrentSelection(_emptyHistory, 5);

            // 元の履歴と同じインスタンスが返される
            Assert.AreSame(_emptyHistory, newHistory);
        }

        [Test]
        public void AddCurrentSelection_WithExistingHistory_PreservesExistingItems()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject3 };

            // Act
            var newHistory = SelectionHistoryService.AddCurrentSelection(_historyWithItems, 5);

            // Assert
            Assert.AreEqual(3, newHistory.Selections.Count); // 既存2個 + 新規1個
            Assert.AreEqual("Test Selection 1", newHistory.Selections[0].Name);
            Assert.AreEqual("Test Selection 2", newHistory.Selections[1].Name);
            Assert.AreEqual("Selection 3", newHistory.Selections[2].Name); // 自動生成される名前
            Assert.AreEqual(1, newHistory.Selections[2].Objects.Count);
            Assert.AreEqual(_testObject3, newHistory.Selections[2].Objects[0]);
        }

        [Test]
        public void LoadSelection_WithValidHistory_SetsSelection()
        {
            // Arrange
            var history = new SelectionHistory(5);
            var selection = new SelectionSet("Test", new UnityEngine.Object[] { _testObject1, _testObject2 });
            history.AddSelection(selection);

            // Act
            SelectionHistoryService.LoadSelection(history);

            // Assert
            var selectedObjects = UnityEditor.Selection.objects;
            Assert.AreEqual(2, selectedObjects.Length);
            Assert.IsTrue(selectedObjects.Contains(_testObject1));
            Assert.IsTrue(selectedObjects.Contains(_testObject2));
        }

        [Test]
        public void LoadSelection_WithEmptyHistory_LogsWarning()
        {
            // Arrange
            var emptyHistory = new SelectionHistory(5);

            // Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("アクティブな選択履歴がありません"));
            SelectionHistoryService.LoadSelection(emptyHistory);
        }

        [Test]
        public void DeleteSelectedItems_WithValidSelections_RemovesSelectedItems()
        {
            // Arrange
            var selectionStates = new List<bool> { true, false }; // 最初のアイテムを削除

            // Act
            var newHistory = SelectionHistoryService.DeleteSelectedItems(_historyWithItems, selectionStates);

            // Assert
            Assert.AreEqual(1, newHistory.Selections.Count);
            Assert.AreEqual("Test Selection 2", newHistory.Selections[0].Name);
        }

        [Test]
        public void DeleteSelectedItems_WithAllSelected_RemovesAllItems()
        {
            // Arrange
            var selectionStates = new List<bool> { true, true };

            // Act
            var newHistory = SelectionHistoryService.DeleteSelectedItems(_historyWithItems, selectionStates);

            // Assert
            Assert.AreEqual(0, newHistory.Selections.Count);
        }

        [Test]
        public void DeleteSelectedItems_WithNoneSelected_PreservesAllItems()
        {
            // Arrange
            var selectionStates = new List<bool> { false, false };

            // Act
            var newHistory = SelectionHistoryService.DeleteSelectedItems(_historyWithItems, selectionStates);

            // Assert
            Assert.AreEqual(2, newHistory.Selections.Count);
            Assert.AreEqual("Test Selection 1", newHistory.Selections[0].Name);
            Assert.AreEqual("Test Selection 2", newHistory.Selections[1].Name);
        }

        [Test]
        public void ToggleAllSelections_WithMixedActiveStates_SetsAllToActive()
        {
            // Arrange
            var history = new SelectionHistory(5);
            var selection1 = new SelectionSet("Test 1", new UnityEngine.Object[] { _testObject1 }, false);
            var selection2 = new SelectionSet("Test 2", new UnityEngine.Object[] { _testObject2 }, true);
            history.AddSelection(selection1);
            history.AddSelection(selection2);

            // Act
            var newHistory = SelectionHistoryService.ToggleAllSelections(history);

            // Assert
            Assert.IsTrue(newHistory.Selections.All(s => s.IsActive));
        }

        [Test]
        public void ToggleAllSelections_WithAllActive_SetsAllToInactive()
        {
            // Arrange
            var history = new SelectionHistory(5);
            var selection1 = new SelectionSet("Test 1", new UnityEngine.Object[] { _testObject1 }, true);
            var selection2 = new SelectionSet("Test 2", new UnityEngine.Object[] { _testObject2 }, true);
            history.AddSelection(selection1);
            history.AddSelection(selection2);

            // Act
            var newHistory = SelectionHistoryService.ToggleAllSelections(history);

            // Assert
            Assert.IsTrue(newHistory.Selections.All(s => !s.IsActive));
        }

        [Test]
        public void ToggleSelectionActive_WithValidIndex_TogglesSpecificItem()
        {
            // Arrange
            var originalActiveState = _historyWithItems.Selections[0].IsActive;

            // Act
            var newHistory = SelectionHistoryService.ToggleSelectionActive(_historyWithItems, 0);

            // Assert
            Assert.AreEqual(!originalActiveState, newHistory.Selections[0].IsActive);
            Assert.AreEqual(_historyWithItems.Selections[1].IsActive, newHistory.Selections[1].IsActive); // 他は変更されない
        }

        [Test]
        public void ToggleSelectionActive_WithInvalidIndex_LogsWarningAndReturnsOriginal()
        {
            // Arrange & Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("無効なインデックスです"));
            var newHistory = SelectionHistoryService.ToggleSelectionActive(_historyWithItems, 10);

            Assert.AreSame(_historyWithItems, newHistory);
        }

        [Test]
        public void UpdateSelectionName_WithValidIndexAndName_UpdatesName()
        {
            // Arrange
            string newName = "Updated Name";

            // Act
            var newHistory = SelectionHistoryService.UpdateSelectionName(_historyWithItems, 0, newName);

            // Assert
            Assert.AreEqual(newName, newHistory.Selections[0].Name);
            Assert.AreEqual("Test Selection 2", newHistory.Selections[1].Name); // 他は変更されない
        }

        [Test]
        public void UpdateSelectionName_WithInvalidIndex_LogsWarningAndReturnsOriginal()
        {
            // Arrange & Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("無効なインデックスです"));
            var newHistory = SelectionHistoryService.UpdateSelectionName(_historyWithItems, 10, "New Name");

            Assert.AreSame(_historyWithItems, newHistory);
        }

        [Test]
        public void UpdateSelectionName_WithEmptyName_LogsWarningAndReturnsOriginal()
        {
            // Arrange & Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("名前は空にできません"));
            var newHistory = SelectionHistoryService.UpdateSelectionName(_historyWithItems, 0, "");

            Assert.AreSame(_historyWithItems, newHistory);
        }

        [Test]
        public void UpdateSelectionName_WithWhitespaceOnlyName_LogsWarningAndReturnsOriginal()
        {
            // Arrange & Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("名前は空にできません"));
            var newHistory = SelectionHistoryService.UpdateSelectionName(_historyWithItems, 0, "   ");

            Assert.AreSame(_historyWithItems, newHistory);
        }

        [Test]
        public void CleanupInvalidReferences_WithValidObjects_PreservesAllSelections()
        {
            // Arrange - 全て有効なオブジェクト

            // Act
            var newHistory = SelectionHistoryService.CleanupInvalidReferences(_historyWithItems);

            // Assert
            Assert.AreEqual(2, newHistory.Selections.Count);
            Assert.AreEqual("Test Selection 1", newHistory.Selections[0].Name);
            Assert.AreEqual("Test Selection 2", newHistory.Selections[1].Name);
        }

        [Test]
        public void CleanupInvalidReferences_WithMixedValidInvalidObjects_RemovesInvalidOnly()
        {
            // Arrange
            var destroyedObject = new GameObject("ToBeDestroyed");
            var history = new SelectionHistory(5);
            var validSelection = new SelectionSet("Valid", new UnityEngine.Object[] { _testObject1 });
            var mixedSelection = new SelectionSet("Mixed", new UnityEngine.Object[] { _testObject2, destroyedObject });
            history.AddSelection(validSelection);
            history.AddSelection(mixedSelection);

            // オブジェクトを削除して無効にする
            UnityEngine.Object.DestroyImmediate(destroyedObject);

            // Act
            var newHistory = SelectionHistoryService.CleanupInvalidReferences(history);

            // Assert
            Assert.AreEqual(2, newHistory.Selections.Count);
            Assert.AreEqual(1, newHistory.Selections[0].Objects.Count); // Valid selection unchanged
            Assert.AreEqual(1, newHistory.Selections[1].Objects.Count); // Mixed selection should have only valid object
            Assert.AreEqual(_testObject2, newHistory.Selections[1].Objects[0]);
        }

        [Test]
        public void CleanupInvalidReferences_WithEmptyHistory_ReturnsEmptyHistory()
        {
            // Arrange & Act
            var newHistory = SelectionHistoryService.CleanupInvalidReferences(_emptyHistory);

            // Assert
            Assert.AreEqual(0, newHistory.Selections.Count);
        }

        [Test]
        public void AddCurrentSelection_WithMultipleObjects_CreatesCorrectSelection()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject1, _testObject2, _testObject3 };

            // Act
            var newHistory = SelectionHistoryService.AddCurrentSelection(_emptyHistory, 5);

            // Assert
            Assert.AreEqual(1, newHistory.Selections.Count);
            Assert.AreEqual("Selection 1", newHistory.Selections[0].Name);
            Assert.AreEqual(3, newHistory.Selections[0].Objects.Count);
            Assert.IsTrue(newHistory.Selections[0].Objects.Contains(_testObject1));
            Assert.IsTrue(newHistory.Selections[0].Objects.Contains(_testObject2));
            Assert.IsTrue(newHistory.Selections[0].Objects.Contains(_testObject3));
        }

        [Test]
        public void LoadSelection_WithInactiveSelections_DoesNotLoadInactiveItems()
        {
            // Arrange
            var history = new SelectionHistory(5);
            var activeSelection = new SelectionSet("Active", new UnityEngine.Object[] { _testObject1 }, true);
            var inactiveSelection = new SelectionSet("Inactive", new UnityEngine.Object[] { _testObject2 }, false);
            history.AddSelection(activeSelection);
            history.AddSelection(inactiveSelection);

            // Act
            SelectionHistoryService.LoadSelection(history);

            // Assert
            var selectedObjects = UnityEditor.Selection.objects;
            Assert.AreEqual(1, selectedObjects.Length);
            Assert.AreEqual(_testObject1, selectedObjects[0]);
        }
    }
}