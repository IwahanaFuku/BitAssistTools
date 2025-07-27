
using NUnit.Framework;
using BitAssistTools.Domain.Models.Renaming;

namespace BitAssistTools.Tests.Domain.Models.Renaming
{
    [TestFixture]
    public class PrefixSuffixRuleTests
    {
        [Test]
        public void ApplyTo_WithPrefixAndSuffix_ReturnsCorrectlyFormattedString()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Prefix", "Suffix", "_");
            var originalName = "Test";

            // Act
            var newName = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("Prefix_Test_Suffix", newName);
        }

        [Test]
        public void ApplyTo_WithOnlyPrefix_ReturnsCorrectlyFormattedString()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Prefix", "", "_");
            var originalName = "Test";

            // Act
            var newName = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("Prefix_Test", newName);
        }

        [Test]
        public void ApplyTo_WithOnlySuffix_ReturnsCorrectlyFormattedString()
        {
            // Arrange
            var rule = new PrefixSuffixRule("", "Suffix", "_");
            var originalName = "Test";

            // Act
            var newName = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("Test_Suffix", newName);
        }

        [Test]
        public void IsValid_WithPrefix_ReturnsTrue()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Prefix", "", "_");

            // Act & Assert
            Assert.IsTrue(rule.IsValid());
        }

        [Test]
        public void IsValid_WithSuffix_ReturnsTrue()
        {
            // Arrange
            var rule = new PrefixSuffixRule("", "Suffix", "_");

            // Act & Assert
            Assert.IsTrue(rule.IsValid());
        }

        [Test]
        public void IsValid_WithNoPrefixAndNoSuffix_ReturnsFalse()
        {
            // Arrange
            var rule = new PrefixSuffixRule("", "", "_");

            // Act & Assert
            Assert.IsFalse(rule.IsValid());
        }

        [Test]
        public void Constructor_WithNullValues_CreatesRuleWithEmptyStrings()
        {
            // Arrange & Act
            var rule = new PrefixSuffixRule(null, null, null);

            // Assert
            Assert.AreEqual(string.Empty, rule.Prefix);
            Assert.AreEqual(string.Empty, rule.Suffix);
            Assert.AreEqual(string.Empty, rule.Delimiter);
        }

        [Test]
        public void ApplyTo_WithNullOriginalName_ReturnsNull()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Prefix", "Suffix", "_");

            // Act
            var result = rule.ApplyTo(null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ApplyTo_WithEmptyOriginalName_ReturnsEmptyString()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Prefix", "Suffix", "_");

            // Act
            var result = rule.ApplyTo("");

            // Assert
            Assert.AreEqual("", result); // 実装では空文字列をそのまま返す
        }

        [Test]
        public void ApplyTo_WithEmptySeparator_ConcatenatesDirectly()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Pre", "Suf", "");
            var originalName = "Test";

            // Act
            var result = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("PreTestSuf", result);
        }

        [Test]
        public void ApplyTo_WithNullSeparator_ConcatenatesDirectly()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Pre", "Suf", null);
            var originalName = "Test";

            // Act
            var result = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("PreTestSuf", result);
        }

        [Test]
        public void ApplyTo_WithSpecialCharacterSeparator_HandlesCorrectly()
        {
            // Arrange
            var rule = new PrefixSuffixRule("A", "Z", "##");
            var originalName = "Mid";

            // Act
            var result = rule.ApplyTo(originalName);

            // Assert
            Assert.AreEqual("A##Mid##Z", result);
        }

        [Test]
        public void ApplyTo_WithLongStrings_HandlesCorrectly()
        {
            // Arrange
            var longPrefix = new string('P', 100);
            var longSuffix = new string('S', 100);
            var rule = new PrefixSuffixRule(longPrefix, longSuffix, "_");
            var originalName = "Test";

            // Act
            var result = rule.ApplyTo(originalName);

            // Assert
            Assert.IsTrue(result.StartsWith(longPrefix));
            Assert.IsTrue(result.EndsWith(longSuffix));
            Assert.IsTrue(result.Contains("_Test_"));
        }

        [Test]
        public void Default_ReturnsRuleWithDefaultValues()
        {
            // Act
            var defaultRule = PrefixSuffixRule.Default;

            // Assert
            Assert.AreEqual("Prefix", defaultRule.Prefix);
            Assert.AreEqual("Suffix", defaultRule.Suffix);
            Assert.AreEqual("_", defaultRule.Delimiter);
            Assert.IsTrue(defaultRule.IsValid());
        }

        [Test]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var rule1 = new PrefixSuffixRule("Pre", "Suf", "_");
            var rule2 = new PrefixSuffixRule("Pre", "Suf", "_");

            // Act & Assert
            Assert.IsTrue(rule1.Equals(rule2));
            Assert.IsTrue(rule1 == rule2);
            Assert.IsFalse(rule1 != rule2);
        }

        [Test]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var rule1 = new PrefixSuffixRule("Pre", "Suf", "_");
            var rule2 = new PrefixSuffixRule("Different", "Suf", "_");

            // Act & Assert
            Assert.IsFalse(rule1.Equals(rule2));
            Assert.IsFalse(rule1 == rule2);
            Assert.IsTrue(rule1 != rule2);
        }

        [Test]
        public void Equals_WithObjectType_HandlesCorrectly()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Pre", "Suf", "_");
            object otherRule = new PrefixSuffixRule("Pre", "Suf", "_");
            object differentObject = "not a prefix suffix rule";

            // Act & Assert
            Assert.IsTrue(rule.Equals(otherRule));
            Assert.IsFalse(rule.Equals(differentObject));
            Assert.IsFalse(rule.Equals(null));
        }

        [Test]
        public void GetHashCode_WithSameValues_ReturnsSameHashCode()
        {
            // Arrange
            var rule1 = new PrefixSuffixRule("Pre", "Suf", "_");
            var rule2 = new PrefixSuffixRule("Pre", "Suf", "_");

            // Act & Assert
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_WithDifferentValues_ReturnsDifferentHashCodes()
        {
            // Arrange
            var rule1 = new PrefixSuffixRule("Pre", "Suf", "_");
            var rule2 = new PrefixSuffixRule("Different", "Suf", "_");
            var rule3 = new PrefixSuffixRule("Pre", "Different", "_");
            var rule4 = new PrefixSuffixRule("Pre", "Suf", "-");

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
            Assert.AreNotEqual(rule1.GetHashCode(), rule3.GetHashCode());
            Assert.AreNotEqual(rule1.GetHashCode(), rule4.GetHashCode());
        }

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var rule = new PrefixSuffixRule("Pre", "Suf", "_");

            // Act
            var result = rule.ToString();

            // Assert
            Assert.IsTrue(result.Contains("PrefixSuffixRule"));
            Assert.IsTrue(result.Contains("Pre"));
            Assert.IsTrue(result.Contains("Suf"));
            Assert.IsTrue(result.Contains("_"));
        }

        [Test]
        public void IsValid_WithNullValues_ReturnsFalse()
        {
            // Arrange
            var rule = new PrefixSuffixRule(null, null, "_");

            // Act & Assert
            Assert.IsFalse(rule.IsValid());
        }
    }
}
