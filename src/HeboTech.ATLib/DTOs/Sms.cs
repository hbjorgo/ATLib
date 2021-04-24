using System;

namespace HeboTech.ATLib.DTOs
{
    public class Sms
    {
        public Sms(SmsStatus status, PhoneNumber sender, DateTimeOffset receiveTime, string message)
        {
            Status = status;
            Sender = sender;
            ReceiveTime = receiveTime;
            Message = message;
        }

        public SmsStatus Status { get; }
        public PhoneNumber Sender { get; }
        public DateTimeOffset ReceiveTime { get;}
        public string Message { get; }

        public override string ToString()
        {
            return $"Sender: {Sender}, ReceiveTime: {ReceiveTime}, Message:{Environment.NewLine}{Message}";
        }
    }
}
