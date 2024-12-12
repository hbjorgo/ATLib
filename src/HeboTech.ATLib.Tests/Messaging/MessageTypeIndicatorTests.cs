using HeboTech.ATLib.Messaging;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class MessageTypeIndicatorTests
    {
        [Theory]
        [InlineData(MessageTypeIndicatorInbound.SMS_DELIVER, 0x00)]
        [InlineData(MessageTypeIndicatorInbound.SMS_SUBMIT_REPORT, 0x01)]
        [InlineData(MessageTypeIndicatorInbound.SMS_STATUS_REPORT, 0x02)]
        [InlineData(MessageTypeIndicatorInbound.Reserved, 0x03)]
        internal void Have_correct_inbound_values(MessageTypeIndicatorInbound messageTypeIndicator, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)messageTypeIndicator);
        }

        [Theory]
        [InlineData(MessageTypeIndicatorOutbound.SMS_DELIVER_REPORT, 0x00)]
        [InlineData(MessageTypeIndicatorOutbound.SMS_SUBMIT, 0x01)]
        [InlineData(MessageTypeIndicatorOutbound.SMS_COMMAND, 0x02)]
        [InlineData(MessageTypeIndicatorOutbound.Reserved, 0x03)]
        internal void Have_correct_outbound_values(MessageTypeIndicatorOutbound messageTypeIndicator, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)messageTypeIndicator);
        }
    }
}
