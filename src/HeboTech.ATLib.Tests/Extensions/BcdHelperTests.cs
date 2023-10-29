using HeboTech.ATLib.Extensions;
using Xunit;

namespace HeboTech.ATLib.Tests.Extensions
{
    public class BcdHelperTests
    {
        [Theory]
        [InlineData(0x00, 0x00)]
        [InlineData(0x01, 0x10)]
        [InlineData(0x13, 0x31)]
        [InlineData(0x98, 0x89)]
        public void SwapNibbles_test(byte input, byte expected)
        {
            Assert.Equal(expected, input.SwapNibbles());
        }

        [Theory]
        [InlineData(00, 0x00)]
        [InlineData(01, 0x01)]
        [InlineData(12, 0x12)]
        [InlineData(98, 0x98)]
        public void DecimalToBcd_test(byte input, byte expected)
        {
            Assert.Equal(expected, input.DecimalToBcd());
        }

        [Theory]
        [InlineData(0x00, 00)]
        [InlineData(0x01, 01)]
        [InlineData(0x12, 12)]
        [InlineData(0x98, 98)]
        public void BcdToDecimal_test(byte input, byte expected)
        {
            Assert.Equal(expected, input.BcdToDecimal());
        }
    }
}
