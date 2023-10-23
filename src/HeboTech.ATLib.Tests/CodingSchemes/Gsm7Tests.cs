using HeboTech.ATLib.CodingSchemes;
using System;
using System.Text;
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
        [InlineData("Hello world", "C8329BFD06DDDF723619")]
        [InlineData("This is testdata!", "54747A0E4ACF41F4F29C4E0ED3C321")]
        [InlineData("The quick brown fox jumps over the lazy dog", "54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719")]
        [InlineData("Tada :)", "D430390CD2A500")]
        [InlineData("hellohello", "E8329BFD4697D9EC37")]
        [InlineData("Hi", "C834")]
        public void Encoder_returns_encoded_text(string gsm7Bit, string expected)
        {
            byte[] result = Gsm7.Pack(Gsm7.EncodeToBytes(gsm7Bit.ToCharArray()));

            Assert.Equal(Convert.FromHexString(expected), result);
        }

        [Theory]
        [InlineData("41", "A")]
        [InlineData("4121", "AB")]
        [InlineData("C834", "Hi")]
        [InlineData("41E110", "ABC")]
        [InlineData("C7F7FBCC2E03", "Google")]
        [InlineData("D430390CD2A500", "Tada :)")]
        [InlineData("D3E61424ADB3F5", "SMS Rulz")]
        [InlineData("C8329BFD7601", "Hello.")]
        [InlineData("C8329BFD06DDDF723619", "Hello world")]
        [InlineData("54747A0E4ACF41F4F29C4E0ED3C321", "This is testdata!")]
        [InlineData("54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719", "The quick brown fox jumps over the lazy dog")]
        public void Decoder_returns_decoded_text(string gsm7Bit, string expected)
        {
            byte[] result = Gsm7.Unpack(Convert.FromHexString(gsm7Bit));

            Assert.Equal(expected, Encoding.ASCII.GetString(result));
        }

        [Theory]
        [InlineData("A", 0, "41")]
        [InlineData("Hello world", 0, "C8329BFD06DDDF723619")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostru", 0, "CCB7BCDC06A5E1F37A1B447EB3DF72D03C4D0785DB653A0B347EBBE7E531BD4CAFCB4161721A9E9E8FD3EE33A8CC4ED359A079990C22BF41E5747DDE7E9341F4721BFE9683D2EE719A9C26D7DD74509D0E6287C56F791954A683C86FF65B5E06B5C36777181466A7E3F5B00B54A583CAEE741B142683DA6977BA0DB297DDE9709B058AD7D37390FB3DA7CBEB")]
        [InlineData("Hello world", 1, "906536FB0DBABFE56C3200")]
        [InlineData("Hi", 2, "20D300")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Ut enim ad minim veniam, quis", 1, "986F79B90D4AC3E7F53688FC66BFE5A0799A0E0AB7CB741668FC76CFCB637A995E9783C2E4343C3D4F8FD3EE33A8CC4ED359A079990C22BF41E5747DDE7E9341F4721BFE9683D2EE719A9C26D7DD74509D0E6287C56F791954A683C86FF65B5E06B5C36777181466A7E3F5B0AB4A0795DDE936284C06B5D3EE741B642FBBD3E1360B14AFA7E700")]
        [InlineData(" nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolor", 1, "40EEF79C2EAF9341657C593E4ED3C3F4F4DB0DAAB3D9E1F6F80D6287C56F797A0E72A7E769509D0E0AB3D3F17A1A0E2AE341E53068FC6EB7DFE43768FC76CFCBF17A98EE22D6D37350B84E2F83D2F2BABC0C22BFD96F3928ED06C9CB7079195D7693CBF2341D947683EC6F761D4E0FD3CB207B999DA683CAF37919344EB3D9F53688FC66BFE500")]
        [InlineData("e eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", 1, "CAA0721D64AE9FD3613AC85D67B3C32078589E0ED3EB7257113F2EC3E9E5BA1C344FBBE9A0F7781C2E8FC374D0B80E4F93C3F4301DE47EBB4170F93B4D2EBBE92CD0BCEEA683D26ED0B8CE868741F17A1AF4369BD3E37418442ECFCBF2BA9B0E6ABFD9EC341D1476A7DBA03419549ED341ECB0F82DAFB75D00")]
        public void Encoder_returns_encoded_text_with_padding(string gsm7Bit, int paddingBits, string expected)
        {
            byte[] result = Gsm7.Pack(Gsm7.EncodeToBytes(gsm7Bit), paddingBits);

            Assert.Equal(expected, BitConverter.ToString(result).Replace("-", ""));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AB")]
        [InlineData("ABC")]
        [InlineData("Google")]
        [InlineData("SMS Rulz")]
        [InlineData("Hello.")]
        [InlineData("Hello world")]
        [InlineData("This is testdata!")]
        [InlineData("The quick brown fox jumps over the lazy dog")]
        [InlineData("Tada :)")]
        [InlineData("hellohello")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud")]
        public void Encode_decode_returns_original_text(string text)
        {
            byte[] encoded = Gsm7.Pack(Gsm7.EncodeToBytes(text));
            byte[] decoded = Gsm7.Unpack(encoded);
            string receivedText = Encoding.ASCII.GetString(decoded);

            Assert.Equal(text, receivedText);
        }
    }
}
