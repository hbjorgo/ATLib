using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.Modems.SIMCOM
{
    public interface ISIM5320
    {
        event EventHandler<IncomingCallEventArgs> IncomingCall;
        event EventHandler<MissedCallEventArgs> MissedCall;
        event EventHandler<SmsReceivedEventArgs> SmsReceived;

        CommandStatus AnswerIncomingCall();
        void Close();
        CommandStatus DeleteSMS(int index);
        CommandStatus DisableEcho();
        CommandStatus EnterSimPin(PersonalIdentificationNumber pin);
        BatteryStatus GetBatteryStatus();
        DateTimeOffset? GetDateTime();
        ProductIdentificationInformation GetProductIdentificationInformation();
        RemainingPinPukAttempts GetRemainingPinPukAttempts();
        SignalStrength GetSignalStrength();
        SimStatus GetSimStatus();
        CallDetails Hangup();
        IList<SmsWithIndex> ListSMSs(SmsStatus smsStatus);
        Sms ReadSMS(int index);
        SmsReference SendSMS(PhoneNumber phoneNumber, string message);
        CommandStatus SetDateTime(DateTimeOffset value);
        CommandStatus SetSmsMessageFormat(SmsTextFormat format);
    }
}