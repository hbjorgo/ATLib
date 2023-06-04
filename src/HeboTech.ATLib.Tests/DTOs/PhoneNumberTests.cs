using HeboTech.ATLib.DTOs;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class PhoneNumberTests
    {
        [Fact]
        public void Number_property_is_set()
        {
            PhoneNumber dut = new("+123456789");

            Assert.Equal("+123456789", dut.Number);
        }

        [Fact]
        public void ToString_returns_number()
        {
            PhoneNumber dut = new("+123456789");

            Assert.Equal("+123456789", dut.ToString());
        }
    }
}
