using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems
{
    public interface IModem : IDisposable
    {
        event EventHandler<IncomingCallEventArgs> IncomingCall;
        event EventHandler<MissedCallEventArgs> MissedCall;
        event EventHandler<SmsReceivedEventArgs> SmsReceived;

        Task<CommandStatus> AnswerIncomingCallAsync();
        void Close();
        Task<CommandStatus> DeleteSmsAsync(int index);
        Task<CommandStatus> DisableEchoAsync();
        Task<CommandStatus> EnterSimPinAsync(PersonalIdentificationNumber pin);
        Task<BatteryStatus> GetBatteryStatusAsync();
        Task<DateTimeOffset?> GetDateTimeAsync();
        Task<ProductIdentificationInformation> GetProductIdentificationInformationAsync();
        Task<SignalStrength> GetSignalStrengthAsync();
        Task<SimStatus> GetSimStatusAsync();
        Task<CallDetails> HangupAsync();
        Task<IList<SmsWithIndex>> ListSmssAsync(SmsStatus smsStatus);
        Task<Sms> ReadSmsAsync(int index);
        Task<SmsReference> SendSmsAsync(PhoneNumber phoneNumber, string message);
        Task<CommandStatus> SetDateTimeAsync(DateTimeOffset value);
        Task<CommandStatus> SetSmsMessageFormatAsync(SmsTextFormat format);
    }
}