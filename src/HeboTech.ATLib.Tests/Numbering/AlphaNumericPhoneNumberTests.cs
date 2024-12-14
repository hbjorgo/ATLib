using HeboTech.ATLib.Numbering;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class AlphaNumericPhoneNumberTests
    {
        [Theory]
        [InlineData("1-800-PIZZA", NumberingPlanIdentification.ISDN, "1-800-PIZZA", "1-800-PIZZA")]
        [InlineData("Design@Home", NumberingPlanIdentification.Telex, "Design@Home", "Design@Home")]
        [InlineData("12345678901", NumberingPlanIdentification.Telex, "12345678901", "12345678901")]
        public void Creates_correct_number(string number, NumberingPlanIdentification npi, string expectedNumber, string expectedStringRepresentation)
        {
            AlphaNumericPhoneNumber sut = new(number, npi);

            Assert.Equal(expectedNumber, sut.Number);
            Assert.Equal(TypeOfNumber.AlphaNumeric, sut.TypeOfNumber);
            Assert.Equal(npi, sut.NumberingPlanIdentification);
            Assert.Equal(expectedStringRepresentation, sut);
        }

        [Theory]
        [InlineData(null, NumberingPlanIdentification.ISDN)]
        [InlineData("", NumberingPlanIdentification.ISDN)]
        public void Throws_if_invalid_number(string number, NumberingPlanIdentification npi)
        {
            Assert.Throws<ArgumentException>(() => new AlphaNumericPhoneNumber(number, npi));
        }
    }
}
