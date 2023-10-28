using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.PDU;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class PduTests
    {

        [Theory]
        [InlineData("", "56840182", "Tada", CodingScheme.Gsm7, true, new string[] { "00010008A065481028000004D430390C" })]
        [InlineData("", "56840182", "Tada :)", CodingScheme.Gsm7, true, new string[] { "00010008A065481028000007D430390CD2A500" })]
        [InlineData("", "12345678", "A", CodingScheme.Gsm7, true, new string[] { "00010008A02143658700000141" })]
        [InlineData("", "12345678", "A", CodingScheme.UCS2, true, new string[] { "00010008A0214365870008020041" })]
        [InlineData("", "12345678", "A", CodingScheme.UCS2, false, new string[] { "010008A0214365870008020041" })]
        [InlineData("", "12345678", "😀", CodingScheme.UCS2, true, new string[] { "00010008A021436587000804D83DDE00" })]
        [InlineData("", "12345678", "😀😹📱📶📞", CodingScheme.UCS2, true, new string[] { "00010008A021436587000814D83DDE00D83DDE39D83DDCF1D83DDCF6D83DDCDE" })]
        [InlineData("", "12345678", "A😀B😹C📱D📶E📞F", CodingScheme.UCS2, true, new string[] { "00010008A0214365870008200041D83DDE000042D83DDE390043D83DDCF10044D83DDCF60045D83DDCDE0046" })]
        [InlineData(
            "1",
            "5125551234",
            "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
            CodingScheme.Gsm7,
            true,
            new string[] {
                        "0041000B915121551532F40000A00500030C0301986F79B90D4AC3E7F53688FC66BFE5A0799A0E0AB7CB741668FC76CFCB637A995E9783C2E4343C3D4F8FD3EE33A8CC4ED359A079990C22BF41E5747DDE7E9341F4721BFE9683D2EE719A9C26D7DD74509D0E6287C56F791954A683C86FF65B5E06B5C36777181466A7E3F5B0AB4A0795DDE936284C06B5D3EE741B642FBBD3E1360B14AFA7E700",
            "0041000B915121551532F40000A00500030C030240EEF79C2EAF9341657C593E4ED3C3F4F4DB0DAAB3D9E1F6F80D6287C56F797A0E72A7E769509D0E0AB3D3F17A1A0E2AE341E53068FC6EB7DFE43768FC76CFCBF17A98EE22D6D37350B84E2F83D2F2BABC0C22BFD96F3928ED06C9CB7079195D7693CBF2341D947683EC6F761D4E0FD3CB207B999DA683CAF37919344EB3D9F53688FC66BFE500",
            "0041000B915121551532F40000900500030C0303CAA0721D64AE9FD3613AC85D67B3C32078589E0ED3EB7257113F2EC3E9E5BA1C344FBBE9A0F7781C2E8FC374D0B80E4F93C3F4301DE47EBB4170F93B4D2EBBE92CD0BCEEA683D26ED0B8CE868741F17A1AF4369BD3E37418442ECFCBF2BA9B0E6ABFD9EC341D1476A7DBA03419549ED341ECB0F82DAFB75D00"
            })]

        public void Encode_SmsSubmit_test(string countryCode, string subscriberNumber, string encodedMessage, CodingScheme dataCodingScheme, bool includeEmptySmscLength, string[] answer)
        {
            IEnumerable<string> encoded = Pdu.EncodeSmsSubmit(
                new SmsSubmitRequest(
                    new PhoneNumber(countryCode, subscriberNumber),
                    encodedMessage,
                    dataCodingScheme)
                {
                    IncludeEmptySmscLength = includeEmptySmscLength,
                    MessageReferenceNumber = 12
                });

            Assert.Equal(answer, encoded.ToArray());
        }

        [Theory]
        [InlineData("", "56840182", 39_016, CodingScheme.Gsm7, true)] // Max message length is 39015 characters
        [InlineData("", "56840182", 17_086, CodingScheme.UCS2, true)] // Max message length is 17085 characters
        public void Encode_SmsSubmit_message_too_long_test(string countryCode, string subscriberNumber, int characterCount, CodingScheme dataCodingScheme, bool includeEmptySmscLength)
        {
            var request = new SmsSubmitRequest(
                    new PhoneNumber(countryCode, subscriberNumber),
                    new string('a', characterCount),
                    dataCodingScheme)
                    {
                        IncludeEmptySmscLength = includeEmptySmscLength,
                        MessageReferenceNumber = 12
                    };
            Assert.Throws<ArgumentException>(() => Pdu.EncodeSmsSubmit(request).ToList());
        }

        [Theory]
        [InlineData("07917238010010F5040BC87238880900F10000993092516195800AE8329BFD4697D9EC37", "+27831000015", "27838890001", "99-03-29-15-16-59-+02", "hellohello")]
        [InlineData("07911326040000F0040B911346610089F60000208062917314800CC8F71D14969741F977FD07", "+31624000000", "+31641600986", "02-08-26-19-37-41-+02", "How are you?")]
        public void Decode_SmsDeliver_tests(string data, string serviceCenterNumber, string senderNumber, string timestamp, string message)
        {
#if NETFRAMEWORK
            SmsDeliver pduMessage = Pdu.DecodeSmsDeliver(data.AsSpan());
#else
            SmsDeliver pduMessage = Pdu.DecodeSmsDeliver(data);
#endif

            Assert.NotNull(pduMessage);
            Assert.Equal(serviceCenterNumber, pduMessage.ServiceCenterNumber.ToString());
            Assert.Equal(senderNumber, pduMessage.SenderNumber.ToString());
            Assert.Equal(timestamp, pduMessage.Timestamp.ToString("yy-MM-dd-HH-mm-ss-zz"));
            Assert.Equal(message, pduMessage.Message);
        }
    }
}
