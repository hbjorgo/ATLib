﻿using HeboTech.ATLib.PDU;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class PduTests
    {
        [Theory]
        [InlineData("07917238010010F5040BC87238880900F10000993092516195800AE8329BFD4697D9EC37", "+27831000015", "+27838890001", "99-03-29-15-16-59-+02", "hellohello")]
        public void OkTest(string data, string serviceCenterNumber, string senderNumber, string timestamp, string message)
        {
            PduMessage pduMessage = Pdu.Decode(data);

            Assert.NotNull(pduMessage);
            Assert.Equal(serviceCenterNumber, pduMessage.ServiceCenterNumber);
            Assert.Equal(senderNumber, pduMessage.SenderNumber);
            Assert.Equal(timestamp, pduMessage.Timestamp.ToString("yy-MM-dd-HH-mm-ss-zz"));
            Assert.Equal(message, pduMessage.Message);
        }
    }
}