using HeboTech.ATLib.CodingSchemes;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class Gsm7Tests
    {
        [Theory]
        [InlineData("A", "41")]
        [InlineData("AB", "4121")]
        [InlineData("ABC", "41E110")]
        [InlineData("Google", "C7F7FBCC2E03")]
        [InlineData("SMS Rulz", "D3E61424ADB3F5")]
        [InlineData("Hello.", "C8329BFD7601")]
        [InlineData("This is testdata!", "54747A0E4ACF41F4F29C4E0ED3C321")]
        [InlineData("The quick brown fox jumps over the lazy dog", "54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719")]
        [InlineData("Tada :)", "D430390CD2A500")]
        public void Encoder_returns_encoded_text(string gsm7Bit, string expected)
        {
            string result = Gsm7.Encode(gsm7Bit);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("41", "A")]
        [InlineData("4121", "AB")]
        [InlineData("41E110", "ABC")]
        [InlineData("C7F7FBCC2E03", "Google")]
        [InlineData("D3E61424ADB3F5", "SMS Rulz")]
        [InlineData("C8329BFD7601", "Hello.")]
        [InlineData("54747A0E4ACF41F4F29C4E0ED3C321", "This is testdata!")]
        [InlineData("54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719", "The quick brown fox jumps over the lazy dog")]
        public void Decoder_returns_decoded_text(string gsm7Bit, string expected)
        {
            string result = Gsm7.Decode(gsm7Bit);

            Assert.Equal(expected, result);
        }
    }
}
