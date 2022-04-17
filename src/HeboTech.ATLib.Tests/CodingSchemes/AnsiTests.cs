using HeboTech.ATLib.CodingSchemes;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class AnsiTests
    {
        [Theory]
        [InlineData("A", "41")]
        [InlineData("AB", "4142")]
        [InlineData("ABC", "414243")]
        [InlineData("Google", "476F6F676C65")]
        [InlineData("SMS Rulz", "534D532052756C7A")]
        [InlineData("Hello.", "48656C6C6F2E")]
        [InlineData("This is testdata!", "5468697320697320746573746461746121")]
        [InlineData("The quick brown fox jumps over the lazy dog", "54686520717569636B2062726F776E20666F78206A756D7073206F76657220746865206C617A7920646F67")]
        public void Encoder_returns_encoded_text(string encoded, string expected)
        {
            string result = Ansi.Encode(encoded);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("41", "A")]
        [InlineData("4142", "AB")]
        [InlineData("414243", "ABC")]
        [InlineData("476F6F676C65", "Google")]
        [InlineData("534D532052756C7A", "SMS Rulz")]
        [InlineData("48656C6C6F2E", "Hello.")]
        [InlineData("5468697320697320746573746461746121", "This is testdata!")]
        [InlineData("54686520717569636B2062726F776E20666F78206A756D7073206F76657220746865206C617A7920646F67", "The quick brown fox jumps over the lazy dog")]
        public void Decoder_returns_decoded_text(string encoded, string expected)
        {
            string result = Ansi.Decode(encoded);

            Assert.Equal(expected, result);
        }
    }
}
