using HeboTech.ATLib.PDU;

namespace HeboTech.ATLib.DTOs
{
    public class SmsBase
    {
        protected SmsBase(MessageTypeIndicatorInbound messageTypeIndicator)
        {
            MessageTypeIndicator = messageTypeIndicator;
        }

        protected SmsBase(MessageTypeIndicatorInbound messageTypeIndicator, int messageReference)
            : this(messageTypeIndicator)
        {
            MessageReference = messageReference;
        }

        public int MessageReference { get; }
        public MessageTypeIndicatorInbound MessageTypeIndicator { get; }

        public override string ToString()
        {
            return $"MTI: {MessageTypeIndicator}, Msg. ref.: {MessageReference}";
        }
    }
}