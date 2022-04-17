using HeboTech.ATLib.DTOs;
using System;

namespace HeboTech.ATLib.PDU
{
    public abstract class PduMessage
    {
        public PduMessage(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
        }

        public PhoneNumber ServiceCenterNumber { get; }
        public PhoneNumber SenderNumber { get; }
        public string Message { get; }
    }

    public class SmsDeliver : PduMessage
    {
        public SmsDeliver(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message, DateTimeOffset timestamp)
            : base(serviceCenterNumber, senderNumber, message)
        {
            Timestamp = timestamp;
        }

        public DateTimeOffset Timestamp { get; }
    }

    public class SmsSubmit : PduMessage
    {
        public SmsSubmit(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message)
            : base(serviceCenterNumber, senderNumber, message)
        {
        }
    }
}
