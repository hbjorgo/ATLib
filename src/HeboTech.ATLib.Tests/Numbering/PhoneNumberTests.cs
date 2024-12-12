using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class PhoneNumberTests
    {
        [Fact]
        public void Properties_are_set()
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber("1", "23456789");

            Assert.Equal("1", dut.CountryCode);
            Assert.Equal("23456789", dut.NationalNumber);
        }

        [Theory]
        [InlineData("", "23456789", "23456789")]
        [InlineData("1", "23456789", "+123456789")]
        public void ToString_returns_number(string countryCode, string nationalNumber, string expected)
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber(countryCode, nationalNumber);

            Assert.Equal(expected, dut.ToString());
        }

        [Fact]
        public void GetTypeOfNumber_returns_international()
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber("1", "23456789");

            Assert.Equal(TypeOfNumber.International, dut.TypeOfNumber);
        }

        [Fact]
        public void GetTypeOfNumber_returns_national()
        {
            PhoneNumber dut = PhoneNumber.CreateNationalNumber("123456789");

            Assert.Equal(TypeOfNumber.National, dut.TypeOfNumber);
        }

        [Fact]
        public void GetNumberPlanIdentification_returns_ISDN()
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber("1", "23456789");

            Assert.Equal(NumberPlanIdentification.ISDN, dut.NumberPlanIdentification);
        }

        [Fact]
        public void GetNumberPlanIdentification_returns_Unknown()
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber(null, "123456789");

            Assert.Equal(NumberPlanIdentification.Unknown, dut.NumberPlanIdentification);
        }
    }
}
