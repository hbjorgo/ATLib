using HeboTech.ATLib.Messaging;
using HeboTech.ATLib.Numbering;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class PhoneNumberDecoderTests
    {
        [Theory]
        // National
        [InlineData("A189674523F1", "987654321")]

        // International
        [InlineData("912143658709", "+1234567890")]
        [InlineData("91447721436587", "+447712345678")]
        [InlineData("914477214365F7", "+44771234567")]
        [InlineData("915155214365F7", "+15551234567")]

        // Alphanumeric
        [InlineData("D0C4F23C7D760390EF7619", "Design@Home")]
        [InlineData("50C82293F98CC966", "HELLO123")]
        [InlineData("D0D4323B1D06", "Telia")]
        public void Decode_PhoneNumber_tests(string data, string number)
        {
            var bytes = Convert.FromHexString(data);
            PhoneNumber phoneNumber = PhoneNumberDecoder.DecodePhoneNumber(bytes);

            Assert.NotNull(phoneNumber);
            Assert.Equal(number, phoneNumber.ToString());
        }
    }
}
