using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.PDU;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class PduTests
    {

        [Theory]
        [InlineData("56840182", "D430390C", CodingScheme.Gsm7, true, "0011000882654810280000AA04D430390C")]
        [InlineData("56840182", "D430390CD2A500", CodingScheme.Gsm7, true, "0011000882654810280000AA07D430390CD2A500")]
        public void Encode_SmsSubmit_test(string phoneNumber, string encodedMessage, CodingScheme dataCodingScheme, bool includeEmptySmscLength, string answer)
        {
            string encoded = Pdu.EncodeSmsSubmit(new PhoneNumber(phoneNumber), encodedMessage, dataCodingScheme, includeEmptySmscLength);

            Assert.Equal(answer, encoded);
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
            Assert.Equal(TypeOfNumber.International, pduMessage.ServiceCenterNumber.GetTypeOfNumber());
            Assert.Equal(serviceCenterNumber, pduMessage.ServiceCenterNumber.Number);
            Assert.Equal(senderNumber, pduMessage.SenderNumber.Number);
            Assert.Equal(timestamp, pduMessage.Timestamp.ToString("yy-MM-dd-HH-mm-ss-zz"));
            Assert.Equal(message, pduMessage.Message);
        }

        [Theory]
        [InlineData("0011000B916407281553F80000AA0AE8329BFD4697D9EC37", "", "+46708251358", "hellohello")]
        [InlineData("058178563412110008812143658700000B2B54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719", "87654321", "12345678", "The quick brown fox jumps over the lazy dog")]
        [InlineData("0011000802231537180000AA0D5062154403D1CB68D03DED06", "", "32517381", "PDU 4 teh win")]
        public void Decode_SmsSubmit_tests(string data, string serviceCenterNumber, string senderNumber, string message)
        {
#if NETFRAMEWORK
            SmsSubmit pduMessage = Pdu.DecodeSmsSubmit(data.AsSpan());
#else
            SmsSubmit pduMessage = Pdu.DecodeSmsSubmit(data);
#endif

            Assert.NotNull(pduMessage);
            Assert.Equal(serviceCenterNumber, pduMessage.ServiceCenterNumber?.Number ?? "");
            Assert.Equal(senderNumber, pduMessage.SenderNumber.Number);
            Assert.Equal(message, pduMessage.Message);
        }
    }
}
