using System;

namespace HeboTech.ATLib.DTOs
{
    public class Sms
    {
        public Sms(SmsStatus status, PhoneNumberDTO sender, DateTimeOffset receiveTime, string message)
            : this(status, sender, receiveTime, message, 0, 1, 1)
        {
        }

        public Sms(SmsStatus status, PhoneNumberDTO sender, DateTimeOffset receiveTime, string message, int messageReferenceNumber, int totalNumberOfParts, int partNumber)
        {
            Status = status;
            Sender = sender;
            ReceiveTime = receiveTime;
            Message = message;
            MessageReferenceNumber = messageReferenceNumber;
            TotalNumberOfParts = totalNumberOfParts;
            PartNumber = partNumber;
        }

        public SmsStatus Status { get; }
        public PhoneNumberDTO Sender { get; }
        public DateTimeOffset ReceiveTime { get;}
        public string Message { get; }
        public int MessageReferenceNumber { get; }
        public int TotalNumberOfParts { get; }
        public int PartNumber { get; }

        public override string ToString()
        {
            return $"Sender:\t\t{Sender}{Environment.NewLine}ReceiveTime:\t{ReceiveTime}{Environment.NewLine}Ref. no.:\t{MessageReferenceNumber}{Environment.NewLine}Part:\t\t{PartNumber}/{TotalNumberOfParts}{Environment.NewLine}Message:\t{Message}";
        }
    }
}
