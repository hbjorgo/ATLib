using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.PDU;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class SmsDeliverDecoderTests
    {
        [Theory]
        [InlineData("07917238010010F5040BC87238880900F10000993092516195800AE8329BFD4697D9EC37", "+27831000015", "27838890001", "99-03-29-15-16-59-+02", "hellohello")]
        [InlineData("07911326040000F0040B911346610089F60000208062917314800CC8F71D14969741F977FD07", "+31624000000", "+31641600986", "02-08-26-19-37-41-+02", "How are you?")]
        public void Decode_SmsDeliver_tests(string data, string serviceCenterNumber, string senderNumber, string timestamp, string message)
        {
            var bytes = data.ToByteArray();
            SmsDeliver pduMessage = SmsDeliverDecoder.Decode(bytes);

            Assert.NotNull(pduMessage);
            Assert.Equal(serviceCenterNumber, pduMessage.ServiceCenterNumber.ToString());
            Assert.Equal(senderNumber, pduMessage.SenderNumber.ToString());
            Assert.Equal(timestamp, pduMessage.Timestamp.ToString("yy-MM-dd-HH-mm-ss-zz"));
            Assert.Equal(message, pduMessage.Message);
        }
    }
}
