using HeboTech.ATLib.DTOs;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class PhoneNumberExtensionsTests
    {
        [Fact]
        public void GetTypeOfNumber_returns_international()
        {
            PhoneNumber dut = new("+123456789");

            Assert.Equal(TypeOfNumber.International, dut.GetTypeOfNumber());
        }

        [Fact]
        public void GetTypeOfNumber_returns_national()
        {
            PhoneNumber dut = new("123456789");

            Assert.Equal(TypeOfNumber.National, dut.GetTypeOfNumber());
        }

        [Fact]
        public void GetNumberPlanIdentification_returns_ISDN()
        {
            PhoneNumber dut = new("+123456789");

            Assert.Equal(NumberPlanIdentification.ISDN, dut.GetNumberPlanIdentification());
        }

        [Fact]
        public void GetNumberPlanIdentification_returns_Unknown()
        {
            PhoneNumber dut = new("123456789");

            Assert.Equal(NumberPlanIdentification.Unknown, dut.GetNumberPlanIdentification());
        }
    }
}
