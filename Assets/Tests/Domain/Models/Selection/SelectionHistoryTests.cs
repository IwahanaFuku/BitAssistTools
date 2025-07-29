using NUnit.Framework;
using UnityEngine;
using System.Linq;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Tests.Domain.Models.Selection
{
    [TestFixture]
    public class SelectionHistoryTests
    {
        private SelectionHistory _history;
        private GameObject[] _testObjects;

        [SetUp]
        public void SetUp()
        {
            _history = new SelectionHistory(5); // 最大5個の履歴
            
            // テスト用オブジェクトを作成
            _testObjects = new GameObject[]
            {
                new GameObject("TestObject1"),
                new GameObject("TestObject2"),
                new GameObject("TestObject3")
            };
        }

        [TearDown]
        public void TearDown()
        {
            // テストオブジェクトを削除
            foreach (var obj in _testObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Constructor_WithValidMaxCount_InitializesCorrectly()
        {
            // Arrange & Act
            var history = new SelectionHistory(10);

            // Assert
            Assert.AreEqual(10, history.MaxHistoryCount);
            Assert.AreEqual(0, history.CurrentCount);
            Assert.IsNotNull(history.Id);
            Assert.IsEmpty(history.Selections);
        }

        [Test]
        public void Constructor_WithInvalidMaxCount_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => new SelectionHistory(0));
            Assert.Throws<System.ArgumentException>(() => new SelectionHistory(-1));
        }

        [Test]
        public void AddSelection_WithValidSelectionSet_AddsToHistory()
        {
            // Arrange
            var selectionSet = new SelectionSet("Test Selection", _testObjects);

            // Act
            _history.AddSelection(selectionSet);

            // Assert
            Assert.AreEqual(1, _history.CurrentCount);
            Assert.AreEqual(selectionSet, _history.Selections[0]);
        }

        [Test]
        public void AddSelection_WithEmptySelectionSet_ThrowsArgumentException()
        {
            // Arrange
            var emptySelectionSet = new SelectionSet("Empty", new UnityEngine.Object[0]);

            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => _history.AddSelection(emptySelectionSet));
        }

        [Test]
        public void AddCurrentSelection_WithValidObjects_AddsToHistory()
        {
            // Act
            _history.AddCurrentSelection(_testObjects);

            // Assert
            Assert.AreEqual(1, _history.CurrentCount);
            Assert.AreEqual("Selection 1", _history.Selections[0].Name);
            Assert.AreEqual(3, _history.Selections[0].Count);
        }

        [Test]
        public void AddCurrentSelection_WithNullObjects_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => _history.AddCurrentSelection(null));
        }

        [Test]
        public void AddSelection_ExceedsMaxCount_RemovesOldestSelection()
        {
            // Arrange
            for (int i = 0; i < 7; i++) // 最大5個を超えて7個追加
            {
                var selectionSet = new SelectionSet($"Selection {i}", _testObjects);
                _history.AddSelection(selectionSet);
            }

            // Act & Assert
            Assert.AreEqual(5, _history.CurrentCount); // 最大数を維持
            Assert.AreEqual("Selection 2", _history.Selections[0].Name); // 最初の2個が削除されて、3番目が先頭
            Assert.AreEqual("Selection 6", _history.Selections[4].Name); // 最後が6番目
        }

        [Test]
        public void RemoveSelection_WithValidIndex_RemovesSuccessfully()
        {
            // Arrange
            _history.AddCurrentSelection(_testObjects);
            _history.AddCurrentSelection(_testObjects.Take(2).ToArray());

            // Act
            var result = _history.RemoveSelection(0);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _history.CurrentCount);
            Assert.AreEqual("Selection 2", _history.Selections[0].Name);
        }

        [Test]
        public void RemoveSelection_WithInvalidIndex_ReturnsFalse()
        {
            // Arrange
            _history.AddCurrentSelection(_testObjects);

            // Act & Assert
            Assert.IsFalse(_history.RemoveSelection(-1));
            Assert.IsFalse(_history.RemoveSelection(5));
            Assert.AreEqual(1, _history.CurrentCount); // 削除されていない
        }

        [Test]
        public void GetActiveSelections_ReturnsOnlyActiveSelections()
        {
            // Arrange
            var activeSelection = new SelectionSet("Active", _testObjects, true);
            var inactiveSelection = new SelectionSet("Inactive", _testObjects, false);
            
            _history.AddSelection(activeSelection);
            _history.AddSelection(inactiveSelection);

            // Act
            var activeSelections = _history.GetActiveSelections().ToList();

            // Assert
            Assert.AreEqual(1, activeSelections.Count);
            Assert.AreEqual("Active", activeSelections[0].Name);
        }

        [Test]
        public void GetAllActiveObjects_ReturnsUniqueObjectsFromActiveSelections()
        {
            // Arrange
            var selection1 = new SelectionSet("Selection1", new[] { _testObjects[0], _testObjects[1] }, true);
            var selection2 = new SelectionSet("Selection2", new[] { _testObjects[1], _testObjects[2] }, true);
            var inactiveSelection = new SelectionSet("Inactive", new[] { _testObjects[0] }, false);

            _history.AddSelection(selection1);
            _history.AddSelection(selection2);
            _history.AddSelection(inactiveSelection);

            // Act
            var allActiveObjects = _history.GetAllActiveObjects().ToList();

            // Assert
            Assert.AreEqual(3, allActiveObjects.Count); // ユニークなオブジェクト数
            Assert.Contains(_testObjects[0], allActiveObjects);
            Assert.Contains(_testObjects[1], allActiveObjects);
            Assert.Contains(_testObjects[2], allActiveObjects);
        }

        [Test]
        public void ToggleSelectionActive_WithValidIndex_TogglesActiveState()
        {
            // Arrange
            var selectionSet = new SelectionSet("Test", _testObjects, true);
            _history.AddSelection(selectionSet);

            // Act
            var result = _history.ToggleSelectionActive(0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(_history.Selections[0].IsActive);

            // Toggle again
            _history.ToggleSelectionActive(0);
            Assert.IsTrue(_history.Selections[0].IsActive);
        }

        [Test]
        public void Clear_RemovesAllSelections()
        {
            // Arrange
            _history.AddCurrentSelection(_testObjects);
            _history.AddCurrentSelection(_testObjects.Take(2).ToArray());

            // Act
            _history.Clear();

            // Assert
            Assert.AreEqual(0, _history.CurrentCount);
            Assert.IsEmpty(_history.Selections);
        }

        [Test]
        public void CleanupInvalidReferences_RemovesSelectionsWithNoValidObjects()
        {
            // Arrange
            var validObject = _testObjects[0];
            var validSelection = new SelectionSet("Valid", new[] { validObject });
            _history.AddSelection(validSelection);

            // 先にinvalidSelectionを作成（まだオブジェクトは有効）
            var invalidSelection = new SelectionSet("Invalid", new[] { _testObjects[1], _testObjects[2] });
            _history.AddSelection(invalidSelection);

            // その後でオブジェクトを削除してnull参照を作成
            Object.DestroyImmediate(_testObjects[1]);
            Object.DestroyImmediate(_testObjects[2]);

            // Act
            var removedCount = _history.CleanupInvalidReferences();

            // Assert
            Assert.AreEqual(1, removedCount);
            Assert.AreEqual(1, _history.CurrentCount);
            Assert.AreEqual("Valid", _history.Selections[0].Name);
        }

        [Test]
        public void GetStats_ReturnsCorrectStatistics()
        {
            // Arrange
            _history.AddCurrentSelection(_testObjects);
            _history.AddCurrentSelection(_testObjects.Take(2).ToArray());
            _history.ToggleSelectionActive(1); // 2番目を非アクティブに

            // Act
            var stats = _history.GetStats();

            // Assert
            Assert.AreEqual(2, stats.TotalSelections);
            Assert.AreEqual(1, stats.ActiveSelections);
            Assert.AreEqual(5, stats.TotalObjects); // 3 + 2
            Assert.AreEqual(3, stats.UniqueObjects); // ユニークオブジェクト数
        }

        [Test]
        public void LastModified_UpdatesOnModification()
        {
            // Arrange
            var initialTime = _history.LastModified;

            // Act
            System.Threading.Thread.Sleep(10); // 時間差を作るため
            _history.AddCurrentSelection(_testObjects);

            // Assert
            Assert.Greater(_history.LastModified, initialTime);
        }
    }
}