using HeboTech.ATLib.Messaging;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class SmsDecoderTests
    {
        [Theory]
        [InlineData("07917238010010F5040B917238880900F10000993092516195800AE8329BFD4697D9EC37", "+27831000015", "+27838890001", "29.03.2099 15:16:59 +02:00", "hellohello")]
        [InlineData("07911326040000F0040B911346610089F60000208062917314800CC8F71D14969741F977FD07", "+31624000000", "+31641600986", "26.08.2002 19:37:41 +02:00", "How are you?")]
        public void Decode_SmsDeliver(string data, string serviceCenterNumber, string senderNumber, string timestamp, string message)
        {
            var bytes = Convert.FromHexString(data);
            Sms sms = SmsDecoder.Decode(bytes);
            SmsDeliver smsDeliver = sms as SmsDeliver;

            Assert.NotNull(sms);
            Assert.Equal(MessageTypeIndicatorInbound.SMS_DELIVER, sms.MessageTypeIndicator);
            Assert.NotNull(smsDeliver);
            Assert.Equal(serviceCenterNumber, smsDeliver.ServiceCenterNumber.ToString());
            Assert.Equal(senderNumber, smsDeliver.SenderNumber.ToString());
            Assert.Equal(DateTimeOffset.Parse(timestamp), smsDeliver.Timestamp);
            Assert.Equal(message, smsDeliver.Message);
        }

        [Theory]
        [InlineData("06916309002100067708A025057218422180218500404221802185004000", "+3690001200", "52502781", "08.12.2024 12:58:00 +01:00", "08.12.2024 12:58:00 +01:00", 119, SmsDeliveryStatus.Message_received_by_SME)]
        [InlineData("06918509002100067808A025057218422180219500404221802195004000", "+5890001200", "52502781", "08.12.2024 12:59:00 +01:00", "08.12.2024 12:59:00 +01:00", 120, SmsDeliveryStatus.Message_received_by_SME)]
        [InlineData("06918509002100067808A025057218422180219500404221802195004000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "+5890001200", "52502781", "08.12.2024 12:59:00 +01:00", "08.12.2024 12:59:00 +01:00", 120, SmsDeliveryStatus.Message_received_by_SME)]
        public void Decode_SmsStatusReport(string data, string serviceCenterNumber, string senderNumber, string serviceCenterTimestamp, string dischargeTimestamp, int messageReference, SmsDeliveryStatus status)
        {
            var bytes = Convert.FromHexString(data);
            Sms sms = SmsDecoder.Decode(bytes);
            SmsStatusReport smsStatusReport = sms as SmsStatusReport;

            Assert.NotNull(sms);
            Assert.Equal(MessageTypeIndicatorInbound.SMS_STATUS_REPORT, sms.MessageTypeIndicator);
            Assert.NotNull(smsStatusReport);
            Assert.Equal(serviceCenterNumber, smsStatusReport.ServiceCenterAddress.ToString());
            Assert.Equal(senderNumber, smsStatusReport.RecipientAddress.ToString());
            Assert.Equal(DateTimeOffset.Parse(serviceCenterTimestamp), smsStatusReport.ServiceCenterTimestamp);
            Assert.Equal(DateTimeOffset.Parse(dischargeTimestamp), smsStatusReport.DischargeTime);
            Assert.Equal(status, smsStatusReport.Status);
            Assert.Equal(MessageTypeIndicatorInbound.SMS_STATUS_REPORT, smsStatusReport.MessageTypeIndicator);
            Assert.Equal(messageReference, smsStatusReport.MessageReference);
        }
    }
}
