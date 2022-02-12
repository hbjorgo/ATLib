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

        //[Theory]
        //[InlineData(
        //    new byte[] { 0xC7, 0xF7, 0xFB, 0xCC, 0x2E, 0x03 },
        //    new byte[] { 0x47, 0x6F, 0x6F, 0x67, 0x6C, 0x65 })]
        //[InlineData(
        //    new byte[] { 0xD3, 0xE6, 0x14, 0x24, 0xAD, 0xB3, 0xF5 },
        //    new byte[] { 0x53, 0x4D, 0x53, 0x20, 0x52, 0x75, 0x6C, 0x7A })]
        //public void OkTests(byte[] data, byte[] answer)
        //{
        //    byte[] result = Gsm7.Decode(data);

        //    Assert.Equal(answer, result);
        //}
    }
}
