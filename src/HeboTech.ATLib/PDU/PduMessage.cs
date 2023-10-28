using HeboTech.ATLib.DTOs;
using System;

namespace HeboTech.ATLib.PDU
{
    public abstract class PduMessage
    {
        public PduMessage(PhoneNumberDTO serviceCenterNumber, PhoneNumberDTO senderNumber, string message)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
        }

        public PhoneNumberDTO ServiceCenterNumber { get; }
        public PhoneNumberDTO SenderNumber { get; }
        public string Message { get; }
    }

    public class SmsDeliver : PduMessage
    {
        public SmsDeliver(PhoneNumberDTO serviceCenterNumber, PhoneNumberDTO senderNumber, string message, DateTimeOffset timestamp)
            : base(serviceCenterNumber, senderNumber, message)
        {
            Timestamp = timestamp;
        }

        public DateTimeOffset Timestamp { get; }
    }

    public class SmsSubmit : PduMessage
    {
        public SmsSubmit(PhoneNumberDTO serviceCenterNumber, PhoneNumberDTO senderNumber, string message)
            : base(serviceCenterNumber, senderNumber, message)
        {
        }
    }
}
