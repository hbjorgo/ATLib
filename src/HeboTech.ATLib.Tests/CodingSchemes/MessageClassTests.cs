using HeboTech.ATLib.CodingSchemes;
using Xunit;

namespace HeboTech.ATLib.Tests.CodingSchemes
{
    public class MessageClassTests
    {
        [Theory]
        [InlineData(MessageClass.Default, 0x00)]
        [InlineData(MessageClass.Class0, 0x01)]
        [InlineData(MessageClass.Class1, 0x02)]
        [InlineData(MessageClass.Class2, 0x03)]
        [InlineData(MessageClass.Class3, 0x04)]
        internal void Has_correct_values(MessageClass messageClass, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)messageClass);
        }
    }
}
