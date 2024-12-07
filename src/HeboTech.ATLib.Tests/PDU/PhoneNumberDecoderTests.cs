using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.PDU;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class PhoneNumberDecoderTests
    {
        [Theory]
        // National
        [InlineData("8189674523F1", "987654321")]

        // International
        [InlineData("912143658709", "+1234567890")]
        [InlineData("91447721436587", "+447712345678")]
        [InlineData("914477214365F7", "+44771234567")]

        // Alphanumeric
        [InlineData("5048454C4C4F313233", "HELLO123")]
        public void Decode_PhoneNumber_tests(string data, string number)
        {
            var bytes = Convert.FromHexString(data);
            PhoneNumberDTO phoneNumber = PhoneNumberDecoder.DecodePhoneNumber(bytes);

            Assert.NotNull(phoneNumber);
            Assert.Equal(number, phoneNumber.Number);
        }
    }
}
