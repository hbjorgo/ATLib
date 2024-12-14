using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Messaging;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class SmsStatusReportDecoderTests
    {
        [Theory]
        [InlineData("0006D60B911326880736F4111011719551401110117195714000", null, "+31628870634", "11.01.2011 17:59:17 +01:00", "11.01.2011 17:59:15 +01:00", 214)]
        [InlineData("07911326040000F006D60B911326880736F4111011719551401110117195714000", "+31624000000", "+31628870634", "11.01.2011 17:59:17 +01:00", "11.01.2011 17:59:15 +01:00", 214)]
        public void Decode_StatusReport_tests(string data, string serviceCenterNumber, string recipientAddress, string dischargeTime, string serviceCenterTimestamp, int messageReference)
        {
            var bytes = data.ToByteArray();
            SmsStatusReport report = SmsStatusReportDecoder.Decode(bytes);

            Assert.NotNull(report);
            Assert.Equal(serviceCenterNumber, report.ServiceCenterAddress?.ToString());
            Assert.Equal(SmsDeliveryStatus.Message_received_by_SME, report.Status);
            Assert.Equal(recipientAddress, report.RecipientAddress?.ToString());
            Assert.Equal(messageReference, report.MessageReference);
            Assert.Equal(DateTimeOffset.Parse(serviceCenterTimestamp), report.ServiceCenterTimestamp);
            Assert.Equal(DateTimeOffset.Parse(dischargeTime), report.DischargeTime);
            Assert.Equal(MessageTypeIndicatorInbound.SMS_STATUS_REPORT, report.MessageTypeIndicator);
        }
    }
}
