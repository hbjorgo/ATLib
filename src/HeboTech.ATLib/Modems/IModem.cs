using HeboTech.ATLib.Events;
using HeboTech.ATLib.Inputs;
using HeboTech.ATLib.Results;
using System;

namespace HeboTech.ATLib.Modems
{
    public interface IModem
    {
        event EventHandler<IncomingCallEventArgs> IncomingCall;
        event EventHandler<MissedCallEventArgs> MissedCall;

        CommandStatus AnswerIncomingCall();
        CommandStatus DisableEcho();
        CommandStatus EnterSimPin(PersonalIdentificationNumber pin);
        BatteryStatus GetBatteryStatus();
        ProductIdentificationInformation GetProductIdentificationInformation();
        RemainingPinPukAttempts GetRemainingPinPukAttempts();
        SignalStrength GetSignalStrength();
        SimStatus GetSimStatus();
        CallDetails Hangup();
        SmsReference SendSMS(PhoneNumber phoneNumber, string message);
        CommandStatus SetDateTime(DateTimeOffset value);
        DateTimeOffset? GetDateTime();
    }
}