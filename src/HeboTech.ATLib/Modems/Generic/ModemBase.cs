﻿using HeboTech.ATLib.Events;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Messaging;
using HeboTech.ATLib.Numbering;
using HeboTech.ATLib.Parsing;
using HeboTech.ATLib.Storage;
using HeboTech.ATLib.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnitsNet;

namespace HeboTech.ATLib.Modems.Generic
{
    public abstract class ModemBase : IDisposable
    {
        protected readonly IAtChannel channel;
        private bool disposed;

        public ModemBase(IAtChannel channel)
        {
            this.channel = channel;
            channel.UnsolicitedEvent += Channel_UnsolicitedEvent;
        }

        protected virtual void Channel_UnsolicitedEvent(object sender, UnsolicitedEventArgs e)
        {
            if (e.Line1 == "RING")
                IncomingCall?.Invoke(this, new IncomingCallEventArgs());
            else if (e.Line1.StartsWith("VOICE CALL: BEGIN"))
                CallStarted?.Invoke(this, new CallStartedEventArgs());
            else if (e.Line1.StartsWith("VOICE CALL: END"))
                CallEnded?.Invoke(this, CallEndedEventArgs.CreateFromResponse(e.Line1));
            else if (e.Line1.StartsWith("MISSED_CALL: "))
                MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(e.Line1));

            else if (e.Line1.StartsWith("+CMT: "))
                SmsReceived?.Invoke(this, SmsReceivedEventArgs.CreateFromResponse(e.Line1, e.Line2));
            else if (e.Line1.StartsWith("+CMTI: "))
                SmsStorageReferenceReceived?.Invoke(this, SmsStorageReferenceReceivedEventArgs.CreateFromResponse(e.Line1));

            //else if (e.Line1.StartsWith("+CBM: "))
            //    BroadcastMessageReceived?.Invoke(this, BroadcastMessageReceivedEventArgs.CreateFromResponse(e.Line1, e.Line2));
            //else if (e.Line1.StartsWith("+CBMI: "))
            //    BroadcastMessageStorageReferenceReceived?.Invoke(this, BroadcastMessageStorageReferenceReceivedEventArgs.CreateFromResponse(e.Line1));

            else if (e.Line1.StartsWith("+CDS: "))
                SmsStatusReportReceived?.Invoke(this, SmsStatusReportEventArgs.CreateFromResponse(e.Line1, e.Line2));
            else if (e.Line1.StartsWith("+CDSI: "))
                SmsStatusReportStorageReferenceReceived?.Invoke(this, SmsStatusReportStorageReferenceEventArgs.CreateFromResponse(e.Line1));

            else if (e.Line1.StartsWith("+CUSD: "))
                UssdResponseReceived?.Invoke(this, UssdResponseEventArgs.CreateFromResponse(e.Line1));

            else if (AtErrorParsers.TryGetError(e.Line1, out Error error))
                ErrorReceived?.Invoke(this, new ErrorEventArgs(error.ToString()));

            else
                GenericEvent?.Invoke(this, new GenericEventArgs(e.Line1));
        }

        public event EventHandler<ErrorEventArgs> ErrorReceived;
        public event EventHandler<GenericEventArgs> GenericEvent;

        public event EventHandler<SmsReceivedEventArgs> SmsReceived;
        public event EventHandler<SmsStorageReferenceReceivedEventArgs> SmsStorageReferenceReceived;

        //public event EventHandler<BroadcastMessageReceivedEventArgs> BroadcastMessageReceived;
        //public event EventHandler<BroadcastMessageStorageReferenceReceivedEventArgs> BroadcastMessageStorageReferenceReceived;

        public event EventHandler<SmsStatusReportEventArgs> SmsStatusReportReceived;
        public event EventHandler<SmsStatusReportStorageReferenceEventArgs> SmsStatusReportStorageReferenceReceived;

        #region _V_25TER
        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;
        public event EventHandler<CallStartedEventArgs> CallStarted;
        public event EventHandler<CallEndedEventArgs> CallEnded;

        public virtual async Task<bool> SetRequiredSettingsBeforePinAsync()
        {
            ModemResponse echo = await DisableEchoAsync().ConfigureAwait(false);
            ModemResponse errorFormat = await SetErrorFormatAsync(1).ConfigureAwait(false);
            ModemResponse currentCharacterSet = await SetCharacterSetAsync(CharacterSet.UCS2).ConfigureAwait(false);
            ModemResponse smsMessageFormat = await SetSmsMessageFormatAsync(SmsTextFormat.PDU).ConfigureAwait(false);
            return echo.Success && errorFormat.Success && currentCharacterSet.Success && smsMessageFormat.Success;
        }

        public virtual Task<bool> SetRequiredSettingsAfterPinAsync()
        {
            return Task.FromResult(true);
        }

        public virtual async Task<ModemResponse<Imsi>> GetImsiAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CIMI", string.Empty).ConfigureAwait(false);

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"(?<imsi>\d+)", RegexOptions.Compiled);
                if (match.Success)
                {
                    string imsi = match.Groups["imsi"].Value;
                    return ModemResponse.IsResultSuccess(new Imsi(imsi));
                }
            }
            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<Imsi>(error);
        }

        public virtual async Task<ModemResponse> AnswerIncomingCallAsync()
        {
            AtResponse response = await channel.SendCommand("ATA").ConfigureAwait(false);
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse> DialAsync(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false)
        {
            string command = $"ATD{phoneNumber}{(hideCallerNumber ? 'I' : 'i')}{(closedUserGroup ? 'G' : 'g')};";
            AtResponse response = await channel.SendCommand(command, TimeSpan.FromSeconds(45)).ConfigureAwait(false);
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse> DisableEchoAsync()
        {
            AtResponse response = await channel.SendCommand("ATE0").ConfigureAwait(false);
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse<ProductIdentificationInformation>> GetProductIdentificationInformationAsync()
        {
            AtResponse response = await channel.SendMultilineCommand("ATI", null).ConfigureAwait(false);

            if (response.Success)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string line in response.Intermediates)
                {
                    builder.AppendLine(line);
                }

                return ModemResponse.IsResultSuccess(new ProductIdentificationInformation(builder.ToString()));
            }
            return ModemResponse.HasResultError<ProductIdentificationInformation>();
        }

        public virtual async Task<ModemResponse> HangupAsync()
        {
            AtResponse response = await channel.SendCommand($"AT+CHUP").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse<IEnumerable<string>>> GetAvailableCharacterSetsAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CSCS=?", "+CSCS:").ConfigureAwait(false);

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"\+CSCS:\s\((?:""(?<characterSet>\w+)"",*)+\)", RegexOptions.Compiled);
                if (match.Success)
                {
                    return ModemResponse.IsResultSuccess(match.Groups["characterSet"].Captures.Select(x => x.Value));
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<IEnumerable<string>>(error);
        }

        public virtual async Task<ModemResponse<CharacterSet>> GetCurrentCharacterSetAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CSCS?", "+CSCS:").ConfigureAwait(false);

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"\+CSCS: ""(?<characterSet>\w+)""", RegexOptions.Compiled);
                if (match.Success)
                {
                    string characterSetString = match.Groups["characterSet"].Value;
                    CharacterSet characterSet = CharacterSetHelpers.FromString(characterSetString);
                    return ModemResponse.IsResultSuccess(characterSet);
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<CharacterSet>(error);
        }

        public virtual async Task<ModemResponse> SetCharacterSetAsync(CharacterSet characterSet)
        {
            AtResponse response = await channel.SendCommand($"AT+CSCS=\"{CharacterSetHelpers.ToString(characterSet)}\"").ConfigureAwait(false);
            if (response.Success)
            {
                return ModemResponse.IsSuccess(response.Success);
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }
        #endregion

        #region _3GPP_TS_27_005
        public virtual async Task<ModemResponse<string>> GetSmsMessageFormatAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CMGF?", "+CMGF:").ConfigureAwait(false);

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"(?<mode>\d)", RegexOptions.Compiled);
                if (match.Success)
                {
                    int mode = int.Parse(match.Groups["mode"].Value);
                    return ModemResponse.IsResultSuccess(mode.ToString());
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<string>(error);
        }

        public virtual async Task<ModemResponse> SetSmsMessageFormatAsync(SmsTextFormat format)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGF={(int)format}").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess(response.Success);

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        //public virtual async Task<ModemResponse> SetSelectMessageService(int service)
        //{
        //    AtResponse response = await channel.SendCommand($"AT+CSMS={service}").ConfigureAwait(false);

        //    if (response.Success)
        //        return ModemResponse.IsSuccess();

        //    AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
        //    return ModemResponse.HasError(error);
        //}

        public virtual async Task<ModemResponse> SetNewSmsIndicationAsync(int mode, int mt, int bm, int ds, int bfr)
        {
            AtResponse response = await channel.SendCommand($"AT+CNMI={mode},{mt},{bm},{ds},{bfr}").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(SmsSubmitRequest request)
        {
            return SendSmsAsync(request, true);
        }

        protected async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(SmsSubmitRequest request, bool includeEmptySmscLength)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            IEnumerable<string> pdus = SmsSubmitEncoder.Encode(request, includeEmptySmscLength);
            List<ModemResponse<SmsReference>> references = new List<ModemResponse<SmsReference>>();
            foreach (string pdu in pdus)
            {
                string cmd1 = $"AT+CMGS={(pdu.Length) / 2}";
                string cmd2 = pdu;
                AtResponse response = await channel.SendSmsAsync(cmd1, cmd2, "+CMGS:", TimeSpan.FromSeconds(30)).ConfigureAwait(false);

                if (response.Success)
                {
                    string line = response.Intermediates.First();
                    var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)", RegexOptions.Compiled);
                    if (match.Success)
                    {
                        int mr = int.Parse(match.Groups["mr"].Value);
                        references.Add(ModemResponse.IsResultSuccess(new SmsReference(mr)));
                    }
                }
                else
                {
                    if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                        references.Add(ModemResponse.HasResultError<SmsReference>(error));
                }
            }
            return references;
        }

        public virtual async Task<ModemResponse<SupportedPreferredMessageStorages>> GetSupportedPreferredMessageStoragesAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CPMS=?", "+CPMS:").ConfigureAwait(false);

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CPMS:\s\((?<s1Storages>(""\w+"",?)+)\),\((?<s2Storages>(""\w+"",?)+)\),\((?<s3Storages>(""\w+"",?)+)\)", RegexOptions.Compiled);
                if (match.Success)
                {
                    IEnumerable<string> s1Storages = match.Groups["s1Storages"].Value.Split(',').Select(x => x.Trim('"'));
                    IEnumerable<string> s2Storages = match.Groups["s2Storages"].Value.Split(',').Select(x => x.Trim('"'));
                    IEnumerable<string> s3Storages = match.Groups["s3Storages"].Value.Split(',').Select(x => x.Trim('"'));

                    return ModemResponse.IsResultSuccess(new SupportedPreferredMessageStorages(s1Storages, s2Storages, s3Storages));
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<SupportedPreferredMessageStorages>(error);
        }

        public virtual async Task<ModemResponse<PreferredMessageStorages>> GetPreferredMessageStoragesAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CPMS?", "+CPMS:").ConfigureAwait(false);

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CPMS:\s(?<storage1>""\w+"",\d+,\d+),(?<storage2>""\w+"",\d+,\d+),(?<storage3>""\w+"",\d+,\d+)", RegexOptions.Compiled);
                if (match.Success)
                {
                    string[] s1Split = match.Groups["storage1"].Value.Split(',');
                    string[] s2Split = match.Groups["storage2"].Value.Split(',');
                    string[] s3Split = match.Groups["storage3"].Value.Split(',');

                    return ModemResponse.IsResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(MessageStorage.Parse(s1Split[0].Trim('"')), int.Parse(s1Split[1]), int.Parse(s1Split[2])),
                        new PreferredMessageStorage(MessageStorage.Parse(s2Split[0].Trim('"')), int.Parse(s2Split[1]), int.Parse(s2Split[2])),
                        new PreferredMessageStorage(MessageStorage.Parse(s3Split[0].Trim('"')), int.Parse(s3Split[1]), int.Parse(s3Split[2]))));
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<PreferredMessageStorages>(error);
        }

        public virtual async Task<ModemResponse<PreferredMessageStorages>> SetPreferredMessageStorageAsync(MessageStorage storage1Name, MessageStorage storage2Name, MessageStorage storage3Name)
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CPMS=\"{storage1Name}\",\"{storage2Name}\",\"{storage3Name}\"", "+CPMS:").ConfigureAwait(false);

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CPMS:\s(?<s1Used>\d+),(?<s1Total>\d+),(?<s2Used>\d+),(?<s2Total>\d+),(?<s3Used>\d+),(?<s3Total>\d+)", RegexOptions.Compiled);
                if (match.Success)
                {
                    int s1Used = int.Parse(match.Groups["s1Used"].Value);
                    int s1Total = int.Parse(match.Groups["s1Total"].Value);
                    int s2Used = int.Parse(match.Groups["s2Used"].Value);
                    int s2Total = int.Parse(match.Groups["s2Total"].Value);
                    int s3Used = int.Parse(match.Groups["s3Used"].Value);
                    int s3Total = int.Parse(match.Groups["s3Total"].Value);

                    return ModemResponse.IsResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(storage1Name, s1Used, s1Total),
                        new PreferredMessageStorage(storage2Name, s2Used, s2Total),
                        new PreferredMessageStorage(storage3Name, s3Used, s3Total)));
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<PreferredMessageStorages>(error);
        }

        public virtual async Task<ModemResponse<Sms>> ReadSmsAsync(int index)
        {
            AtResponse pduResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null).ConfigureAwait(false);

            if (pduResponse.Success)
            {
                if (!pduResponse.Intermediates.Any())
                    return ModemResponse.HasResultError<Sms>();

                string line1 = pduResponse.Intermediates[0];
                var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d),(""(?<alpha>\w*)"")*,(?<length>\d+)", RegexOptions.Compiled);
                if (line1Match.Success)
                {
                    int statusCode = int.Parse(line1Match.Groups["status"].Value);
                    int length = int.Parse(line1Match.Groups["length"].Value);
                    SmsStatus status = (SmsStatus)statusCode;

                    if (length > 0)
                    {
                        string line2 = pduResponse.Intermediates[1];
                        var line2Match = Regex.Match(line2, @"(?<pdu>[0-9A-Z]*)", RegexOptions.Compiled);
                        if (line2Match.Success)
                        {
                            string pduString = line2Match.Groups["pdu"].Value;
                            Sms sms = SmsDecoder.Decode(pduString.ToByteArray());
                            return ModemResponse.IsResultSuccess(sms);
                        }
                    }
                }
            }

            AtErrorParsers.TryGetError(pduResponse.FinalResponse, out Error pduError);
            return ModemResponse.HasResultError<Sms>(pduError);
        }

        public virtual async Task<ModemResponse<List<SmsWithIndex>>> ListSmssAsync(SmsStatus smsStatus)
        {
            string command = $"AT+CMGL={(int)smsStatus}";

            AtResponse response = await channel.SendMultilineCommand(command, null).ConfigureAwait(false);

            List<SmsWithIndex> smss = new List<SmsWithIndex>();
            if (response.Success)
            {
                if ((response.Intermediates.Count % 2) != 0)
                    return ModemResponse.HasResultError<List<SmsWithIndex>>();

                for (int i = 0; i < response.Intermediates.Count; i += 2)
                {
                    string metaDataLine = response.Intermediates[i];
                    string messageLine = response.Intermediates[i + 1];
                    var match = Regex.Match(metaDataLine, @"\+CMGL:\s(?<index>\d+),(?<status>\d+),,(?<length>\d+)", RegexOptions.Compiled);
                    if (match.Success)
                    {
                        int index = int.Parse(match.Groups["index"].Value);
                        SmsStatus status = (SmsStatus)int.Parse(match.Groups["status"].Value);

                        // Sent when AT+CSDH=1 is set
                        int length = int.Parse(match.Groups["length"].Value);

                        Sms sms = SmsDecoder.Decode(messageLine.ToByteArray());
                        smss.Add(new SmsWithIndex(sms, index));
                    }
                }
            }
            return ModemResponse.IsResultSuccess(smss);
        }

        public virtual async Task<ModemResponse> DeleteSmsAsync(int index)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGD={index}").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }
        #endregion

        #region _3GPP_TS_27_007
        public event EventHandler<UssdResponseEventArgs> UssdResponseReceived;

        public virtual async Task<ModemResponse<SimStatus>> GetSimStatusAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CPIN?", "+CPIN:", TimeSpan.FromSeconds(10)).ConfigureAwait(false);

            if (!response.Success)
            {
                if (AtErrorParsers.TryGetError(response.FinalResponse, out Error cmeError))
                    return ModemResponse.HasResultError<SimStatus>(cmeError);
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.First();
            var match = Regex.Match(cpinLine, @"\+CPIN:\s(?<pinresult>.*)", RegexOptions.Compiled);
            if (match.Success)
            {
                string cpinResult = match.Groups["pinresult"].Value;
                return cpinResult switch
                {
                    "SIM PIN" => ModemResponse.IsResultSuccess(SimStatus.SIM_PIN),
                    "SIM PUK" => ModemResponse.IsResultSuccess(SimStatus.SIM_PUK),
                    "PH-NET PIN" => ModemResponse.IsResultSuccess(SimStatus.SIM_NETWORK_PERSONALIZATION),
                    "READY" => ModemResponse.IsResultSuccess(SimStatus.SIM_READY),
                    _ => ModemResponse.IsResultSuccess(SimStatus.SIM_ABSENT),// Treat unsupported lock types as "sim absent"
                };
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<SimStatus>(error);
        }

        public virtual async Task<ModemResponse> EnterSimPinAsync(PersonalIdentificationNumber pin)
        {
            AtResponse response = await channel.SendCommand($"AT+CPIN={pin}").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<SimStatus>(error);
        }

        public virtual async Task<ModemResponse<SignalStrength>> GetSignalStrengthAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CSQ", "+CSQ:").ConfigureAwait(false);

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CSQ:\s(?<rssi>\d+),(?<ber>\d+)", RegexOptions.Compiled);
                if (match.Success)
                {
                    int rssi = int.Parse(match.Groups["rssi"].Value);
                    int ber = int.Parse(match.Groups["ber"].Value);
                    return ModemResponse.IsResultSuccess(new SignalStrength(PowerRatio.FromDecibelMilliwatts(rssi), Ratio.FromPercent(ber)));
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<SignalStrength>(error);
        }

        public virtual async Task<ModemResponse<BatteryStatus>> GetBatteryStatusAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CBC", "+CBC:").ConfigureAwait(false);

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CBC:\s(?<bcs>\d+),(?<bcl>\d+)", RegexOptions.Compiled);
                if (match.Success)
                {
                    int bcs = int.Parse(match.Groups["bcs"].Value);
                    int bcl = int.Parse(match.Groups["bcl"].Value);
                    return ModemResponse.IsResultSuccess(new BatteryStatus((BatteryChargeStatus)bcs, Ratio.FromPercent(bcl)));
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<BatteryStatus>(error);
        }

        public virtual async Task<ModemResponse> SetDateTimeAsync(DateTimeOffset value)
        {
            var sb = new StringBuilder("AT+CCLK=\"");
            int offsetQuarters = value.Offset.Hours * 4;
            sb.Append(value.ToString(@"yy/MM/dd,HH:mm:ss", CultureInfo.InvariantCulture));
            sb.Append(offsetQuarters.ToString("+00;-#", CultureInfo.InvariantCulture));
            sb.Append("\"");
            AtResponse response = await channel.SendCommand(sb.ToString()).ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse<DateTimeOffset>> GetDateTimeAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CCLK?", "+CCLK:").ConfigureAwait(false);

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CCLK:\s""(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d)?""", RegexOptions.Compiled);
                if (match.Success)
                {
                    DateTimeOffset time;

                    int year = int.Parse(match.Groups["year"].Value);
                    int month = int.Parse(match.Groups["month"].Value);
                    int day = int.Parse(match.Groups["day"].Value);
                    int hour = int.Parse(match.Groups["hour"].Value);
                    int minute = int.Parse(match.Groups["minute"].Value);
                    int second = int.Parse(match.Groups["second"].Value);
                    if (match.Groups["zone"].Success)
                    {
                        int zone = int.Parse(match.Groups["zone"].Value);
                        time = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
                    }
                    else
                        time = new DateTimeOffset(new DateTime(2000 + year, month, day, hour, minute, second));
                    return ModemResponse.IsResultSuccess(time);
                }
            }

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<DateTimeOffset>(error);
        }

        public virtual async Task<ModemResponse> SendUssdAsync(string code, int codingScheme = 15)
        {
            AtResponse response = await channel.SendCommand($"AT+CUSD=1,\"{code}\",{codingScheme}").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }
        #endregion

        public virtual async Task<ModemResponse> SetErrorFormatAsync(int errorFormat)
        {
            AtResponse response = await channel.SendCommand($"AT+CMEE={errorFormat}").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse> ShowSmsTextModeParametersAsync(bool activate)
        {
            AtResponse response = await channel.SendCommand($"AT+CSDH={(activate ? "1" : "0")}").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse> ResetToFactoryDefaultsAsync()
        {
            AtResponse response = await channel.SendCommand($"AT&F").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse> RawCommandAsync(string command)
        {
            AtResponse response = await channel.SendCommand(command).ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse<List<string>>> RawCommandWithResponseAsync(string command, string responsePrefix)
        {
            AtResponse response = await channel.SendSingleLineCommandAsync(command, responsePrefix).ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsResultSuccess(response.Intermediates);

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasResultError<List<string>>(error);
        }

        public virtual async Task<ModemResponse> RestoreUserSettingsAsync()
        {
            AtResponse response = await channel.SendCommand("ATZ0").ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse> SaveUserSettingsAsync()
        {
            AtResponse response = await channel.SendCommand("AT&W0").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public virtual async Task<ModemResponse> ResetUserSettingsAsync()
        {
            AtResponse response = await channel.SendCommand("AT&F0").ConfigureAwait(false);

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }

        public void Close()
        {
            Dispose();
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    channel.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ModemBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
