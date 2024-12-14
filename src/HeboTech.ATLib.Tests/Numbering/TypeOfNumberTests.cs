using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class TypeOfNumberTests
    {
        [Theory]
        [InlineData(TypeOfNumber.Unknown, 0x00)]
        [InlineData(TypeOfNumber.International, 0x01)]
        [InlineData(TypeOfNumber.National, 0x02)]
        [InlineData(TypeOfNumber.NetworkSpecific, 0x03)]
        [InlineData(TypeOfNumber.Subscriber, 0x04)]
        [InlineData(TypeOfNumber.AlphaNumeric, 0x05)]
        [InlineData(TypeOfNumber.Abbreviated, 0x06)]
        [InlineData(TypeOfNumber.ReservedForExtension, 0x07)]
        internal void Has_correct_value(TypeOfNumber ton, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)ton);
        }
    }
}
