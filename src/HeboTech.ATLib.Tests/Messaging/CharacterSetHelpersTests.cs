using HeboTech.ATLib.Messaging;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class CharacterSetHelpersTests
    {
        [Theory]
        [InlineData("GSM", CharacterSet.Gsm7)]
        [InlineData("UCS2", CharacterSet.UCS2)]
        public void FromString_returns_correct_string(string value, CharacterSet expectedCharacterSet)
        {
            Assert.Equal(expectedCharacterSet, CharacterSetHelpers.FromString(value));
        }

        [Theory]
        [InlineData("InvalidCharacterSet")]
        public void FromString_throws_on_unknown_characterset(string value)
        {
            Assert.Throws<ArgumentException>(() => CharacterSetHelpers.FromString(value));
        }

        [Theory]
        [InlineData(CharacterSet.Gsm7, "GSM")]
        [InlineData(CharacterSet.UCS2, "UCS2")]
        public void ToString_returns_correct_string(CharacterSet characterSet, string expectedValue)
        {
            Assert.Equal(expectedValue, CharacterSetHelpers.ToString(characterSet));
        }

        [Theory]
        [InlineData((CharacterSet)255)]
        public void ToString_throws_on_unknown_characterset(CharacterSet characterSet)
        {
            Assert.Throws<ArgumentException>(() => CharacterSetHelpers.ToString(characterSet));
        }
    }
}
