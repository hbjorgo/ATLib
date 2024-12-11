using HeboTech.ATLib.PDU;
using System;

namespace HeboTech.ATLib.DTOs
{
    /// <summary>
    /// Data object for a received SMS
    /// </summary>
    public class SmsDeliver : Sms
    {
        public SmsDeliver(PhoneNumberDto serviceCenterNumber, PhoneNumberDto senderNumber, string message, DateTimeOffset timestamp)
            : base(MessageTypeIndicatorInbound.SMS_DELIVER)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
        }

        public SmsDeliver(PhoneNumberDto serviceCenterNumber, PhoneNumberDto senderNumber, string message, DateTimeOffset timestamp, int messageReference, int totalNumberOfParts, int partNumber)
            : base(MessageTypeIndicatorInbound.SMS_DELIVER, messageReference)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
            TotalNumberOfParts = totalNumberOfParts;
            PartNumber = partNumber;
        }

        public PhoneNumberDto ServiceCenterNumber { get; }
        public PhoneNumberDto SenderNumber { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
        public int TotalNumberOfParts { get; }
        public int PartNumber { get; }

        public void DeliverMethod()
        {
        }

        public override string ToString()
        {
            return base.ToString() +  $" Timestamp: {Timestamp}. From: {SenderNumber}. Message: {Message}.";
        }
    }
}
