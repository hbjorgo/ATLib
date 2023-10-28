using System;

namespace HeboTech.ATLib.DTOs
{
    public class SmsDeliver
    {
        public SmsDeliver(PhoneNumberDTO serviceCenterNumber, PhoneNumberDTO senderNumber, string message, DateTimeOffset timestamp)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
        }

        public PhoneNumberDTO ServiceCenterNumber { get; }
        public PhoneNumberDTO SenderNumber { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
    }
}
