using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class PhoneNumberTests
    {
        [Theory]
        [InlineData("+112345678", "1", "12345678", TypeOfNumber.International)]
        [InlineData("+46123456", "46", "123456", TypeOfNumber.International)]
        [InlineData("+15551234567", "1", "5551234567", TypeOfNumber.International)]
        [InlineData("+258 82 123 4567", "258", "821234567", TypeOfNumber.International)]
        [InlineData("+254 701 123456", "254", "701123456", TypeOfNumber.International)]
        [InlineData("+49 30 1234567", "49", "301234567", TypeOfNumber.International)]
        [InlineData("+505 888 1234", "505", "8881234", TypeOfNumber.International)]
        [InlineData("+98 21 1234567", "98", "211234567", TypeOfNumber.International)]
        [InlineData("+60 12 3456789", "60", "123456789", TypeOfNumber.International)]
        [InlineData("+60 3 12345678", "60", "312345678", TypeOfNumber.International)]
        [InlineData("+86 138 1234 5678", "86", "13812345678", TypeOfNumber.International)]
        [InlineData("+8613812345678", "86", "13812345678", TypeOfNumber.International)]
        [InlineData("+86 10 1234 5678", "86", "1012345678", TypeOfNumber.International)]
        [InlineData("+7 905 123-45-67", "7", "9051234567", TypeOfNumber.International)]
        [InlineData("+7 495 123-45-67", "7", "4951234567", TypeOfNumber.International)]
        [InlineData("+1 (555) 123-4567", "1", "5551234567", TypeOfNumber.International)]
        [InlineData("+1.555.123.4567", "1", "5551234567", TypeOfNumber.International)]
        [InlineData("+1/555/123/4567", "1", "5551234567", TypeOfNumber.International)]
        [InlineData("+.1    ((555) 1-23----4.567- ())..)))", "1", "5551234567", TypeOfNumber.International)]
        [InlineData("12345678", "", "12345678", TypeOfNumber.National)]
        [InlineData("42434813", "", "42434813", TypeOfNumber.National)]
        [InlineData("(555) 123-4567", "", "5551234567", TypeOfNumber.National)]
        [InlineData("555.123.4567", "", "5551234567", TypeOfNumber.National)]
        [InlineData("555/123/4567", "", "5551234567", TypeOfNumber.National)]
        public void Create_creates_correct_number(string number, string countryCode, string nationalNumber, TypeOfNumber ton)
        {
            PhoneNumber dut = PhoneNumber.Create(number);

            Assert.Equal(countryCode, dut.CountryCode);
            Assert.Equal(nationalNumber, dut.NationalNumber);
            Assert.Equal(ton, dut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.ISDN, dut.NumberingPlanIdentification);
        }

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
