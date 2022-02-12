using HeboTech.ATLib.PDU;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class Gsm7Tests
    {
        [Theory]
        [InlineData("Google", "C7F7FBCC2E03")]
        [InlineData("SMS Rulz", "D3E61424ADB3F5")]
        public void Encoder_returns_encoded_text(string gsm7Bit, string expected)
        {
            string result = Gsm7.Encode(gsm7Bit);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("C7F7FBCC2E03", "Google")]
        [InlineData("D3E61424ADB3F5", "SMS Rulz")]
        public void Decoder_returns_decoded_text(string gsm7Bit, string expected)
        {
            string result = Gsm7.Decode(gsm7Bit);

            Assert.Equal(expected, result);
        }
    }
}
