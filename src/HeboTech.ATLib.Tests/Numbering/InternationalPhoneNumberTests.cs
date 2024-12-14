using HeboTech.ATLib.Numbering;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class InternationalPhoneNumberTests
    {
        [Theory]
        [InlineData("+1", NumberingPlanIdentification.ISDN, "1", "+1")]
        [InlineData("+4412345678", NumberingPlanIdentification.ISDN, "4412345678", "+4412345678")]
        [InlineData("+4412345678", NumberingPlanIdentification.Telex, "4412345678", "+4412345678")]
        [InlineData("+4842434813", NumberingPlanIdentification.ISDN, "4842434813", "+4842434813")]
        [InlineData("+1 (555) 123-4567", NumberingPlanIdentification.ISDN, "15551234567", "+15551234567")]
        [InlineData("+1 555.123.4567", NumberingPlanIdentification.ISDN, "15551234567", "+15551234567")]
        [InlineData("+1 555/123/4567", NumberingPlanIdentification.ISDN, "15551234567", "+15551234567")]
        [InlineData("+263 123456789012", NumberingPlanIdentification.ISDN, "263123456789012", "+263123456789012")]
        public void Creates_correct_number(string number, NumberingPlanIdentification npi, string expectedNumber, string expectedStringRepresentation)
        {
            InternationalPhoneNumber sut = new(number, npi);

            Assert.Equal(expectedNumber, sut.Number);
            Assert.Equal(TypeOfNumber.International, sut.TypeOfNumber);
            Assert.Equal(npi, sut.NumberingPlanIdentification);
            Assert.Equal(expectedStringRepresentation, sut);
        }

        [Theory]
        [InlineData(null, NumberingPlanIdentification.ISDN)]
        [InlineData("", NumberingPlanIdentification.ISDN)]
        [InlineData("+1234567890123456", NumberingPlanIdentification.ISDN)]
        public void Throws_if_invalid_number(string number, NumberingPlanIdentification npi)
        {
            Assert.Throws<ArgumentException>(() => new InternationalPhoneNumber(number, npi));
        }
    }
}
