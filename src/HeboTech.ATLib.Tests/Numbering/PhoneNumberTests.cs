using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class PhoneNumberTests
    {
        [Fact]
        public void CreateNationalOrInternationalNumber_returns_empty_country_code()
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber(null, "23456789");

            Assert.Equal("", dut.CountryCode);
            Assert.Equal("23456789", dut.NationalNumber);
            Assert.Equal(TypeOfNumber.National, dut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.ISDN, dut.NumberingPlanIdentification);
        }

        [Theory]
        [InlineData("", "23456789", TypeOfNumber.National)]
        [InlineData("1", "23456789", TypeOfNumber.International)]
        public void CreateNationalOrInternationalNumber_creates_correct_number(string countryCode, string nationalNumber, TypeOfNumber ton)
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber(countryCode, nationalNumber);

            Assert.Equal(countryCode, dut.CountryCode);
            Assert.Equal(nationalNumber, dut.NationalNumber);
            Assert.Equal(ton, dut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.ISDN, dut.NumberingPlanIdentification);
        }

        [Theory]
        [InlineData("12345678")]
        public void CreateNationalNumber_creates_correct_number(string nationalNumber)
        {
            PhoneNumber dut = PhoneNumber.CreateNationalNumber(nationalNumber);

            Assert.Equal(nationalNumber, dut.NationalNumber);
            Assert.Equal(TypeOfNumber.National, dut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.ISDN, dut.NumberingPlanIdentification);
        }

        [Theory]
        [InlineData("48", "12345678")]
        public void CreateInternationalNumber_creates_correct_number(string countryCode, string nationalNumber)
        {
            PhoneNumber dut = PhoneNumber.CreateInternationalNumber(countryCode, nationalNumber);

            Assert.Equal(countryCode, dut.CountryCode);
            Assert.Equal(nationalNumber, dut.NationalNumber);
            Assert.Equal(TypeOfNumber.International, dut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.ISDN, dut.NumberingPlanIdentification);
        }

        [Theory]
        [InlineData("1-800-PIZZA")]
        [InlineData("Design@Home")]
        public void CreateAlphaNumericNumber_creates_correct_number(string nationalNumber)
        {
            PhoneNumber dut = PhoneNumber.CreateAlphaNumericNumber(nationalNumber);

            Assert.Equal(nationalNumber, dut.NationalNumber);
            Assert.Equal(TypeOfNumber.AlphaNumeric, dut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.Unknown, dut.NumberingPlanIdentification);
        }

        [Theory]
        [InlineData("", "23456789", "23456789")]
        [InlineData("1", "23456789", "+123456789")]
        public void ToString_returns_number(string countryCode, string nationalNumber, string expected)
        {
            PhoneNumber dut = PhoneNumber.CreateNationalOrInternationalNumber(countryCode, nationalNumber);

            Assert.Equal(expected, dut.ToString());
        }
    }
}
