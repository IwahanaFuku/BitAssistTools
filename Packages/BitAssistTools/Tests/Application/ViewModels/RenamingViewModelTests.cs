using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Domain.Models.Renaming;

namespace BitAssistTools.Tests.Application.ViewModels
{
    [TestFixture]
    public class RenamingViewModelTests
    {
        private RenamingViewModel _viewModel;
        private GameObject _testObject;

        [SetUp]
        public void SetUp()
        {
            _viewModel = new RenamingViewModel();
            _testObject = new GameObject("TestObject");
        }

        [TearDown]
        public void TearDown()
        {
            _viewModel?.Dispose();
            _viewModel = null;
            
            if (_testObject != null) 
            {
                UnityEngine.Object.DestroyImmediate(_testObject);
            }
            
            // 選択状態をクリア
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
        }

        [Test]
        public void Constructor_InitializesAllProperties()
        {
            // Assert
            Assert.IsNotNull(_viewModel.UI);
            Assert.IsNotNull(_viewModel.PrefixSuffix);
            Assert.IsNotNull(_viewModel.StringReplace);
        }

        [Test]
        public void Constructor_SetsDefaultValues()
        {
            // Assert
            Assert.IsTrue(_viewModel.UI.PrefixSuffix);
            Assert.IsTrue(_viewModel.UI.StringReplace);
            Assert.IsTrue(_viewModel.UI.TextureRename);
            Assert.IsTrue(_viewModel.UI.ObjectNumber);
            Assert.IsTrue(_viewModel.UI.PrefabAsset);

            Assert.AreEqual("Prefix", _viewModel.PrefixSuffix.Prefix);
            Assert.AreEqual("Suffix", _viewModel.PrefixSuffix.Suffix);
            Assert.AreEqual("_", _viewModel.PrefixSuffix.Delimiter);

            Assert.AreEqual("", _viewModel.StringReplace.SearchString);
            Assert.AreEqual("", _viewModel.StringReplace.ReplaceString);
        }

        [Test]
        public void UpdatePrefix_UpdatesValueAndNotifiesUIChange()
        {
            // Arrange
            bool uiStateChanged = false;
            _viewModel.OnUIStateChanged += () => uiStateChanged = true;

            // Act
            _viewModel.UpdatePrefix("NewPrefix");

            // Assert
            Assert.AreEqual("NewPrefix", _viewModel.PrefixSuffix.Prefix);
            Assert.IsTrue(uiStateChanged);
        }

        [Test]
        public void UpdateSuffix_UpdatesValueAndNotifiesUIChange()
        {
            // Arrange
            bool uiStateChanged = false;
            _viewModel.OnUIStateChanged += () => uiStateChanged = true;

            // Act
            _viewModel.UpdateSuffix("NewSuffix");

            // Assert
            Assert.AreEqual("NewSuffix", _viewModel.PrefixSuffix.Suffix);
            Assert.IsTrue(uiStateChanged);
        }

        [Test]
        public void UpdateDelimiter_UpdatesValueAndNotifiesUIChange()
        {
            // Arrange
            bool uiStateChanged = false;
            _viewModel.OnUIStateChanged += () => uiStateChanged = true;

            // Act
            _viewModel.UpdateDelimiter("-");

            // Assert
            Assert.AreEqual("-", _viewModel.PrefixSuffix.Delimiter);
            Assert.IsTrue(uiStateChanged);
        }

        [Test]
        public void UpdateSearchString_UpdatesValueAndNotifiesUIChange()
        {
            // Arrange
            bool uiStateChanged = false;
            _viewModel.OnUIStateChanged += () => uiStateChanged = true;

            // Act
            _viewModel.UpdateSearchString("SearchTerm");

            // Assert
            Assert.AreEqual("SearchTerm", _viewModel.StringReplace.SearchString);
            Assert.IsTrue(uiStateChanged);
        }

        [Test]
        public void UpdateReplaceString_UpdatesValueAndNotifiesUIChange()
        {
            // Arrange
            bool uiStateChanged = false;
            _viewModel.OnUIStateChanged += () => uiStateChanged = true;

            // Act
            _viewModel.UpdateReplaceString("ReplaceTerm");

            // Assert
            Assert.AreEqual("ReplaceTerm", _viewModel.StringReplace.ReplaceString);
            Assert.IsTrue(uiStateChanged);
        }

        [Test]
        public void NotifyUIStateChanged_InvokesEvent()
        {
            // Arrange
            bool eventInvoked = false;
            _viewModel.OnUIStateChanged += () => eventInvoked = true;

            // Act
            _viewModel.NotifyUIStateChanged();

            // Assert
            Assert.IsTrue(eventInvoked);
        }

        [Test]
        public void ExecutePrefixSuffixOperation_WithValidSelection_ExecutesOperation()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };
            string successMessage = null;
            _viewModel.OnSuccess += (message) => successMessage = message;

            // Act
            _viewModel.ExecutePrefixSuffixOperation();

            // Assert
            Assert.AreEqual("Prefix_TestObject_Suffix", _testObject.name);
            Assert.IsNotNull(successMessage);
            Assert.IsTrue(successMessage.Contains("接頭辞・接尾辞を追加しました"));
        }

        [Test]
        public void ExecutePrefixSuffixOperation_WithInvalidRule_NotifiesError()
        {
            // Arrange
            _viewModel.UpdatePrefix("");
            _viewModel.UpdateSuffix("");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };
            string errorMessage = null;
            _viewModel.OnError += (message) => errorMessage = message;

            // Act
            _viewModel.ExecutePrefixSuffixOperation();

            // Assert
            Assert.AreEqual("接頭辞または接尾辞を入力してください。", errorMessage);
            Assert.AreEqual("TestObject", _testObject.name); // 名前は変更されない
        }

        [Test]
        public void ExecutePrefixSuffixOperation_WithNoSelection_NotifiesError()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
            string errorMessage = null;
            _viewModel.OnError += (message) => errorMessage = message;

            // Act
            _viewModel.ExecutePrefixSuffixOperation();

            // Assert
            Assert.AreEqual("リネームするGameObjectを選択してください。", errorMessage);
        }

        [Test]
        public void ExecuteStringReplaceOperation_WithValidSelection_ExecutesOperation()
        {
            // Arrange
            _viewModel.UpdateSearchString("Test");
            _viewModel.UpdateReplaceString("Modified");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };
            string successMessage = null;
            _viewModel.OnSuccess += (message) => successMessage = message;

            // Act
            _viewModel.ExecuteStringReplaceOperation();

            // Assert
            Assert.AreEqual("ModifiedObject", _testObject.name);
            Assert.IsNotNull(successMessage);
            Assert.IsTrue(successMessage.Contains("文字列を置換しました"));
        }

        [Test]
        public void ExecuteStringReplaceOperation_WithInvalidRule_NotifiesError()
        {
            // Arrange
            _viewModel.UpdateSearchString("");
            _viewModel.UpdateReplaceString("Replacement");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };
            string errorMessage = null;
            _viewModel.OnError += (message) => errorMessage = message;

            // Act
            _viewModel.ExecuteStringReplaceOperation();

            // Assert
            Assert.AreEqual("検索文字列を入力してください。", errorMessage);
            Assert.AreEqual("TestObject", _testObject.name); // 名前は変更されない
        }

        [Test]
        public void ExecuteStringReplaceOperation_WithNoSelection_NotifiesError()
        {
            // Arrange
            _viewModel.UpdateSearchString("Test");
            _viewModel.UpdateReplaceString("Modified");
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
            string errorMessage = null;
            _viewModel.OnError += (message) => errorMessage = message;

            // Act
            _viewModel.ExecuteStringReplaceOperation();

            // Assert
            Assert.AreEqual("リネームするGameObjectを選択してください。", errorMessage);
        }

        [Test]
        public void ExecuteAddNumberOperation_WithValidSelection_ExecutesOperation()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };
            string successMessage = null;
            _viewModel.OnSuccess += (message) => successMessage = message;

            // Act
            _viewModel.ExecuteAddNumberOperation();

            // Assert
            Assert.AreEqual("TestObject (1)", _testObject.name);
            Assert.IsNotNull(successMessage);
            Assert.IsTrue(successMessage.Contains("オブジェクトに番号を付与しました"));
        }

        [Test]
        public void ExecuteAddNumberOperation_WithNoSelection_NotifiesError()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
            string errorMessage = null;
            _viewModel.OnError += (message) => errorMessage = message;

            // Act
            _viewModel.ExecuteAddNumberOperation();

            // Assert
            Assert.AreEqual("番号を付与するGameObjectを選択してください。", errorMessage);
        }

        [Test]
        public void ExecuteRemoveNumberOperation_WithValidSelection_ExecutesOperation()
        {
            // Arrange
            _testObject.name = "TestObject (1)";
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };
            string successMessage = null;
            _viewModel.OnSuccess += (message) => successMessage = message;

            // Act
            _viewModel.ExecuteRemoveNumberOperation();

            // Assert
            Assert.AreEqual("TestObject", _testObject.name);
            Assert.IsNotNull(successMessage);
            Assert.IsTrue(successMessage.Contains("オブジェクトから番号を除去しました"));
        }

        [Test]
        public void ExecuteRemoveNumberOperation_WithNoSelection_NotifiesError()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];
            string errorMessage = null;
            _viewModel.OnError += (message) => errorMessage = message;

            // Act
            _viewModel.ExecuteRemoveNumberOperation();

            // Assert
            Assert.AreEqual("番号を除去するGameObjectを選択してください。", errorMessage);
        }

        [Test]
        public void CanExecuteOperation_DelegatesToService()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };

            // Act & Assert
            Assert.IsTrue(_viewModel.CanExecuteOperation(RenameOperationType.PrefixSuffix));
            Assert.IsTrue(_viewModel.CanExecuteOperation(RenameOperationType.StringReplace));
            Assert.IsTrue(_viewModel.CanExecuteOperation(RenameOperationType.AddNumber));
            Assert.IsTrue(_viewModel.CanExecuteOperation(RenameOperationType.RemoveNumber));
        }

        [Test]
        public void CanExecuteOperation_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[0];

            // Act & Assert
            Assert.IsFalse(_viewModel.CanExecuteOperation(RenameOperationType.PrefixSuffix));
            Assert.IsFalse(_viewModel.CanExecuteOperation(RenameOperationType.StringReplace));
            Assert.IsFalse(_viewModel.CanExecuteOperation(RenameOperationType.AddNumber));
            Assert.IsFalse(_viewModel.CanExecuteOperation(RenameOperationType.RemoveNumber));
        }

        [Test]
        public void GetCurrentStats_DelegatesToService()
        {
            // Arrange
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };

            // Act
            var stats = _viewModel.GetCurrentStats();

            // Assert
            Assert.AreEqual(1, stats.GameObjectCount);
            Assert.AreEqual(0, stats.MaterialCount);
            Assert.IsTrue(stats.HasSelection);
        }

        [Test]
        public void PrefixSuffixSettings_CanBeModifiedDirectly()
        {
            // Arrange & Act
            _viewModel.PrefixSuffix.Prefix = "NewPrefix";
            _viewModel.PrefixSuffix.Suffix = "NewSuffix";
            _viewModel.PrefixSuffix.Delimiter = "-";

            // Assert
            Assert.AreEqual("NewPrefix", _viewModel.PrefixSuffix.Prefix);
            Assert.AreEqual("NewSuffix", _viewModel.PrefixSuffix.Suffix);
            Assert.AreEqual("-", _viewModel.PrefixSuffix.Delimiter);
        }

        [Test]
        public void StringReplaceSettings_CanBeModifiedDirectly()
        {
            // Arrange & Act
            _viewModel.StringReplace.SearchString = "Search";
            _viewModel.StringReplace.ReplaceString = "Replace";

            // Assert
            Assert.AreEqual("Search", _viewModel.StringReplace.SearchString);
            Assert.AreEqual("Replace", _viewModel.StringReplace.ReplaceString);
        }

        [Test]
        public void UIExpansionState_CanBeModified()
        {
            // Arrange & Act
            _viewModel.UI.PrefixSuffix = false;
            _viewModel.UI.StringReplace = false;
            _viewModel.UI.TextureRename = false;
            _viewModel.UI.ObjectNumber = false;
            _viewModel.UI.PrefabAsset = false;

            // Assert
            Assert.IsFalse(_viewModel.UI.PrefixSuffix);
            Assert.IsFalse(_viewModel.UI.StringReplace);
            Assert.IsFalse(_viewModel.UI.TextureRename);
            Assert.IsFalse(_viewModel.UI.ObjectNumber);
            Assert.IsFalse(_viewModel.UI.PrefabAsset);
        }

        [Test]
        public void OnErrorEvent_CanBeSubscribedAndUnsubscribed()
        {
            // Arrange
            string receivedMessage = null;
            Action<string> handler = (message) => receivedMessage = message;

            // Act - Subscribe
            _viewModel.OnError += handler;
            _viewModel.UpdatePrefix("");
            _viewModel.UpdateSuffix("");
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };
            _viewModel.ExecutePrefixSuffixOperation();

            // Assert - Event was received
            Assert.AreEqual("接頭辞または接尾辞を入力してください。", receivedMessage);

            // Act - Unsubscribe
            _viewModel.OnError -= handler;
            receivedMessage = null;
            _viewModel.ExecutePrefixSuffixOperation();

            // Assert - Event was not received
            Assert.IsNull(receivedMessage);
        }

        [Test]
        public void OnSuccessEvent_CanBeSubscribedAndUnsubscribed()
        {
            // Arrange
            string receivedMessage = null;
            Action<string> handler = (message) => receivedMessage = message;
            UnityEditor.Selection.objects = new UnityEngine.Object[] { _testObject };

            // Act - Subscribe
            _viewModel.OnSuccess += handler;
            _viewModel.ExecutePrefixSuffixOperation();

            // Assert - Event was received
            Assert.IsNotNull(receivedMessage);
            Assert.IsTrue(receivedMessage.Contains("接頭辞・接尾辞を追加しました"));

            // Act - Unsubscribe
            _viewModel.OnSuccess -= handler;
            receivedMessage = null;
            _testObject.name = "TestObject"; // Reset name
            _viewModel.ExecutePrefixSuffixOperation();

            // Assert - Event was not received
            Assert.IsNull(receivedMessage);
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _viewModel.Dispose());
        }
    }
}