using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class PhoneNumberTests
    {
        /*
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
        public void Create_with_number_only_creates_correct_number(string number, string countryCode, string nationalNumber, TypeOfNumber ton)
        {
            PhoneNumber sut = PhoneNumberFactory.Create(number);

            Assert.Equal(countryCode, sut.CountryCode);
            Assert.Equal(nationalNumber, sut.NationalNumber);
            Assert.Equal(ton, sut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.ISDN, sut.NumberingPlanIdentification);
        }

        [Theory]
        [InlineData("1-800-PIZZA", TypeOfNumber.AlphaNumeric, NumberingPlanIdentification.Unknown)]
        [InlineData("Design@Home", TypeOfNumber.AlphaNumeric, NumberingPlanIdentification.Unknown)]
        [InlineData("1989", TypeOfNumber.NetworkSpecific, NumberingPlanIdentification.Unknown)]
        public void Create_with_number_and_types_creates_alphanumeric_number(string number, TypeOfNumber ton, NumberingPlanIdentification npi)
        {
            PhoneNumber sut = PhoneNumberFactory.Create(number, ton, npi);

            Assert.Equal(string.Empty, sut.CountryCode);
            Assert.Equal(number, sut.NationalNumber);
            Assert.Equal(ton, sut.TypeOfNumber);
            Assert.Equal(npi, sut.NumberingPlanIdentification);
        }

        [Theory]
        [InlineData("23456789", "23456789")]
        [InlineData("+123456789", "+123456789")]
        [InlineData("+1 (500) 23456789", "+150023456789")]
        public void ToString_returns_number_when_created_with_number_only(string number, string expected)
        {
            PhoneNumber sut = PhoneNumberFactory.Create(number);

            Assert.Equal(expected, sut.ToString());
        }

        [Theory]
        [InlineData("1-800-PIZZA", TypeOfNumber.AlphaNumeric, NumberingPlanIdentification.Unknown, "1-800-PIZZA")]
        [InlineData("1989", TypeOfNumber.NetworkSpecific, NumberingPlanIdentification.Unknown, "1989")]
        public void ToString_returns_returns_number_when_created_with_number_and_types(string number, TypeOfNumber ton, NumberingPlanIdentification npi, string expected)
        {
            PhoneNumber sut = PhoneNumberFactory.Create(number, ton, npi);

            Assert.Equal(expected, sut.ToString());
        }
        */
    }
}
