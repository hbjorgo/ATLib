using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
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
        protected readonly AtChannel channel;
        private bool disposed;

        public ModemBase(AtChannel channel)
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
                    return ModemResponse.ResultSuccess(new Imsi(imsi));
                }
            }
            return ModemResponse.ResultError<Imsi>();
        }

        public virtual async Task<ModemResponse> AnswerIncomingCallAsync()
        {
            AtResponse response = await channel.SendCommand("ATA");
            return ModemResponse.Success(response.Success);
        }

        public virtual async Task<ModemResponse> DialAsync(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false)
        {
            string command = $"ATD{phoneNumber}{(hideCallerNumber ? 'I' : 'i')}{(closedUserGroup ? 'G' : 'g')};";
            AtResponse response = await channel.SendCommand(command);
            return ModemResponse.Success(response.Success);
        }

        public virtual async Task<ModemResponse> DisableEchoAsync()
        {
            AtResponse response = await channel.SendCommand("ATE0");
            return ModemResponse.Success(response.Success);
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

                return ModemResponse.ResultSuccess(new ProductIdentificationInformation(builder.ToString()));
            }
            return ModemResponse.ResultError<ProductIdentificationInformation>();
        }

        public virtual async Task<ModemResponse> HangupAsync()
        {
            AtResponse response = await channel.SendCommand($"AT+CHUP");
            return ModemResponse.Success(response.Success);
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
#if NETSTANDARD2_0
                    return ModemResponse.ResultSuccess(match.Groups["characterSet"].Captures.Cast<Capture>().Select(x => x.Value));
#elif NETSTANDARD2_1_OR_GREATER
                    return ModemResponse.ResultSuccess(match.Groups["characterSet"].Captures.Select(x => x.Value));
#endif
                }
            }
            return ModemResponse.ResultError<IEnumerable<string>>();
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
                    return ModemResponse.ResultSuccess(characterSet);
                }
            }
            return ModemResponse.ResultError<string>();
        }

        public virtual async Task<ModemResponse> SetCharacterSetAsync(string characterSet)
        {
            AtResponse response = await channel.SendCommand($"AT+CSCS=\"{characterSet}\"");
            return ModemResponse.Success(response.Success);
        }
        #endregion

        #region _3GPP_TS_27_005
        public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        public virtual async Task<ModemResponse> SetSmsMessageFormatAsync(SmsTextFormat format)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGF={(int)format}");
            return ModemResponse.Success(response.Success);
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
            return ModemResponse.Success(response.Success);
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
                    return ModemResponse.ResultSuccess(new SmsReference(mr));
                }
            }
            return ModemResponse.ResultError<SmsReference>();
        }

        public abstract Task<ModemResponse<SmsReference>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, byte[] message, CodingScheme codingScheme);

        protected virtual async Task<ModemResponse<SmsReference>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, byte[] message, CodingScheme codingScheme, bool includeEmptySmscLength)
        {
            if (phoneNumber is null)
                throw new ArgumentNullException(nameof(phoneNumber));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            IEnumerable<string> pdus = Pdu.EncodeMultipartSmsSubmit(phoneNumber, message, codingScheme, includeEmptySmscLength);
            List<SmsReference> smsReferences = new List<SmsReference>();
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
                        smsReferences.Add(new SmsReference(mr));
                        //return ModemResponse.ResultSuccess(new SmsReference(mr));
                    }
                }
                else
                {
                    if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                        smsReferences.Add(null);
                    //return ModemResponse.ResultError<SmsReference>(error.ToString());
                }
            }
            return ModemResponse.ResultError<SmsReference>();
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

                    return ModemResponse.ResultSuccess(new SupportedPreferredMessageStorages(s1Storages, s2Storages, s3Storages));
                }
            }
            return ModemResponse.ResultError<SupportedPreferredMessageStorages>();
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

                    return ModemResponse.ResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(s1Split[0].Trim('"'), int.Parse(s1Split[1]), int.Parse(s1Split[2])),
                        new PreferredMessageStorage(s2Split[0].Trim('"'), int.Parse(s2Split[1]), int.Parse(s2Split[2])),
                        new PreferredMessageStorage(s3Split[0].Trim('"'), int.Parse(s3Split[1]), int.Parse(s3Split[2]))));
                }
            }
            return ModemResponse.ResultError<PreferredMessageStorages>();
        }

        public virtual async Task<ModemResponse<PreferredMessageStorages>> SetPreferredMessageStorageAsync(string storage1Name, string storage2Name, string storage3Name)
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

                    return ModemResponse.ResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(storage1Name, s1Used, s1Total),
                        new PreferredMessageStorage(storage2Name, s2Used, s2Total),
                        new PreferredMessageStorage(storage3Name, s3Used, s3Total)));
                }
            }
            return ModemResponse.ResultError<PreferredMessageStorages>();
        }

        public virtual async Task<ModemResponse<Sms>> ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                case SmsTextFormat.PDU:
                    AtResponse pduResponse = await channel.SendMultilineCommand($"AT+CMGR={index},0", null);

                    if (pduResponse.Success)
                    {
                        string line1 = pduResponse.Intermediates[0];
                        var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d),(""(?<alpha>\w*)"")*,(?<length>\d+)");
                        string line2 = pduResponse.Intermediates[1];
                        var line2Match = Regex.Match(line2, @"(?<status>[0-9A-Z]*)");
                        if (line1Match.Success && line2Match.Success)
                        {
                            int statusCode = int.Parse(line1Match.Groups["status"].Value);
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(statusCode);

                            string pdu = line2Match.Groups["status"].Value;
#if NETSTANDARD2_0
                            SmsDeliver pduMessage = Pdu.DecodeSmsDeliver(pdu.AsSpan());
#elif NETSTANDARD2_1_OR_GREATER
                            SmsDeliver pduMessage = Pdu.DecodeSmsDeliver(pdu);
#endif

                            return ModemResponse.ResultSuccess(new Sms(status, pduMessage.SenderNumber, pduMessage.Timestamp, pduMessage.Message));
                        }
                    }
                    break;
                case SmsTextFormat.Text:
                    AtResponse textResponse = await channel.SendMultilineCommand($"AT+CMGR={index},0", null);

                    if (textResponse.Success && textResponse.Intermediates.Count > 0)
                    {
                        string line = textResponse.Intermediates.First();
                        var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
                        if (match.Success)
                        {
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                            PhoneNumber sender = new PhoneNumber(match.Groups["sender"].Value);
                            int year = int.Parse(match.Groups["year"].Value);
                            int month = int.Parse(match.Groups["month"].Value);
                            int day = int.Parse(match.Groups["day"].Value);
                            int hour = int.Parse(match.Groups["hour"].Value);
                            int minute = int.Parse(match.Groups["minute"].Value);
                            int second = int.Parse(match.Groups["second"].Value);
                            int zone = int.Parse(match.Groups["zone"].Value);
                            DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
                            string message = textResponse.Intermediates.Last();
                            return ModemResponse.ResultSuccess(new Sms(status, sender, received, message));
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("The format is not supported");
            }
            return ModemResponse.ResultError<Sms>();
        }

        public virtual async Task<ModemResponse<List<SmsWithIndex>>> ListSmssAsync(SmsStatus smsStatus)
        {
            AtResponse response = await channel.SendMultilineCommand($"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\",0", null);

            List<SmsWithIndex> smss = new List<SmsWithIndex>();
            if (response.Success)
            {
                string metaRegEx = @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+*\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""";

                using (var enumerator = response.Intermediates.GetEnumerator())
                {
                    string line = null;
                    AdvanceIterator();
                    while (line != null)
                    {
                        var match = Regex.Match(line, metaRegEx);
                        if (match.Success)
                        {
                            int index = int.Parse(match.Groups["index"].Value);
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                            PhoneNumber sender = new PhoneNumber(match.Groups["sender"].Value);
                            int year = int.Parse(match.Groups["year"].Value);
                            int month = int.Parse(match.Groups["month"].Value);
                            int day = int.Parse(match.Groups["day"].Value);
                            int hour = int.Parse(match.Groups["hour"].Value);
                            int minute = int.Parse(match.Groups["minute"].Value);
                            int second = int.Parse(match.Groups["second"].Value);
                            int zone = int.Parse(match.Groups["zone"].Value);
                            DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));

                            StringBuilder messageBuilder = new StringBuilder();
                            AdvanceIterator();
                            while (line != null && !Regex.Match(line, metaRegEx).Success)
                            {
                                messageBuilder.AppendLine(line);
                                AdvanceIterator();
                            }
                            smss.Add(new SmsWithIndex(index, status, sender, received, messageBuilder.ToString()));
                        }
                    }

                    void AdvanceIterator()
                    {
                        line = enumerator.MoveNext() ? enumerator.Current : null;
                    }
                }
            }
            return ModemResponse.ResultSuccess(smss);
        }

        public virtual async Task<ModemResponse> DeleteSmsAsync(int index)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGD={index}");
            return ModemResponse.Success(response.Success);
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
                    return ModemResponse.ResultError<SimStatus>(cmeError.ToString());
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.First();
            var match = Regex.Match(cpinLine, @"\+CPIN:\s(?<pinresult>.*)");
            if (match.Success)
            {
                string cpinResult = match.Groups["pinresult"].Value;
#if NETSTANDARD2_0
                switch (cpinResult)
                {
                    case "SIM PIN": return ModemResponse.ResultSuccess(SimStatus.SIM_PIN);
                    case "SIM PUK": return ModemResponse.ResultSuccess(SimStatus.SIM_PUK);
                    case "PH-NET PIN": return ModemResponse.ResultSuccess(SimStatus.SIM_NETWORK_PERSONALIZATION);
                    case "READY": return ModemResponse.ResultSuccess(SimStatus.SIM_READY);
                    default: return ModemResponse.ResultSuccess(SimStatus.SIM_ABSENT);// Treat unsupported lock types as "sim absent"
                };
#elif NETSTANDARD2_1_OR_GREATER
                return cpinResult switch
                {
                    "SIM PIN" => ModemResponse.ResultSuccess(SimStatus.SIM_PIN),
                    "SIM PUK" => ModemResponse.ResultSuccess(SimStatus.SIM_PUK),
                    "PH-NET PIN" => ModemResponse.ResultSuccess(SimStatus.SIM_NETWORK_PERSONALIZATION),
                    "READY" => ModemResponse.ResultSuccess(SimStatus.SIM_READY),
                    _ => ModemResponse.ResultSuccess(SimStatus.SIM_ABSENT),// Treat unsupported lock types as "sim absent"
                };
#endif
            }

            return ModemResponse.ResultError<SimStatus>();
        }

        public virtual async Task<ModemResponse> EnterSimPinAsync(PersonalIdentificationNumber pin)
        {
            AtResponse response = await channel.SendCommand($"AT+CPIN={pin}");
            return ModemResponse.Success(response.Success);
        }

        public virtual async Task<ModemResponse> ReInitializeSimAsync()
        {
            AtResponse response = await channel.SendCommand($"AT+CRFSIM");
            return ModemResponse.Success(response.Success);
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
                    return ModemResponse.ResultSuccess(new SignalStrength(PowerRatio.FromDecibelMilliwatts(rssi), Ratio.FromPercent(ber)));
                }
            }
            return ModemResponse.ResultError<SignalStrength>();
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
                    return ModemResponse.ResultSuccess(new BatteryStatus((BatteryChargeStatus)bcs, Ratio.FromPercent(bcl)));
                }
            }
            return ModemResponse.ResultError<BatteryStatus>();
        }

        public virtual async Task<ModemResponse> SetDateTimeAsync(DateTimeOffset value)
        {
            var sb = new StringBuilder("AT+CCLK=\"");
            int offsetQuarters = value.Offset.Hours * 4;
            sb.Append(value.ToString(@"yy/MM/dd,HH:mm:ss", CultureInfo.InvariantCulture));
            sb.Append(offsetQuarters.ToString("+00;-#", CultureInfo.InvariantCulture));
            sb.Append("\"");
            AtResponse response = await channel.SendCommand(sb.ToString());
            return ModemResponse.Success(response.Success);
        }

        public virtual async Task<ModemResponse<DateTimeOffset>> GetDateTimeAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CCLK?", "+CCLK:");

            if (response.Success)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CCLK:\s""(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d)""");
                if (match.Success)
                {
                    int year = int.Parse(match.Groups["year"].Value);
                    int month = int.Parse(match.Groups["month"].Value);
                    int day = int.Parse(match.Groups["day"].Value);
                    int hour = int.Parse(match.Groups["hour"].Value);
                    int minute = int.Parse(match.Groups["minute"].Value);
                    int second = int.Parse(match.Groups["second"].Value);
                    int zone = int.Parse(match.Groups["zone"].Value);
                    DateTimeOffset time = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
                    return ModemResponse.ResultSuccess(time);
                }
            }
            return ModemResponse.ResultError<DateTimeOffset>();
        }

        public virtual async Task<ModemResponse> SendUssdAsync(string code, int codingScheme = 15)
        {
            AtResponse response = await channel.SendCommand($"AT+CUSD=1,\"{code}\",{codingScheme}");
            return ModemResponse.Success(response.Success);
        }
        #endregion

        public virtual async Task<ModemResponse> SetErrorFormat(int errorFormat)
        {
            AtResponse response = await channel.SendCommand($"AT+CMEE={errorFormat}");
            return ModemResponse.Success(response.Success);
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
