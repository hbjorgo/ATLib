using HeboTech.ATLib.CodingSchemes;
using Xunit;

namespace HeboTech.ATLib.Tests.CodingSchemes
{
    public class CharacterSetTests
    {
        [Theory]
        [InlineData(CharacterSet.Gsm7, 0x00)]
        [InlineData(CharacterSet.UCS2, 0x08)]
        public void Has_correct_values(CharacterSet characterSet, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)characterSet);
        }
    }
}
