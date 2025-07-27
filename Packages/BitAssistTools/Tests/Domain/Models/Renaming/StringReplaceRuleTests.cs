
using NUnit.Framework;
using BitAssistTools.Domain.Models.Renaming;

namespace BitAssistTools.Tests.Domain.Models.Renaming
{
    [TestFixture]
    public class StringReplaceRuleTests
    {
        [Test]
        public void Constructor_WithValidParameters_CreatesCorrectRule()
        {
            // Arrange
            var search = "Test";
            var replace = "Replaced";

            // Act
            var rule = new StringReplaceRule(search, replace);

            // Assert
            Assert.AreEqual(search, rule.SearchString);
            Assert.AreEqual(replace, rule.ReplaceString);
        }

        [Test]
        public void ApplyTo_WithMatchingString_ReturnsReplacedString()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "Replaced");
            var originalName = "This is a Test string";

            // Act
            var newName = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("This is a Replaced string", newName);
        }

        [Test]
        public void ApplyTo_WithNonMatchingString_ReturnsOriginalString()
        {
            // Arrange
            var rule = new StringReplaceRule("NotInString", "Replaced");
            var originalName = "This is a Test string";

            // Act
            var newName = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual(originalName, newName);
        }

        [Test]
        public void IsValid_WithNonEmptySearchString_ReturnsTrue()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "");

            // Act & Assert
            Assert.IsTrue(rule.IsValid());
        }

        [Test]
        public void IsValid_WithEmptySearchString_ReturnsFalse()
        {
            // Arrange
            var rule = new StringReplaceRule("", "Replaced");

            // Act & Assert
            Assert.IsFalse(rule.IsValid());
        }
    }
}
