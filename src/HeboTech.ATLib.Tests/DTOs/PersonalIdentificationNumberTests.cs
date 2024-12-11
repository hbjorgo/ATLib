using HeboTech.ATLib.DTOs;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class PersonalIdentificationNumberTests
    {
        [Fact]
        internal void Sets_properties()
        {
            PersonalIdentificationNumber sut = new("1234");

            Assert.Equal("1234", sut.Pin);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        [InlineData("12345")]
        [InlineData("A123")]
        [InlineData("ABCD")]
        internal void Invalid_pin_throws(string pin)
        {
            Assert.Throws<ArgumentException>(() => new PersonalIdentificationNumber(pin));
        }
    }
}
