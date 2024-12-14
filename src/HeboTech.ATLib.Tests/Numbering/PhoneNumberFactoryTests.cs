using HeboTech.ATLib.Numbering;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class PhoneNumberFactoryTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("123457A")]
        [InlineData("ABCD")]
        [InlineData("+123457A")]
        [InlineData("+ABCD")]
        public void CreateCommonIsdn_throws_on_not_supported_numbers(string number)
        {
            Assert.Throws<ArgumentException>(() => PhoneNumberFactory.CreateCommonIsdn(number));
        }

        [Theory]
        [InlineData("12345678", TypeOfNumber.National, NumberingPlanIdentification.ISDN, "12345678", "12345678", typeof(NationalPhoneNumber))]
        [InlineData("+12345678", TypeOfNumber.International, NumberingPlanIdentification.ISDN, "12345678", "+12345678", typeof(InternationalPhoneNumber))]
        public void CreateCommonIsdn_creates_correct_number(string number, TypeOfNumber ton, NumberingPlanIdentification npi, string expectedNumber, string expectedStringRepresentation, Type expectedType)
        {
            PhoneNumber sut = PhoneNumberFactory.CreateCommonIsdn(number);

            Assert.Equal(expectedType, sut.GetType());
            Assert.Equal(ton, sut.TypeOfNumber);
            Assert.Equal(npi, sut.NumberingPlanIdentification);
            Assert.Equal(expectedNumber, sut.Number);
            Assert.Equal(expectedStringRepresentation, sut);
        }

        [Theory]
        [InlineData("12345678", TypeOfNumber.National, NumberingPlanIdentification.ISDN, "12345678", "12345678", typeof(NationalPhoneNumber))]
        [InlineData("12345678", TypeOfNumber.National, NumberingPlanIdentification.Telex, "12345678", "12345678", typeof(NationalPhoneNumber))]
        [InlineData("+12345678", TypeOfNumber.International, NumberingPlanIdentification.ISDN, "12345678", "+12345678", typeof(InternationalPhoneNumber))]
        [InlineData("+12345678", TypeOfNumber.International, NumberingPlanIdentification.Telex, "12345678", "+12345678", typeof(InternationalPhoneNumber))]
        public void Create_creates_correct_number(string number, TypeOfNumber ton, NumberingPlanIdentification npi, string expectedNumber, string expectedStringRepresentation, Type expectedType)
        {
            PhoneNumber sut = PhoneNumberFactory.Create(number, ton, npi);

            Assert.Equal(expectedType, sut.GetType());
            Assert.Equal(ton, sut.TypeOfNumber);
            Assert.Equal(npi, sut.NumberingPlanIdentification);
            Assert.Equal(expectedNumber, sut.Number);
            Assert.Equal(expectedStringRepresentation, sut);
        }

        [Theory]
        [InlineData("12345678", TypeOfNumber.Unknown, NumberingPlanIdentification.ISDN)]
        [InlineData("12345678", TypeOfNumber.NetworkSpecific, NumberingPlanIdentification.ISDN)]
        [InlineData("12345678", TypeOfNumber.Subscriber, NumberingPlanIdentification.ISDN)]
        [InlineData("12345678", TypeOfNumber.AlphaNumeric, NumberingPlanIdentification.ISDN)]
        [InlineData("12345678", TypeOfNumber.Abbreviated, NumberingPlanIdentification.ISDN)]
        [InlineData("12345678", TypeOfNumber.ReservedForExtension, NumberingPlanIdentification.ISDN)]
        [InlineData("12345678", (TypeOfNumber)255, NumberingPlanIdentification.ISDN)]
        public void Create_throws_on_not_supported_types(string number, TypeOfNumber ton, NumberingPlanIdentification npi)
        {
            Assert.Throws<NotSupportedException>(() => PhoneNumberFactory.Create(number, ton, npi));
        }
    }
}
