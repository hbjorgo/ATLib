using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.Modems
{
    public interface IModem : IDisposable
    {
        event EventHandler<IncomingCallEventArgs> IncomingCall;
        event EventHandler<MissedCallEventArgs> MissedCall;
        event EventHandler<SmsReceivedEventArgs> SmsReceived;

        CommandStatus AnswerIncomingCall();
        void Close();
        CommandStatus DeleteSms(int index);
        CommandStatus DisableEcho();
        CommandStatus EnterSimPin(PersonalIdentificationNumber pin);
        BatteryStatus GetBatteryStatus();
        DateTimeOffset? GetDateTime();
        ProductIdentificationInformation GetProductIdentificationInformation();
        SignalStrength GetSignalStrength();
        SimStatus GetSimStatus();
        CallDetails Hangup();
        IList<SmsWithIndex> ListSmss(SmsStatus smsStatus);
        Sms ReadSms(int index);
        SmsReference SendSms(PhoneNumber phoneNumber, string message);
        CommandStatus SetDateTime(DateTimeOffset value);
        CommandStatus SetSmsMessageFormat(SmsTextFormat format);
    }
}