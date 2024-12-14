using HeboTech.ATLib.Numbering;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class UnknownPhoneNumberTests
    {
        [Theory]
        [InlineData("1", NumberingPlanIdentification.ISDN, "1", "1")]
        [InlineData("1234", NumberingPlanIdentification.ISDN, "1234", "1234")]
        [InlineData("12345678", NumberingPlanIdentification.ISDN, "12345678", "12345678")]
        [InlineData("42434813", NumberingPlanIdentification.Telex, "42434813", "42434813")]
        [InlineData("(555) 123-4567", NumberingPlanIdentification.ISDN, "5551234567", "5551234567")]
        [InlineData("555.123.4567", NumberingPlanIdentification.ISDN, "5551234567", "5551234567")]
        [InlineData("555/123/4567", NumberingPlanIdentification.ISDN, "5551234567", "5551234567")]
        [InlineData("123456789012345", NumberingPlanIdentification.ISDN, "123456789012345", "123456789012345")]
        public void Creates_correct_number(string number, NumberingPlanIdentification npi, string expectedNumber, string expectedStringRepresentation)
        {
            UnknownPhoneNumber sut = new(number, npi);

            Assert.Equal(expectedNumber, sut.Number);
            Assert.Equal(TypeOfNumber.Unknown, sut.TypeOfNumber);
            Assert.Equal(npi, sut.NumberingPlanIdentification);
            Assert.Equal(expectedStringRepresentation, sut);
        }

        [Theory]
        [InlineData(null, NumberingPlanIdentification.ISDN)]
        [InlineData("", NumberingPlanIdentification.ISDN)]
        public void Throws_if_invalid_number(string number, NumberingPlanIdentification npi)
        {
            Assert.Throws<ArgumentException>(() => new UnknownPhoneNumber(number, npi));
        }
    }
}
