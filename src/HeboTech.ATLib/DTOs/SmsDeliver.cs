using System;

namespace HeboTech.ATLib.DTOs
{
    /// <summary>
    /// Data object for a received SMS
    /// </summary>
    public class SmsDeliver
    {
        public SmsDeliver(PhoneNumberDTO serviceCenterNumber, PhoneNumberDTO senderNumber, string message, DateTimeOffset timestamp)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
        }

        public SmsDeliver(PhoneNumberDTO serviceCenterNumber, PhoneNumberDTO senderNumber, string message, DateTimeOffset timestamp, int messageReferenceNumber, int totalNumberOfParts, int partNumber)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
            MessageReferenceNumber = messageReferenceNumber;
            TotalNumberOfParts = totalNumberOfParts;
            PartNumber = partNumber;
        }

        public PhoneNumberDTO ServiceCenterNumber { get; }
        public PhoneNumberDTO SenderNumber { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
        public int MessageReferenceNumber { get; }
        public int TotalNumberOfParts { get; }
        public int PartNumber { get; }

        public override string ToString()
        {
            return $"From: {SenderNumber}. Message: {Message}. Timestamp: {Timestamp}";
        }
    }
}
