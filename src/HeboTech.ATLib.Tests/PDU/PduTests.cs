using HeboTech.ATLib.PDU;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class PduTests
    {
        [Theory]
        [InlineData("07917238010010F5040BC87238880900F10000993092516195800AE8329BFD4697D9EC37", "+27831000015", "27838890001", "99-03-29-15-16-59-+02", "hellohello")]
        [InlineData("07911326040000F0040B911346610089F60000208062917314800CC8F71D14969741F977FD07", "+31624000000", "31641600986", "02-08-26-19-37-41-+02", "How are you?")]
        public void SMS_DELIVER_Tests(string data, string serviceCenterNumber, string senderNumber, string timestamp, string message)
        {
            PduMessage pduMessage = Pdu.Decode(data);

            Assert.NotNull(pduMessage);
            Assert.Equal(serviceCenterNumber, pduMessage.ServiceCenterNumber.Number);
            Assert.Equal(senderNumber, pduMessage.SenderNumber);
            Assert.Equal(timestamp, pduMessage.Timestamp.ToString("yy-MM-dd-HH-mm-ss-zz"));
            Assert.Equal(message, pduMessage.Message);
        }

        [Theory]
        [InlineData("0011000B916407281553F80000AA0AE8329BFD4697D9EC37", "", "46708251358", "", "hellohello")]
        [InlineData("068189674523F11100098121436587F90000FF2B54741914AFA7C76B9058FEBEBB41E6371EA4AEB7E173D0DB5E9683E8E832881DD6E741E4F719", "987654321", "123456789", "99-03-29-15-16-59-+02", "The quick brown fox jumps over the lazy dog")]
        public void SMS_SUBMIT_Tests(string data, string serviceCenterNumber, string senderNumber, string timestamp, string message)
        {
            PduMessage pduMessage = Pdu.Decode(data);

            Assert.NotNull(pduMessage);
            Assert.Null(pduMessage.ServiceCenterNumber);
            Assert.Equal(senderNumber, pduMessage.SenderNumber);
            //Assert.Equal(timestamp, pduMessage.Timestamp.ToString("yy-MM-dd-HH-mm-ss-zz"));
            Assert.Equal(message, pduMessage.Message);
        }
    }
}
