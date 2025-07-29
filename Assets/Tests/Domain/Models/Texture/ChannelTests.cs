using System;
using NUnit.Framework;
using BitAssistTools.Domain.Models.Texture;

namespace BitAssistTools.Tests.Domain.Models.Texture
{
    /// <summary>
    /// Channelのテストクラス
    /// </summary>
    [TestFixture]
    public class ChannelTests
    {
        #region Enum Value Tests

        [Test]
        public void Channel_R_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(0, (int)Channel.R);
        }

        [Test]
        public void Channel_G_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(1, (int)Channel.G);
        }

        [Test]
        public void Channel_B_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(2, (int)Channel.B);
        }

        [Test]
        public void Channel_A_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(3, (int)Channel.A);
        }

        #endregion

        #region Enum Validation Tests

        [Test]
        public void Channel_AllValues_AreDefinedCorrectly()
        {
            // すべてのChannel値が定義されていることを確認
            var values = Enum.GetValues(typeof(Channel));
            
            Assert.Contains(Channel.R, values);
            Assert.Contains(Channel.G, values);
            Assert.Contains(Channel.B, values);
            Assert.Contains(Channel.A, values);
            Assert.AreEqual(4, values.Length, "Channel列挙型は4つの値を持つべきです");
        }

        [Test]
        public void Channel_Values_HaveSequentialOrder()
        {
            // チャンネル値が連続した順序であることを確認
            Assert.AreEqual(0, (int)Channel.R);
            Assert.AreEqual(1, (int)Channel.G);
            Assert.AreEqual(2, (int)Channel.B);
            Assert.AreEqual(3, (int)Channel.A);
        }

        #endregion

        #region Casting and Conversion Tests

        [Test]
        public void Channel_CanBeCastToInt()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                int rValue = (int)Channel.R;
                int gValue = (int)Channel.G;
                int bValue = (int)Channel.B;
                int aValue = (int)Channel.A;
                
                Assert.AreEqual(0, rValue);
                Assert.AreEqual(1, gValue);
                Assert.AreEqual(2, bValue);
                Assert.AreEqual(3, aValue);
            });
        }

        [Test]
        public void Channel_CanBeCastFromInt()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                Channel r = (Channel)0;
                Channel g = (Channel)1;
                Channel b = (Channel)2;
                Channel a = (Channel)3;
                
                Assert.AreEqual(Channel.R, r);
                Assert.AreEqual(Channel.G, g);
                Assert.AreEqual(Channel.B, b);
                Assert.AreEqual(Channel.A, a);
            });
        }

        [TestCase(0, Channel.R)]
        [TestCase(1, Channel.G)]
        [TestCase(2, Channel.B)]
        [TestCase(3, Channel.A)]
        public void Channel_IntegerMapping_IsCorrect(int intValue, Channel expectedChannel)
        {
            // Act
            Channel actualChannel = (Channel)intValue;

            // Assert
            Assert.AreEqual(expectedChannel, actualChannel);
        }

        #endregion

        #region Usage in Array Index Tests

        [Test]
        public void Channel_CanBeUsedAsArrayIndex()
        {
            // Arrange
            var testArray = new float[4] { 0.1f, 0.2f, 0.3f, 0.4f };

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                float rValue = testArray[(int)Channel.R];
                float gValue = testArray[(int)Channel.G];
                float bValue = testArray[(int)Channel.B];
                float aValue = testArray[(int)Channel.A];
                
                Assert.AreEqual(0.1f, rValue);
                Assert.AreEqual(0.2f, gValue);
                Assert.AreEqual(0.3f, bValue);
                Assert.AreEqual(0.4f, aValue);
            });
        }

        [Test]
        public void Channel_AllChannels_CanAccessArrayElements()
        {
            // Arrange
            var channels = new[] { Channel.R, Channel.G, Channel.B, Channel.A };
            var values = new float[4] { 1.0f, 0.5f, 0.25f, 0.75f };

            // Act & Assert
            for (int i = 0; i < channels.Length; i++)
            {
                var channel = channels[i];
                var expectedValue = values[i];
                var actualValue = values[(int)channel];
                
                Assert.AreEqual(expectedValue, actualValue, $"Channel {channel} should access correct array element");
            }
        }

        #endregion

        #region ToString and Display Tests

        [Test]
        public void Channel_ToString_ReturnsCorrectName()
        {
            // Act & Assert
            Assert.AreEqual("R", Channel.R.ToString());
            Assert.AreEqual("G", Channel.G.ToString());
            Assert.AreEqual("B", Channel.B.ToString());
            Assert.AreEqual("A", Channel.A.ToString());
        }

        [TestCase(Channel.R, "R")]
        [TestCase(Channel.G, "G")]
        [TestCase(Channel.B, "B")]
        [TestCase(Channel.A, "A")]
        public void Channel_ToString_ReturnsExpectedString(Channel channel, string expected)
        {
            // Act
            string actual = channel.ToString();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Enum Parsing Tests

        [Test]
        public void Channel_CanBeParsedFromString()
        {
            // Act & Assert
            Assert.IsTrue(Enum.TryParse<Channel>("R", out var r));
            Assert.AreEqual(Channel.R, r);
            
            Assert.IsTrue(Enum.TryParse<Channel>("G", out var g));
            Assert.AreEqual(Channel.G, g);
            
            Assert.IsTrue(Enum.TryParse<Channel>("B", out var b));
            Assert.AreEqual(Channel.B, b);
            
            Assert.IsTrue(Enum.TryParse<Channel>("A", out var a));
            Assert.AreEqual(Channel.A, a);
        }

        [Test]
        public void Channel_ParseFromInvalidString_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(Enum.TryParse<Channel>("X", out _));
            Assert.IsFalse(Enum.TryParse<Channel>("RGB", out _));
            Assert.IsFalse(Enum.TryParse<Channel>("", out _));
            Assert.IsFalse(Enum.TryParse<Channel>("Invalid", out _));
            Assert.IsFalse(Enum.TryParse<Channel>("Channel", out _));
        }

        [Test]
        public void Channel_ParseFromInvalidNumericString_SucceedsButIsInvalid()
        {
            // Enum.TryParseは範囲外の数値でもパースに成功するが、
            // Enum.IsDefinedで有効性をチェックできることを確認
            Assert.IsTrue(Enum.TryParse<Channel>("5", out var result));
            Assert.IsFalse(Enum.IsDefined(typeof(Channel), result));
            
            Assert.IsTrue(Enum.TryParse<Channel>("-1", out var result2));
            Assert.IsFalse(Enum.IsDefined(typeof(Channel), result2));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Channel_InSwitchStatement_WorksCorrectly()
        {
            // Arrange & Act & Assert
            foreach (Channel channel in Enum.GetValues(typeof(Channel)))
            {
                Assert.DoesNotThrow(() =>
                {
                    string result;
                    switch (channel)
                    {
                        case Channel.R:
                            result = "Red";
                            break;
                        case Channel.G:
                            result = "Green";
                            break;
                        case Channel.B:
                            result = "Blue";
                            break;
                        case Channel.A:
                            result = "Alpha";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    Assert.IsNotNull(result);
                    Assert.IsNotEmpty(result);
                });
            }
        }

        [Test]
        public void Channel_InDictionary_WorksAsKey()
        {
            // Arrange
            var channelNames = new System.Collections.Generic.Dictionary<Channel, string>
            {
                { Channel.R, "Red" },
                { Channel.G, "Green" },
                { Channel.B, "Blue" },
                { Channel.A, "Alpha" }
            };

            // Act & Assert
            Assert.AreEqual("Red", channelNames[Channel.R]);
            Assert.AreEqual("Green", channelNames[Channel.G]);
            Assert.AreEqual("Blue", channelNames[Channel.B]);
            Assert.AreEqual("Alpha", channelNames[Channel.A]);
        }

        #endregion
    }
}