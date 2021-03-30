using HeboTech.ATLib.Inputs;
using System;

namespace HeboTech.ATLib.DTOs
{
    public class SmsWithIndex : Sms
    {
        public SmsWithIndex(int index, SmsStatus status, PhoneNumber sender, DateTimeOffset receiveTime, string message)
            : base(status, sender, receiveTime, message)
        {
            Index = index;
        }

        public int Index { get; }
    }
}
