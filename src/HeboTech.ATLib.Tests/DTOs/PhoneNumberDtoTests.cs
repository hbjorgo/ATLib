using HeboTech.ATLib.DTOs;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class PhoneNumberDtoTests
    {
        [Fact]
        internal void Sets_properties()
        {
            PhoneNumberDto sut = new("12345678");

            Assert.Equal("12345678", sut.Number);
        }
    }
}
