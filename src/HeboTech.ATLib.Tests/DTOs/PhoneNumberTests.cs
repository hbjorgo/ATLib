using HeboTech.ATLib.DTOs;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class PhoneNumberTests
    {
        [Fact]
        public void Properties_are_set()
        {
            PhoneNumber dut = new("1", "23456789");

            Assert.Equal("1", dut.CountryCode);
            Assert.Equal("23456789", dut.NationalNumber);
        }

        [Theory]
        [InlineData("", "23456789", "23456789")]
        [InlineData("1", "23456789", "+123456789")]
        public void ToString_returns_number(string countryCode, string nationalNumber, string expected)
        {
            PhoneNumber dut = new(countryCode, nationalNumber);

            Assert.Equal(expected, dut.ToString());
        }

        [Fact]
        public void GetTypeOfNumber_returns_international()
        {
            PhoneNumber dut = new("1", "23456789");

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
            PhoneNumber dut = new("1", "23456789");

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
