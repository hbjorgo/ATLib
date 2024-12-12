using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
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
