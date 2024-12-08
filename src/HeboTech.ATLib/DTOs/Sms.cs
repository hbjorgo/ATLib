using HeboTech.ATLib.PDU;

namespace HeboTech.ATLib.DTOs
{
    public abstract class Sms
    {
        protected Sms(MessageTypeIndicatorInbound messageTypeIndicator)
        {
            MessageTypeIndicator = messageTypeIndicator;
        }

        protected Sms(MessageTypeIndicatorInbound messageTypeIndicator, int messageReference)
            : this(messageTypeIndicator)
        {
            MessageReference = messageReference;
        }

        public int MessageReference { get; }
        public MessageTypeIndicatorInbound MessageTypeIndicator { get; }

        public override string ToString()
        {
            return $"MTI: {MessageTypeIndicator}. Msg. ref.: {MessageReference}.";
        }
    }
}