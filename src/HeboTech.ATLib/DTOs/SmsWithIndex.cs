using System;

namespace HeboTech.ATLib.DTOs
{
    public class SmsWithIndex : Sms
    {
        public SmsWithIndex(int index, SmsStatus status, PhoneNumberDTO sender, DateTimeOffset receiveTime, string message)
            : base(status, sender, receiveTime, message)
        {
            Index = index;
        }

        public int Index { get; }

        public override string ToString()
        {
            return $"Index:\t\t{Index}{Environment.NewLine}Sender:\t\t{Sender}{Environment.NewLine}ReceiveTime:\t{ReceiveTime}{Environment.NewLine}Message:\t{Message}";
        }
    }
}
