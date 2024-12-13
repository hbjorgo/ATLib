using HeboTech.ATLib.Numbering;
using System;

namespace HeboTech.ATLib.Messaging
{
    /// <summary>
    /// Data object for a received SMS
    /// </summary>
    public class SmsDeliver : Sms
    {
        public SmsDeliver(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message, DateTimeOffset timestamp)
            : base(MessageTypeIndicatorInbound.SMS_DELIVER)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
        }

        public SmsDeliver(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message, DateTimeOffset timestamp, int messageReference, int totalNumberOfParts, int partNumber)
            : base(MessageTypeIndicatorInbound.SMS_DELIVER, messageReference)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
            TotalNumberOfParts = totalNumberOfParts;
            PartNumber = partNumber;
        }

        public PhoneNumber ServiceCenterNumber { get; }
        public PhoneNumber SenderNumber { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
        public int TotalNumberOfParts { get; }
        public int PartNumber { get; }

        public void DeliverMethod()
        {
        }

        public override string ToString()
        {
            return base.ToString() + $" Timestamp: {Timestamp}. From: {SenderNumber}. Message: {Message}.";
        }
    }
}
