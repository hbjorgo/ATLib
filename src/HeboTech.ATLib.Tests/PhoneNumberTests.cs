using System.ComponentModel.DataAnnotations;
using Xunit;

namespace HeboTech.ATLib.Tests
{
    public class PhoneNumberTests
    {
        [Fact]
        public void ValidNationalPhoneNumberTest()
        {
            PhoneNumber pn = new PhoneNumber("12345678");
            Assert.Equal("12345678", pn.ToString());
        }

        [Fact]
        public void ValidInternationalPhoneNumberTest()
        {
            PhoneNumber pn = new PhoneNumber("+0112345678");
            Assert.Equal("+0112345678", pn.ToString());
        }

        [Fact]
        public void EmptyPhoneNumberTest()
        {
            Assert.Throws<ValidationException>(() => new PhoneNumber(""));
        }
    }
}
