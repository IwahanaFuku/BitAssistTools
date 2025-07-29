
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

        [Test]
        public void Constructor_WithNullSearchString_CreatesRuleWithEmptyString()
        {
            // Arrange & Act
            var rule = new StringReplaceRule(null, "Replaced");

            // Assert
            Assert.AreEqual(string.Empty, rule.SearchString);
            Assert.AreEqual("Replaced", rule.ReplaceString);
        }

        [Test]
        public void Constructor_WithNullReplaceString_CreatesRuleWithEmptyString()
        {
            // Arrange & Act
            var rule = new StringReplaceRule("Test", null);

            // Assert
            Assert.AreEqual("Test", rule.SearchString);
            Assert.AreEqual(string.Empty, rule.ReplaceString);
        }

        [Test]
        public void ApplyTo_WithNullOriginalName_ReturnsNull()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "Replaced");

            // Act
            var result = rule.ApplyTo(null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ApplyTo_WithEmptyOriginalName_ReturnsEmptyString()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "Replaced");

            // Act
            var result = rule.ApplyTo("");

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void ApplyTo_WithEmptySearchString_ReturnsOriginalName()
        {
            // Arrange
            var rule = new StringReplaceRule("", "Replaced");
            var originalName = "Test String";

            // Act
            var result = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual(originalName, result);
        }

        [Test]
        public void ApplyTo_WithMultipleOccurrences_ReplacesAllOccurrences()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "Replaced");
            var originalName = "Test string with Test word Test";

            // Act
            var result = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("Replaced string with Replaced word Replaced", result);
        }

        [Test]
        public void ApplyTo_WithCaseSensitiveMatch_ReplacesExactMatch()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "Replaced");
            var originalName = "test TEST Test";

            // Act
            var result = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("test TEST Replaced", result);
        }

        [Test]
        public void Default_ReturnsRuleWithEmptyStrings()
        {
            // Act
            var defaultRule = StringReplaceRule.Default;

            // Assert
            Assert.AreEqual("", defaultRule.SearchString);
            Assert.AreEqual("", defaultRule.ReplaceString);
            Assert.IsFalse(defaultRule.IsValid());
        }

        [Test]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var rule1 = new StringReplaceRule("Test", "Replaced");
            var rule2 = new StringReplaceRule("Test", "Replaced");

            // Act & Assert
            Assert.IsTrue(rule1.Equals(rule2));
            Assert.IsTrue(rule1 == rule2);
            Assert.IsFalse(rule1 != rule2);
        }

        [Test]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var rule1 = new StringReplaceRule("Test", "Replaced");
            var rule2 = new StringReplaceRule("Different", "Replaced");

            // Act & Assert
            Assert.IsFalse(rule1.Equals(rule2));
            Assert.IsFalse(rule1 == rule2);
            Assert.IsTrue(rule1 != rule2);
        }

        [Test]
        public void GetHashCode_WithSameValues_ReturnsSameHashCode()
        {
            // Arrange
            var rule1 = new StringReplaceRule("Test", "Replaced");
            var rule2 = new StringReplaceRule("Test", "Replaced");

            // Act & Assert
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_WithDifferentValues_ReturnsDifferentHashCodes()
        {
            // Arrange
            var rule1 = new StringReplaceRule("Test", "Replaced");
            var rule2 = new StringReplaceRule("Different", "Replaced");

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var rule = new StringReplaceRule("Test", "Replaced");

            // Act
            var result = rule.ToString();

            // Assert
            Assert.AreEqual("StringReplaceRule('Test' → 'Replaced')", result);
        }

        [Test]
        public void IsValid_WithNullSearchString_ReturnsFalse()
        {
            // Arrange
            var rule = new StringReplaceRule(null, "Replaced");

            // Act & Assert
            Assert.IsFalse(rule.IsValid());
        }
    }
}
