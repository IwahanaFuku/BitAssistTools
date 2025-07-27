
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
    }
}
