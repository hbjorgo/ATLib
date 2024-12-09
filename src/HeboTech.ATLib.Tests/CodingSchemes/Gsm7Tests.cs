using HeboTech.ATLib.CodingSchemes;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class Gsm7Tests
    {
        [Theory]
        [InlineData("41", "41")] // A
        [InlineData("4142", "4121")] // AB
        [InlineData("54616461203A29", "D430390CD2A500")] // Tada :)
        [InlineData("54686520717569636b2062726f776e20666f78206a756d7073206f76657220746865206c617a7920646f67", "54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719")] // "The quick brown fox jumps over the lazy dog"
        [InlineData("44657369676E00486F6D65", "C4F23C7D760390EF7619")] // Design@Home
        [InlineData("4C696E65311B0A4C696E6532", "CCB4BB1CDB289869775906")] // Line1\r\nLine2
        public void Pack_returns_packed_bytes(string gsm7Bit, string expected)
        {
            byte[] result = Gsm7.Pack(Convert.FromHexString(gsm7Bit));

            Assert.Equal(Convert.FromHexString(expected), result);
        }

        [Theory]
        [InlineData("41" , "41")] // A
        [InlineData("4121", "4142")] // AB
        [InlineData("D430390CD2A500", "54616461203A29")] // Tada :)
        [InlineData("54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719", "54686520717569636b2062726f776e20666f78206a756d7073206f76657220746865206c617a7920646f67")] // "The quick brown fox jumps over the lazy dog"
        [InlineData("C4F23C7D760390EF7619", "44657369676E00486F6D65")] // Design@Home
        [InlineData("CCB4BB1CDB289869775906", "4C696E65311B0A4C696E6532")] // Line1\r\nLine2
        public void Unpack_returns_unpacked_bytes(string gsm7Bit, string expected)
        {
            byte[] result = Gsm7.Unpack(Convert.FromHexString(gsm7Bit));

            Assert.Equal(Convert.FromHexString(expected), result);
        }

        [Theory]
        [InlineData("A", "41")]
        [InlineData("AB", "4121")]
        [InlineData("The quick brown fox jumps over the lazy dog", "54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719")]
        [InlineData("Tada :)", "D430390CD2A500")]
        [InlineData("HELLO123", "C82293F98CC966")]
        [InlineData("Design@Home", "C4F23C7D760390EF7619")]
        [InlineData("Hello world", "C8329BFD06DDDF723619")]
        [InlineData("It is easy to send text messages.", "493A283D0795C3F33C88FE06CDCB6E32885EC6D341EDF27C1E3E97E72E")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Ut enim ad minim veniam, quis nostru", "CCB7BCDC06A5E1F37A1B447EB3DF72D03C4D0785DB653A0B347EBBE7E531BD4CAFCB4161721A9E9EA7C769F7195466A7E92CD0BC4C0691DFA072BA3E6FBFC9207AB90D7FCB4169F7384D4E93EB6E3AA84E07B1C3E2B7BC0C2AD341E437FB2D2F83DAE1B33B0C0AB3D3F17AD855A583CAEE741B142683DA6977BA0DB297DDE9709B058AD7D37390FB3DA7CBEB")] // 160 characters
        public void EncodeToBytes_returns_encoded_bytes(string gsm7Bit, string expected)
        {
            byte[] result = Gsm7.Encode(gsm7Bit.ToCharArray());

            Assert.Equal(Convert.FromHexString(expected), result);
        }

        [Theory]
        [InlineData("Hello world", 1, "906536FB0DBABFE56C32")]
        public void EncodeToBytes_with_padding_returns_encoded_bytes(string gsm7Bit, int padding, string expected)
        {
            byte[] result = Gsm7.Encode(gsm7Bit.ToCharArray(), padding);

            Assert.Equal(Convert.FromHexString(expected), result);
        }

        [Theory]
        [InlineData("41", "A")]
        [InlineData("4121", "AB")]
        [InlineData("54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719", "The quick brown fox jumps over the lazy dog")]
        [InlineData("D430390CD2A500", "Tada :)")]
        [InlineData("C82293F98CC966", "HELLO123")]
        [InlineData("C4F23C7D760390EF7619", "Design@Home")]
        [InlineData("CCB7BCDC06A5E1F37A1B447EB3DF72D03C4D0785DB653A0B347EBBE7E531BD4CAFCB4161721A9E9EA7C769F7195466A7E92CD0BC4C0691DFA072BA3E6FBFC9207AB90D7FCB4169F7384D4E93EB6E3AA84E07B1C3E2B7BC0C2AD341E437FB2D2F83DAE1B33B0C0AB3D3F17AD855A583CAEE741B142683DA6977BA0DB297DDE9709B058AD7D37390FB3DA7CBEB", "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Ut enim ad minim veniam, quis nostru")] // 160 characters
        [InlineData("CC98822903", "L1\nL2")]
        [InlineData("493A283D0795C3F33C88FE06CDCB6E32885EC6D341EDF27C1E3E97E72E", "It is easy to send text messages.")]
        public void DecodeFromBytes_returns_decoded_text(string gsm7Bit, string expected)
        {
            string result = Gsm7.Decode(Convert.FromHexString(gsm7Bit));

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("\u001B", "1B")] // ESC character at the end of a string results in a space
        [InlineData(" ", "20")] // Space character at the end of a string results in a space
        [InlineData("  ", "2010")] // Two spaces at the end of a string results in two spaces
        [InlineData("{", "1B14")]
        [InlineData("{}", "1BD42605")]
        [InlineData("()", "A814")]
        public void EncodeToBytes_returns_encoded_bytes_with_default_extension_table(string text, string expected)
        {
            byte[] result = Gsm7.Encode(text);

            Assert.Equal(Convert.FromHexString(expected), result);
        }

        [Theory]
        [InlineData("À", Gsm7Extension.Portugese, Gsm7Extension.Portugese, "14")]
        [InlineData("Φ", Gsm7Extension.Portugese, Gsm7Extension.Portugese, "1B09")]
        [InlineData("ΦΣ", Gsm7Extension.Portugese, Gsm7Extension.Portugese, "1BC90603")]
        public void EncodeToBytes_returns_encoded_bytes_with_extension_table(string gsm7Bit, Gsm7Extension singleShift, Gsm7Extension lockingShift, string expected)
        {
            byte[] result = Gsm7.Encode(gsm7Bit, 0, singleShift, lockingShift);

            Assert.Equal(Convert.FromHexString(expected), result);
        }

        [Theory]
        [InlineData("1B", " ")] // ESC character at the end of a string results in a space
        [InlineData("20", " ")] // Space results in a space
        [InlineData("2010", "  ")] // Two spaces at the end of a string results in two spaces
        [InlineData("1B14", "{")]
        [InlineData("1BD42605", "{}")]
        [InlineData("A814", "()")]
        public void DecodeFromBytes_returns_decoded_text_with_default_extension_table(string gsm7Bit, string expected)
        {
            string result = Gsm7.Decode(Convert.FromHexString(gsm7Bit));

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("14", Gsm7Extension.Portugese, Gsm7Extension.Portugese, "À")]
        [InlineData("1B", Gsm7Extension.Portugese, Gsm7Extension.Portugese, " ")]
        [InlineData("1B09", Gsm7Extension.Portugese, Gsm7Extension.Portugese, "Φ")]
        public void DecodeFromBytes_returns_decoded_text_with_extension_table(string gsm7Bit, Gsm7Extension singleShift, Gsm7Extension lockingShift, string expected)
        {
            string result = Gsm7.Decode(Convert.FromHexString(gsm7Bit), 0, singleShift, lockingShift);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("A", 0, "41")]
        [InlineData("Hello world", 0, "C8329BFD06DDDF723619")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostru", 0, "CCB7BCDC06A5E1F37A1B447EB3DF72D03C4D0785DB653A0B347EBBE7E531BD4CAFCB4161721A9E9E8FD3EE33A8CC4ED359A079990C22BF41E5747DDE7E9341F4721BFE9683D2EE719A9C26D7DD74509D0E6287C56F791954A683C86FF65B5E06B5C36777181466A7E3F5B00B54A583CAEE741B142683DA6977BA0DB297DDE9709B058AD7D37390FB3DA7CBEB")]
        [InlineData("Hello world", 1, "906536FB0DBABFE56C32")]
        [InlineData("Hi", 2, "20D3")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Ut enim ad minim veniam, quis", 1, "986F79B90D4AC3E7F53688FC66BFE5A0799A0E0AB7CB741668FC76CFCB637A995E9783C2E4343C3D4F8FD3EE33A8CC4ED359A079990C22BF41E5747DDE7E9341F4721BFE9683D2EE719A9C26D7DD74509D0E6287C56F791954A683C86FF65B5E06B5C36777181466A7E3F5B0AB4A0795DDE936284C06B5D3EE741B642FBBD3E1360B14AFA7E7")]
        [InlineData(" nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolor", 1, "40EEF79C2EAF9341657C593E4ED3C3F4F4DB0DAAB3D9E1F6F80D6287C56F797A0E72A7E769509D0E0AB3D3F17A1A0E2AE341E53068FC6EB7DFE43768FC76CFCBF17A98EE22D6D37350B84E2F83D2F2BABC0C22BFD96F3928ED06C9CB7079195D7693CBF2341D947683EC6F761D4E0FD3CB207B999DA683CAF37919344EB3D9F53688FC66BFE5")]
        [InlineData("e eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", 1, "CAA0721D64AE9FD3613AC85D67B3C32078589E0ED3EB7257113F2EC3E9E5BA1C344FBBE9A0F7781C2E8FC374D0B80E4F93C3F4301DE47EBB4170F93B4D2EBBE92CD0BCEEA683D26ED0B8CE868741F17A1AF4369BD3E37418442ECFCBF2BA9B0E6ABFD9EC341D1476A7DBA03419549ED341ECB0F82DAFB75D")]
        public void Encoder_returns_encoded_text_with_padding(string gsm7Bit, int paddingBits, string expected)
        {
            byte[] result = Gsm7.Encode(gsm7Bit, paddingBits);

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
        [InlineData("Line1\r\nLine2")]
        public void Encode_decode_returns_original_text(string text)
        {
            byte[] encoded = Gsm7.Encode(text);
            string result = Gsm7.Decode(encoded);

            Assert.Equal(text, result);
        }
    }
}
