using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
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
        /// Indicates that an event with no specific event handler is received
        /// </summary>
        event EventHandler<GenericEventArgs> GenericEvent;

        /// <summary>
        /// Answers incoming call
        /// </summary>
        /// <returns>Command status</returns>
        Task<ModemResponse> AnswerIncomingCallAsync();

        /// <summary>
        /// Closes the connection
        /// </summary>
        void Close();

        /// <summary>
        /// Deletes an SMS in the preferred storage
        /// </summary>
        /// <param name="index">The SMS index</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> DeleteSmsAsync(int index);

        /// <summary>
        /// Makes a call
        /// </summary>
        /// <param name="phoneNumber">The phone number to dial</param>
        /// <param name="hideCallerNumber">Enable/disable presentation of own phone number to called party</param>
        /// <param name="closedUserGroup">Enable/disable Closed User Group for this call</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> DialAsync(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false);

        /// <summary>
        /// Disables modem echo
        /// </summary>
        /// <returns>Command status</returns>
        Task<ModemResponse> DisableEchoAsync();

        /// <summary>
        /// Enters the SIM PIN to unlock SIM card
        /// Note: It might take some time from entering SIM PIN before the response is ready.
        /// Consider waiting ~1500ms after entering SIM PIN to let the SIM initialize.
        /// </summary>
        /// <param name="pin">The PIN to enter</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> EnterSimPinAsync(PersonalIdentificationNumber pin);

        /// <summary>
        /// Gets the available character sets
        /// </summary>
        /// <returns>Command status with character sets</returns>
        Task<ModemResponse<IEnumerable<string>>> GetAvailableCharacterSetsAsync();

        /// <summary>
        /// Gets the current battery status
        /// </summary>
        /// <returns>Command status with battery status</returns>
        Task<ModemResponse<BatteryStatus>> GetBatteryStatusAsync();

        /// <summary>
        /// Gets the current character set
        /// </summary>
        /// <returns>Command status with character set</returns>
        Task<ModemResponse<string>> GetCurrentCharacterSetAsync();

        /// <summary>
        /// Gets the current date and time
        /// </summary>
        /// <returns>Command status with current date and time</returns>
        Task<ModemResponse<DateTimeOffset>> GetDateTimeAsync();

        /// <summary>
        /// Gets the international mobile subscriber identity
        /// </summary>
        /// <returns>Command status with IMSI</returns>
        Task<ModemResponse<Imsi>> GetImsiAsync();

        /// <summary>
        /// Gets the product information (manufacturer id, model id, revision id, IMEI etc)
        /// </summary>
        /// <returns>Command status with product information</returns>
        Task<ModemResponse<ProductIdentificationInformation>> GetProductIdentificationInformationAsync();

        /// <summary>
        /// Gets the signal quality
        /// </summary>
        /// <returns>Command status with signal quality</returns>
        Task<ModemResponse<SignalStrength>> GetSignalStrengthAsync();

        /// <summary>
        /// Gets the SIM status
        /// </summary>
        /// <returns>Command status with SIM status</returns>
        Task<ModemResponse<SimStatus>> GetSimStatusAsync();

        /// <summary>
        /// Hangs up a call. After a call is ended, a CallEnded event occurs
        /// </summary>
        /// <returns>Command status</returns>
        Task<ModemResponse> HangupAsync();

        /// <summary>
        /// Lists SMSs with a given status from the preferred storage
        /// </summary>
        /// <param name="smsStatus"></param>
        /// <returns>Command status with a list of SMSs</returns>
        Task<ModemResponse<List<SmsWithIndex>>> ListSmssAsync(SmsStatus smsStatus);

        /// <summary>
        /// Set preferred message storages
        /// </summary>
        /// <param name="storage1Name"></param>
        /// <param name="storage2Name"></param>
        /// <param name="storage3Name"></param>
        /// <returns>Command status with set preferred message storages</returns>
        Task<ModemResponse<PreferredMessageStorages>> SetPreferredMessageStorageAsync(MessageStorage storage1Name, MessageStorage storage2Name, MessageStorage storage3Name);

        /// <summary>
        /// Get supported preferred message storages
        /// </summary>
        /// <returns>Command status with supported preferred message storages/returns>
        Task<ModemResponse<SupportedPreferredMessageStorages>> GetSupportedPreferredMessageStoragesAsync();

        /// <summary>
        /// Get preferred message storages
        /// </summary>
        /// <returns>Command status with preferred message storages</returns>
        Task<ModemResponse<PreferredMessageStorages>> GetPreferredMessageStoragesAsync();

        /// <summary>
        /// Reads an SMS from the preferred storage
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Command status with SMS</returns>
        Task<ModemResponse<Sms>> ReadSmsAsync(int index, SmsTextFormat smsTextFormat);

        /// <summary>
        /// Reload and initialize the SIM card
        /// </summary>
        /// <returns>Command status</returns>
        Task<ModemResponse> ReInitializeSimAsync();

        /// <summary>
        /// Sends an SMS in text format
        /// </summary>
        /// <param name="phoneNumber">The number to send to</param>
        /// <param name="message">The message body</param>
        /// <returns>Command status with SMS reference</returns>
        Task<ModemResponse<SmsReference>> SendSmsInTextFormatAsync(PhoneNumber phoneNumber, string message);

        /// <summary>
        /// Sends an SMS in PDU format. This will automatically select the Data Coding Scheme that will result in the fewest messages being sent in case of a concatenated SMS based on the content of the message.
        /// </summary>
        /// <param name="phoneNumber">The number to send to</param>
        /// <param name="message">The message body</param>
        /// <returns>Command status with SMS reference</returns>
        Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message);

        /// <summary>
        /// Sends an SMS in PDU format
        /// </summary>
        /// <param name="phoneNumber">The number to send to</param>
        /// <param name="message">The message body</param>
        /// <param name="codingScheme">Encoding to use</param>
        /// <returns>Command status with SMS reference</returns>
        Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme);

        /// <summary>
        /// Sends an USSD code. Results in an UssdResponseReceived event
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="codingScheme">Cell Broadcast Data Coding Scheme</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> SendUssdAsync(string code, int codingScheme = 15);

        /// <summary>
        /// Sets the current character set. Get available character sets to see the supported sets
        /// </summary>
        /// <param name="characterSet"></param>
        /// <returns>Command status</returns>
        Task<ModemResponse> SetCharacterSetAsync(string characterSet);

        /// <summary>
        /// Sets the current date and time
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Command status</returns>
        Task<ModemResponse> SetDateTimeAsync(DateTimeOffset value);

        /// <summary>
        /// Set error format
        /// </summary>
        /// <param name="errorFormat">Typical: 0 (disable), 1 (numeric), 2 (verbose)</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> SetErrorFormat(int errorFormat);

        /// <summary>
        /// Sets how receiving a new SMS is indicated
        /// </summary>
        /// <param name="mode">mode</param>
        /// <param name="mt">mt</param>
        /// <param name="bm">bm</param>
        /// <param name="ds">ds</param>
        /// <param name="bfr">bfr</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> SetNewSmsIndication(int mode, int mt, int bm, int ds, int bfr);

        /// <summary>
        /// Sets the input and output format of SMSs. Currently, only Text is supported and must be set before sending SMSs
        /// </summary>
        /// <param name="format">The format</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> SetSmsMessageFormatAsync(SmsTextFormat format);

        /// <summary>
        /// Sets whether or not detailed header information is shown in text mode result codes
        /// </summary>
        /// <param name="activate">True to activate, false to deactivate</param>
        /// <returns>Command status</returns>
        Task<ModemResponse> ShowSmsTextModeParameters(bool activate);

        /// <summary>
        /// Sets settings required for correct operation before PIN is entered
        /// </summary>
        /// <returns>Command status</returns>
        Task<bool> SetRequiredSettingsBeforePinAsync();

        /// <summary>
        /// Sets settings required for correct operation after PIN is entered.
        /// </summary>
        /// <returns></returns>
        Task<bool> SetRequiredSettingsAfterPinAsync();
    }
}