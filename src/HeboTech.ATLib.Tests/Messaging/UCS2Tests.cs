using HeboTech.ATLib.Messaging;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class UCS2Tests
    {
        [Theory]
        [InlineData("A", "0041")]
        [InlineData("ښ", "069A")]
        [InlineData("AB", "00410042")]
        [InlineData("ښڼ", "069A06BC")]
        [InlineData("ABC", "004100420043")]
        [InlineData("Google", "0047006F006F0067006C0065")]
        [InlineData("SMS Rulz", "0053004D0053002000520075006C007A")]
        [InlineData("Hello.", "00480065006C006C006F002E")]
        [InlineData("This is testdata!", "00540068006900730020006900730020007400650073007400640061007400610021")]
        [InlineData("The quick brown fox jumps over the lazy dog", "00540068006500200071007500690063006B002000620072006F0077006E00200066006F00780020006A0075006D007000730020006F00760065007200200074006800650020006C0061007A007900200064006F0067")]
        public void Encoder_returns_encoded_text(string encoded, string expected)
        {
            string result = UCS2.Encode(encoded);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("0041", "A")]
        [InlineData("069A", "ښ")]
        [InlineData("00410042", "AB")]
        [InlineData("069A06BC", "ښڼ")]
        [InlineData("004100420043", "ABC")]
        [InlineData("0047006F006F0067006C0065", "Google")]
        [InlineData("0053004D0053002000520075006C007A", "SMS Rulz")]
        [InlineData("00480065006C006C006F002E", "Hello.")]
        [InlineData("00540068006900730020006900730020007400650073007400640061007400610021", "This is testdata!")]
        [InlineData("00540068006500200071007500690063006B002000620072006F0077006E00200066006F00780020006A0075006D007000730020006F00760065007200200074006800650020006C0061007A007900200064006F0067", "The quick brown fox jumps over the lazy dog")]
        public void Decoder_returns_decoded_text(string encoded, string expected)
        {
            string result = UCS2.Decode(encoded);

            Assert.Equal(expected, result);
        }
    }
}
