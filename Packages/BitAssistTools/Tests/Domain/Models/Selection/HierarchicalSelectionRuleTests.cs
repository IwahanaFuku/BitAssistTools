
using NUnit.Framework;
using UnityEngine;
using BitAssistTools.Domain.Models.Selection;

namespace BitAssistTools.Tests.Domain.Models.Selection
{
    [TestFixture]
    public class HierarchicalSelectionRuleTests
    {
        private GameObject _root;
        private GameObject _child1;
        private GameObject _child2;
        private GameObject _grandchild1;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root");
            _child1 = new GameObject("Child1");
            _child2 = new GameObject("Child2");
            _grandchild1 = new GameObject("Grandchild1");

            _child1.transform.SetParent(_root.transform);
            _child2.transform.SetParent(_root.transform);
            _grandchild1.transform.SetParent(_child1.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void CanApplyTo_WithSufficientParentDepth_ReturnsTrue()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, true, false);

            // Act & Assert
            Assert.IsTrue(rule.CanApplyTo(_child1));
        }

        [Test]
        public void CanApplyTo_WithInsufficientParentDepth_ReturnsFalse()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(2, true, false);

            // Act & Assert
            Assert.IsFalse(rule.CanApplyTo(_child1));
        }

        [Test]
        public void GetParentObject_ReturnsCorrectParent()
        {
            // Arrange
            var rule = new HierarchicalSelectionRule(1, true, false);

            // Act
            var parent = rule.GetParentObject(_grandchild1);

            // Assert
            Assert.AreEqual(_child1, parent);
        }
    }
}
