﻿using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.PDU;
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
        protected enum CurrentSmsTextFormat
        {
            PDU = 0,
            Text = 1,
            Unknown
        }

        protected readonly IAtChannel channel;
        private bool disposed;
        protected CurrentSmsTextFormat currentSmsTextFormat = CurrentSmsTextFormat.Unknown;

        public ModemBase(IAtChannel channel)
        {
            this.channel = channel;
            channel.UnsolicitedEvent += Channel_UnsolicitedEvent;
        }

        private void Channel_UnsolicitedEvent(object sender, UnsolicitedEventArgs e)
        {
            if (e.Line1 == "RING")
                IncomingCall?.Invoke(this, new IncomingCallEventArgs());
            else if (e.Line1.StartsWith("VOICE CALL: BEGIN"))
                CallStarted?.Invoke(this, new CallStartedEventArgs());
            else if (e.Line1.StartsWith("VOICE CALL: END"))
                CallEnded?.Invoke(this, CallEndedEventArgs.CreateFromResponse(e.Line1));
            else if (e.Line1.StartsWith("MISSED_CALL: "))
                MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(e.Line1));
            else if (e.Line1.StartsWith("+CMTI: "))
                SmsReceived?.Invoke(this, SmsReceivedEventArgs.CreateFromResponse(e.Line1));
            else if (e.Line1.StartsWith("+CUSD: "))
                UssdResponseReceived?.Invoke(this, UssdResponseEventArgs.CreateFromResponse(e.Line1));
            else if (AtErrorParsers.TryGetError(e.Line1, out Error error))
                ErrorReceived?.Invoke(this, new ErrorEventArgs(error.ToString()));
            else
                GenericEvent?.Invoke(this, new GenericEventArgs(e.Line1));
        }

        public event EventHandler<ErrorEventArgs> ErrorReceived;
        public event EventHandler<GenericEventArgs> GenericEvent;

        #region _V_25TER
        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;
        public event EventHandler<CallStartedEventArgs> CallStarted;
        public event EventHandler<CallEndedEventArgs> CallEnded;

        public virtual async Task<bool> SetRequiredSettingsBeforePinAsync()
        {
            ModemResponse echo = await DisableEchoAsync();
            ModemResponse errorFormat = await SetErrorFormat(1);
            return echo.Success && errorFormat.Success;
        }

        public virtual async Task<bool> SetRequiredSettingsAfterPinAsync()
        {
            ModemResponse detailedTextModeResultCodes = await ShowSmsTextModeParameters(true);
            return detailedTextModeResultCodes.Success;
        }

        public virtual async Task<ModemResponse<Imsi>> GetImsiAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CIMI", string.Empty);

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"(?<imsi>\d+)");
                if (match.Success)
                {
                    string imsi = match.Groups["imsi"].Value;
                    return ModemResponse.IsResultSuccess(new Imsi(imsi));
                }
            }
            return ModemResponse.HasResultError<Imsi>();
        }

        public virtual async Task<ModemResponse> AnswerIncomingCallAsync()
        {
            AtResponse response = await channel.SendCommand("ATA");
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse> DialAsync(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false)
        {
            string command = $"ATD{phoneNumber}{(hideCallerNumber ? 'I' : 'i')}{(closedUserGroup ? 'G' : 'g')};";
            AtResponse response = await channel.SendCommand(command);
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse> DisableEchoAsync()
        {
            AtResponse response = await channel.SendCommand("ATE0");
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse<ProductIdentificationInformation>> GetProductIdentificationInformationAsync()
        {
            AtResponse response = await channel.SendMultilineCommand("ATI", null);

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
            AtResponse response = await channel.SendCommand($"AT+CHUP");
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse<IEnumerable<string>>> GetAvailableCharacterSetsAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CSCS=?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"\+CSCS:\s\((?:""(?<characterSet>\w+)"",*)+\)");
                if (match.Success)
                {
                    return ModemResponse.IsResultSuccess(match.Groups["characterSet"].Captures.Select(x => x.Value));
                }
            }
            return ModemResponse.HasResultError<IEnumerable<string>>();
        }

        public virtual async Task<ModemResponse<string>> GetCurrentCharacterSetAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CSCS?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"""(?<characterSet>\w)""");
                if (match.Success)
                {
                    string characterSet = match.Groups["characterSet"].Value;
                    return ModemResponse.IsResultSuccess(characterSet);
                }
            }
            return ModemResponse.HasResultError<string>();
        }

        public virtual async Task<ModemResponse> SetCharacterSetAsync(string characterSet)
        {
            AtResponse response = await channel.SendCommand($"AT+CSCS=\"{characterSet}\"");
            return ModemResponse.IsSuccess(response.Success);
        }
        #endregion

        #region _3GPP_TS_27_005
        public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        public virtual async Task<ModemResponse> SetSmsMessageFormatAsync(SmsTextFormat format)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGF={(int)format}");

            if (response.Success)
                currentSmsTextFormat = (CurrentSmsTextFormat)format;

            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse> SetNewSmsIndication(int mode, int mt, int bm, int ds, int bfr)
        {
            if (mode < 0 || mode > 2)
                throw new ArgumentOutOfRangeException(nameof(mode));
            if (mt < 0 || mt > 3)
                throw new ArgumentOutOfRangeException(nameof(mt));
            if (!(bm == 0 || bm == 2))
                throw new ArgumentOutOfRangeException(nameof(bm));
            if (ds < 0 || ds > 2)
                throw new ArgumentOutOfRangeException(nameof(ds));
            if (bfr < 0 || bfr > 1)
                throw new ArgumentOutOfRangeException(nameof(bfr));

            AtResponse response = await channel.SendCommand($"AT+CNMI={mode},{mt},{bm},{ds},{bfr}");
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse<SmsReference>> SendSmsInTextFormatAsync(PhoneNumber phoneNumber, string message)
        {
            if (phoneNumber is null)
                throw new ArgumentNullException(nameof(phoneNumber));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            string cmd1 = $"AT+CMGS=\"{phoneNumber}\"";
            string cmd2 = message;
            AtResponse response = await channel.SendSmsAsync(cmd1, cmd2, "+CMGS:");

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
                if (match.Success)
                {
                    int mr = int.Parse(match.Groups["mr"].Value);
                    return ModemResponse.IsResultSuccess(new SmsReference(mr));
                }
            }
            return ModemResponse.HasResultError<SmsReference>();
        }

        public abstract Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message);

        protected virtual async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, bool includeEmptySmscLength)
        {
            if (phoneNumber is null)
                throw new ArgumentNullException(nameof(phoneNumber));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            IEnumerable<string> pdus = SmsSubmitEncoder.Encode(new SmsSubmitRequest(phoneNumber, message) { IncludeEmptySmscLength = includeEmptySmscLength });
            List<ModemResponse<SmsReference>> references = new List<ModemResponse<SmsReference>>();
            foreach (string pdu in pdus)
            {
                string cmd1 = $"AT+CMGS={(pdu.Length) / 2}";
                string cmd2 = pdu;
                AtResponse response = await channel.SendSmsAsync(cmd1, cmd2, "+CMGS:", TimeSpan.FromSeconds(30));

                if (response.Success)
                {
                    string line = response.Intermediates.First();
                    var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
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

        public abstract Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme);

        protected virtual async Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme, bool includeEmptySmscLength)
        {
            if (phoneNumber is null)
                throw new ArgumentNullException(nameof(phoneNumber));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            IEnumerable<string> pdus = SmsSubmitEncoder.Encode(new SmsSubmitRequest(phoneNumber, message, codingScheme) { IncludeEmptySmscLength = includeEmptySmscLength });
            List<ModemResponse<SmsReference>> references = new List<ModemResponse<SmsReference>>();
            foreach (string pdu in pdus)
            {
                string cmd1 = $"AT+CMGS={(pdu.Length) / 2}";
                string cmd2 = pdu;
                AtResponse response = await channel.SendSmsAsync(cmd1, cmd2, "+CMGS:", TimeSpan.FromSeconds(30));

                if (response.Success)
                {
                    string line = response.Intermediates.First();
                    var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
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
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CPMS=?", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CPMS:\s\((?<s1Storages>(""\w+"",?)+)\),\((?<s2Storages>(""\w+"",?)+)\),\((?<s3Storages>(""\w+"",?)+)\)");
                if (match.Success)
                {
                    IEnumerable<string> s1Storages = match.Groups["s1Storages"].Value.Split(',').Select(x => x.Trim('"'));
                    IEnumerable<string> s2Storages = match.Groups["s2Storages"].Value.Split(',').Select(x => x.Trim('"'));
                    IEnumerable<string> s3Storages = match.Groups["s3Storages"].Value.Split(',').Select(x => x.Trim('"'));

                    return ModemResponse.IsResultSuccess(new SupportedPreferredMessageStorages(s1Storages, s2Storages, s3Storages));
                }
            }
            if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                return ModemResponse.HasResultError<SupportedPreferredMessageStorages>(error);
            return ModemResponse.HasResultError<SupportedPreferredMessageStorages>();
        }

        public virtual async Task<ModemResponse<PreferredMessageStorages>> GetPreferredMessageStoragesAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CPMS?", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CPMS:\s(?<storage1>""\w+"",\d+,\d+),(?<storage2>""\w+"",\d+,\d+),(?<storage3>""\w+"",\d+,\d+)");
                if (match.Success)
                {
                    string[] s1Split = match.Groups["storage1"].Value.Split(',');
                    string[] s2Split = match.Groups["storage2"].Value.Split(',');
                    string[] s3Split = match.Groups["storage3"].Value.Split(',');

                    return ModemResponse.IsResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage((MessageStorage)s1Split[0].Trim('"'), int.Parse(s1Split[1]), int.Parse(s1Split[2])),
                        new PreferredMessageStorage((MessageStorage)s2Split[0].Trim('"'), int.Parse(s2Split[1]), int.Parse(s2Split[2])),
                        new PreferredMessageStorage((MessageStorage)s3Split[0].Trim('"'), int.Parse(s3Split[1]), int.Parse(s3Split[2]))));
                }
            }
            if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                return ModemResponse.HasResultError<PreferredMessageStorages>(error);
            return ModemResponse.HasResultError<PreferredMessageStorages>();
        }

        public virtual async Task<ModemResponse<PreferredMessageStorages>> SetPreferredMessageStorageAsync(MessageStorage storage1Name, MessageStorage storage2Name, MessageStorage storage3Name)
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CPMS=\"{storage1Name}\",\"{storage2Name}\",\"{storage3Name}\"", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CPMS:\s(?<s1Used>\d+),(?<s1Total>\d+),(?<s2Used>\d+),(?<s2Total>\d+),(?<s3Used>\d+),(?<s3Total>\d+)");
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
            
            if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                        return ModemResponse.HasResultError<PreferredMessageStorages>(error);
            return ModemResponse.HasResultError<PreferredMessageStorages>();
        }

        public virtual async Task<ModemResponse<Sms>> ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                case SmsTextFormat.PDU:
                    AtResponse pduResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

                    if (pduResponse.Success)
                    {
                        string line1 = pduResponse.Intermediates[0];
                        var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d),(""(?<alpha>\w*)"")*,(?<length>\d+)");
                        string line2 = pduResponse.Intermediates[1];
                        var line2Match = Regex.Match(line2, @"(?<status>[0-9A-Z]*)");
                        if (line1Match.Success && line2Match.Success)
                        {
                            int statusCode = int.Parse(line1Match.Groups["status"].Value);
                            SmsStatus status = (SmsStatus)statusCode;

                            string pdu = line2Match.Groups["status"].Value;
                            SmsDeliver pduMessage = SmsDeliverDecoder.Decode(pdu.ToByteArray());

                            return ModemResponse.IsResultSuccess(new Sms(status, pduMessage.SenderNumber, pduMessage.Timestamp, pduMessage.Message));
                        }
                        if (AtErrorParsers.TryGetError(pduResponse.FinalResponse, out Error pduError))
                            return ModemResponse.HasResultError<Sms>(pduError);
                    }
                    break;
                case SmsTextFormat.Text:
                    AtResponse textResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

                    if (textResponse.Success && textResponse.Intermediates.Count > 0)
                    {
                        string line = textResponse.Intermediates.First();
                        var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+\d+)"",("""")?,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))"",(?<addressType>\d+),(?<tpduFirstOctet>\d),(?<pid>\d),(?<dcs>\d),(?<serviceCenterAddress>""\+\d+""),(?<serviceCenterAddressType>\d+),(?<length>\d+)");
                        if (match.Success)
                        {
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                            PhoneNumberDTO sender = new PhoneNumberDTO(match.Groups["sender"].Value);
                            int year = int.Parse(match.Groups["year"].Value);
                            int month = int.Parse(match.Groups["month"].Value);
                            int day = int.Parse(match.Groups["day"].Value);
                            int hour = int.Parse(match.Groups["hour"].Value);
                            int minute = int.Parse(match.Groups["minute"].Value);
                            int second = int.Parse(match.Groups["second"].Value);
                            int zone = int.Parse(match.Groups["zone"].Value);

                            int addressType = int.Parse(match.Groups["addressType"].Value);
                            int tpduFirstOctet = int.Parse(match.Groups["tpduFirstOctet"].Value);
                            int protocolIdentifier = int.Parse(match.Groups["pid"].Value);
                            int dataCodingScheme = int.Parse(match.Groups["dcs"].Value);
                            string serviceCenterAddress = match.Groups["serviceCenterAddress"].Value;
                            int serviceCenterAddressType = int.Parse(match.Groups["serviceCenterAddressType"].Value);
                            int length = int.Parse(match.Groups["length"].Value);

                            DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
                            string message = textResponse.Intermediates.Last();

                            CodingScheme dcs = (CodingScheme)dataCodingScheme;
                            if (dcs == CodingScheme.UCS2)
                                message = UCS2.Decode(message);

                            return ModemResponse.IsResultSuccess(new Sms(status, sender, received, message));
                        }
                    }
                    if (AtErrorParsers.TryGetError(textResponse.FinalResponse, out Error textError))
                        return ModemResponse.HasResultError<Sms>(textError);
                    break;
                default:
                    throw new NotSupportedException("The format is not supported");
            }
            return ModemResponse.HasResultError<Sms>();
        }

        public virtual async Task<ModemResponse<List<SmsWithIndex>>> ListSmssAsync(SmsStatus smsStatus)
        {
            string command = currentSmsTextFormat switch
            {
                CurrentSmsTextFormat.Text => $"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\"",
                CurrentSmsTextFormat.PDU => $"AT+CMGL={(int)smsStatus}",
                _ => throw new Exception("Unknown SMS Text Format")
            };

            AtResponse response = await channel.SendMultilineCommand(command, null);

            List<SmsWithIndex> smss = new List<SmsWithIndex>();
            if (response.Success)
            {
                switch (currentSmsTextFormat)
                {
                    case CurrentSmsTextFormat.PDU:
                        if ((response.Intermediates.Count % 2) != 0)
                            return ModemResponse.HasResultError<List<SmsWithIndex>>();

                        for (int i = 0; i < response.Intermediates.Count; i += 2)
                        {
                            string metaDataLine = response.Intermediates[i];
                            string messageLine = response.Intermediates[i + 1];
                            var match = Regex.Match(metaDataLine, @"\+CMGL:\s(?<index>\d+),(?<status>\d+),,(?<length>\d+)");
                            if (match.Success)
                            {
                                int index = int.Parse(match.Groups["index"].Value);
                                SmsStatus status = (SmsStatus)int.Parse(match.Groups["status"].Value);

                                // Sent when AT+CSDH=1 is set
                                int length = int.Parse(match.Groups["length"].Value);

                                SmsDeliver sms = SmsDeliverDecoder.Decode(messageLine.ToByteArray());
                                smss.Add(new SmsWithIndex(index, status, sms.SenderNumber, sms.Timestamp, sms.Message));
                            }
                        }
                        break;
                    case CurrentSmsTextFormat.Text:
                        if ((response.Intermediates.Count % 2) != 0)
                            return ModemResponse.HasResultError<List<SmsWithIndex>>();

                        for (int i = 0; i < response.Intermediates.Count; i += 2)
                        {
                            string metaDataLine = response.Intermediates[i];
                            string messageLine = response.Intermediates[i + 1];
                            var match = Regex.Match(metaDataLine, @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+*\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))"",(?<addressType>\d+),(?<length>\d+)");
                            if (match.Success)
                            {
                                int index = int.Parse(match.Groups["index"].Value);
                                SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                                PhoneNumberDTO sender = new PhoneNumberDTO(match.Groups["sender"].Value);
                                int year = int.Parse(match.Groups["year"].Value);
                                int month = int.Parse(match.Groups["month"].Value);
                                int day = int.Parse(match.Groups["day"].Value);
                                int hour = int.Parse(match.Groups["hour"].Value);
                                int minute = int.Parse(match.Groups["minute"].Value);
                                int second = int.Parse(match.Groups["second"].Value);
                                int zone = int.Parse(match.Groups["zone"].Value);

                                // Sent when AT+CSDH=1 is set
                                int addressType = int.Parse(match.Groups["addressType"].Value);
                                int dataLength = int.Parse(match.Groups["length"].Value);

                                DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));

                                string message = messageLine;
                                if (messageLine.Length != dataLength)
                                    message = UCS2.Decode(messageLine);

                                smss.Add(new SmsWithIndex(index, status, sender, received, message));
                            }
                        }
                        break;
                    case CurrentSmsTextFormat.Unknown:
                        break;
                    default:
                        break;
                }
            }
            return ModemResponse.IsResultSuccess(smss);
        }

        public virtual async Task<ModemResponse> DeleteSmsAsync(int index)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGD={index}");
            return ModemResponse.IsSuccess(response.Success);
        }
        #endregion

        #region _3GPP_TS_27_007
        public event EventHandler<UssdResponseEventArgs> UssdResponseReceived;

        public virtual async Task<ModemResponse<SimStatus>> GetSimStatusAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CPIN?", "+CPIN:", TimeSpan.FromSeconds(10));

            if (!response.Success)
            {
                if (AtErrorParsers.TryGetError(response.FinalResponse, out Error cmeError))
                    return ModemResponse.HasResultError<SimStatus>(cmeError);
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.First();
            var match = Regex.Match(cpinLine, @"\+CPIN:\s(?<pinresult>.*)");
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

            return ModemResponse.HasResultError<SimStatus>();
        }

        public virtual async Task<ModemResponse> EnterSimPinAsync(PersonalIdentificationNumber pin)
        {
            AtResponse response = await channel.SendCommand($"AT+CPIN={pin}");
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse<SignalStrength>> GetSignalStrengthAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CSQ", "+CSQ:");

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CSQ:\s(?<rssi>\d+),(?<ber>\d+)");
                if (match.Success)
                {
                    int rssi = int.Parse(match.Groups["rssi"].Value);
                    int ber = int.Parse(match.Groups["ber"].Value);
                    return ModemResponse.IsResultSuccess(new SignalStrength(PowerRatio.FromDecibelMilliwatts(rssi), Ratio.FromPercent(ber)));
                }
            }
            return ModemResponse.HasResultError<SignalStrength>();
        }

        public virtual async Task<ModemResponse<BatteryStatus>> GetBatteryStatusAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CBC", "+CBC:");

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CBC:\s(?<bcs>\d+),(?<bcl>\d+)");
                if (match.Success)
                {
                    int bcs = int.Parse(match.Groups["bcs"].Value);
                    int bcl = int.Parse(match.Groups["bcl"].Value);
                    return ModemResponse.IsResultSuccess(new BatteryStatus((BatteryChargeStatus)bcs, Ratio.FromPercent(bcl)));
                }
            }
            return ModemResponse.HasResultError<BatteryStatus>();
        }

        public virtual async Task<ModemResponse> SetDateTimeAsync(DateTimeOffset value)
        {
            var sb = new StringBuilder("AT+CCLK=\"");
            int offsetQuarters = value.Offset.Hours * 4;
            sb.Append(value.ToString(@"yy/MM/dd,HH:mm:ss", CultureInfo.InvariantCulture));
            sb.Append(offsetQuarters.ToString("+00;-#", CultureInfo.InvariantCulture));
            sb.Append("\"");
            AtResponse response = await channel.SendCommand(sb.ToString());
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse<DateTimeOffset>> GetDateTimeAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CCLK?", "+CCLK:");

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CCLK:\s""(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d)?""");
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
            return ModemResponse.HasResultError<DateTimeOffset>();
        }

        public virtual async Task<ModemResponse> SendUssdAsync(string code, int codingScheme = 15)
        {
            AtResponse response = await channel.SendCommand($"AT+CUSD=1,\"{code}\",{codingScheme}");
            return ModemResponse.IsSuccess(response.Success);
        }
        #endregion

        public virtual async Task<ModemResponse> SetErrorFormat(int errorFormat)
        {
            AtResponse response = await channel.SendCommand($"AT+CMEE={errorFormat}");
            return ModemResponse.IsSuccess(response.Success);
        }

        public virtual async Task<ModemResponse> ShowSmsTextModeParameters(bool activate)
        {

            AtResponse response = await channel.SendCommand($"AT+CSDH={(activate ? "1" : "0")}");
            return ModemResponse.IsSuccess(response.Success);
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
