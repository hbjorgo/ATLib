using HeboTech.ATLib.Messaging;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class DataCodingSchemeTests
    {
        [Theory]
        [InlineData(0x00, CharacterSet.Gsm7, MessageClass.Default, CodingGroup.GeneralDataCoding)]
        [InlineData(0x08, CharacterSet.UCS2, MessageClass.Default, CodingGroup.GeneralDataCoding)]
        [InlineData(0x11, CharacterSet.Gsm7, MessageClass.Class1, CodingGroup.GeneralDataCoding)]
        internal void ParseByte_returns_correct_DataCodingScheme(byte value, CharacterSet expectedCharacterSet, MessageClass expectedMessageClass, CodingGroup expectedCodingGroup)
        {
            var dcs = DataCodingScheme.ParseByte(value);

            Assert.Equal(expectedCharacterSet, dcs.CharacterSet);
            Assert.Equal(expectedMessageClass, dcs.MessageClass);
            Assert.Equal(expectedCodingGroup, dcs.CodingGroup);
        }

        [Theory]
        [InlineData(0x01)]
        [InlineData(0xFF)]
        internal void ParseByte_throws_on_unknown_value(byte value)
        {
            Assert.Throws<ArgumentException>(() => DataCodingScheme.ParseByte(value));
        }
    }
}
