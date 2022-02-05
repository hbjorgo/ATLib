using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems
{
    public interface IModem : IDisposable
    {
        /// <summary>
        /// Indicates that a call has ended
        /// </summary>
        event EventHandler<CallEndedEventArgs> CallEnded;

        /// <summary>
        /// Indicates that a call has started
        /// </summary>
        event EventHandler<CallStartedEventArgs> CallStarted;

        /// <summary>
        /// Indicates that an error is received
        /// </summary>
        event EventHandler<ErrorEventArgs> ErrorReceived;

        /// <summary>
        /// Indicates that there is an incoming call
        /// </summary>
        event EventHandler<IncomingCallEventArgs> IncomingCall;

        /// <summary>
        /// Indicates that there is a missed call
        /// </summary>
        event EventHandler<MissedCallEventArgs> MissedCall;

        /// <summary>
        /// Indicates that an SMS is received
        /// </summary>
        event EventHandler<SmsReceivedEventArgs> SmsReceived;

        /// <summary>
        /// Indicates that a USSD response is received
        /// </summary>
        event EventHandler<UssdResponseEventArgs> UssdResponseReceived;

        /// <summary>
        /// Answers incoming call
        /// </summary>
        /// <returns>Command status</returns>
        Task<CommandStatus> AnswerIncomingCallAsync();

        /// <summary>
        /// Closes the connection
        /// </summary>
        void Close();

        /// <summary>
        /// Deletes an SMS in the preferred storage
        /// </summary>
        /// <param name="index">The SMS index</param>
        /// <returns>Command status</returns>
        Task<CommandStatus> DeleteSmsAsync(int index);

        /// <summary>
        /// Makes a call
        /// </summary>
        /// <param name="phoneNumber">The phone number to dial</param>
        /// <param name="hideCallerNumber">Enable/disable presentation of own phone number to called party</param>
        /// <param name="closedUserGroup">Enable/disable Closed User Group for this call</param>
        /// <returns>Command status</returns>
        Task<CommandStatus> DialAsync(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false);

        /// <summary>
        /// Disables modem echo
        /// </summary>
        /// <returns>Command status</returns>
        Task<CommandStatus> DisableEchoAsync();

        /// <summary>
        /// Enters the SIM PIN to unlock SIM card
        /// </summary>
        /// <param name="pin">The PIN to enter</param>
        /// <returns>Command status</returns>
        Task<CommandStatus> EnterSimPinAsync(PersonalIdentificationNumber pin);

        /// <summary>
        /// Gets the available character sets
        /// </summary>
        /// <returns>Character sets</returns>
        Task<IEnumerable<string>> GetAvailableCharacterSetsAsync();

        /// <summary>
        /// Gets the current battery status
        /// </summary>
        /// <returns>Battery status</returns>
        Task<BatteryStatus> GetBatteryStatusAsync();

        /// <summary>
        /// Gets the current character set
        /// </summary>
        /// <returns>Character set</returns>
        Task<string> GetCurrentCharacterSetAsync();

        /// <summary>
        /// Gets the current date and time
        /// </summary>
        /// <returns>Current date and time</returns>
        Task<DateTimeOffset?> GetDateTimeAsync();

        /// <summary>
        /// Gets the international mobile subscriber identity
        /// </summary>
        /// <returns>IMSI</returns>
        Task<Imsi> GetImsiAsync();

        /// <summary>
        /// Gets the product information (manufacturer id, model id, revision id, IMEI etc)
        /// </summary>
        /// <returns>Product information</returns>
        Task<ProductIdentificationInformation> GetProductIdentificationInformationAsync();

        /// <summary>
        /// Gets the signal quality
        /// </summary>
        /// <returns>Signal quality</returns>
        Task<SignalStrength> GetSignalStrengthAsync();

        /// <summary>
        /// Gets the SIM status
        /// </summary>
        /// <returns>SIM status</returns>
        Task<SimStatus> GetSimStatusAsync();

        /// <summary>
        /// Hangs up a call. After a call is ended, a CallEnded event occurs
        /// </summary>
        /// <returns>Command status</returns>
        Task<CommandStatus> HangupAsync();

        /// <summary>
        /// Lists SMSs with a given status from the preferred storage
        /// </summary>
        /// <param name="smsStatus"></param>
        /// <returns>A list of SMSs</returns>
        Task<IList<SmsWithIndex>> ListSmssAsync(SmsStatus smsStatus);

        Task SetPreferredMessageStorageAsync(string storage1, string storage2, string storage3);

        /// <summary>
        /// Reads an SMS from the preferred storage
        /// </summary>
        /// <param name="index"></param>
        /// <returns>SMS</returns>
        Task<Sms> ReadSmsAsync(int index);

        /// <summary>
        /// Sends an SMS. Note: Only text format is supported. Make sure to set the message format to text during initialization
        /// </summary>
        /// <param name="phoneNumber">The number to send to</param>
        /// <param name="message">The message body</param>
        /// <returns></returns>
        Task<SmsReference> SendSmsAsync(PhoneNumber phoneNumber, string message);

        /// <summary>
        /// Sends an USSD code. Results in an UssdResponseReceived event
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="codingScheme">Cell Broadcast Data Coding Scheme</param>
        /// <returns>Command status</returns>
        Task<CommandStatus> SendUssdAsync(string code, int codingScheme = 15);

        /// <summary>
        /// Sets the current character set. Get available character sets to see the supported sets
        /// </summary>
        /// <param name="characterSet"></param>
        /// <returns>Command status</returns>
        Task<CommandStatus> SetCharacterSetAsync(string characterSet);

        /// <summary>
        /// Sets the current date and time
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Command status</returns>
        Task<CommandStatus> SetDateTimeAsync(DateTimeOffset value);

        /// <summary>
        /// Sets how receiving a new SMS is indicated
        /// </summary>
        /// <param name="mode">mode</param>
        /// <param name="mt">mt</param>
        /// <param name="bm">bm</param>
        /// <param name="ds">ds</param>
        /// <param name="bfr">bfr</param>
        /// <returns></returns>
        Task<CommandStatus> SetNewSmsIndication(int mode, int mt, int bm, int ds, int bfr);

        /// <summary>
        /// Sets the input and output format of SMSs. Currently, only Text is supported and must be set before sending SMSs
        /// </summary>
        /// <param name="format">The format</param>
        /// <returns>Command status</returns>
        Task<CommandStatus> SetSmsMessageFormatAsync(SmsTextFormat format);
    }
}