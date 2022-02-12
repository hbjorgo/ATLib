using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Exceptions;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
            else if (e.Line1.StartsWith("+CME ERROR:"))
                ErrorReceived?.Invoke(this, ErrorEventArgs.CreateFromCmeResponse(e.Line1));
            else if (e.Line1.StartsWith("+CMS ERROR:"))
                ErrorReceived?.Invoke(this, ErrorEventArgs.CreateFromCmsResponse(e.Line1));
        }

        public event EventHandler<ErrorEventArgs> ErrorReceived;

        #region _V_25TER
        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;
        public event EventHandler<CallStartedEventArgs> CallStarted;
        public event EventHandler<CallEndedEventArgs> CallEnded;

        public virtual async Task<Imsi> GetImsiAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CIMI", string.Empty);

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"(?<imsi>\d+)");
                if (match.Success)
                {
                    string imsi = match.Groups["imsi"].Value;
                    return new Imsi(imsi);
                }
            }
            return default;
        }

        public virtual async Task<CommandStatus> AnswerIncomingCallAsync()
        {
            AtResponse response = await channel.SendCommand("ATA");

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual async Task<CommandStatus> DialAsync(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false)
        {
            string command = $"ATD{phoneNumber}{(hideCallerNumber ? 'I' : 'i')}{(closedUserGroup ? 'G' : 'g')};";
            AtResponse response = await channel.SendCommand(command);

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual async Task<CommandStatus> DisableEchoAsync()
        {
            AtResponse response = await channel.SendCommand("ATE0");

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual async Task<ProductIdentificationInformation> GetProductIdentificationInformationAsync()
        {
            AtResponse response = await channel.SendMultilineCommand("ATI", null);

            if (response.Success)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string line in response.Intermediates)
                {
                    builder.AppendLine(line);
                }

                return new ProductIdentificationInformation(builder.ToString());
            }
            return default;
        }

        public virtual async Task<CommandStatus> HangupAsync()
        {
            AtResponse response = await channel.SendCommand($"AT+CHUP");

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual async Task<IEnumerable<string>> GetAvailableCharacterSetsAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CSCS=?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"\+CSCS:\s\((?:""(?<characterSet>\w+)"",*)+\)");
                if (match.Success)
                {
                    return match.Groups["characterSet"].Captures.Select(x => x.Value);
                }
            }
            return default;
        }

        public virtual async Task<string> GetCurrentCharacterSetAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync($"AT+CSCS?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.FirstOrDefault() ?? string.Empty;
                var match = Regex.Match(line, @"""(?<characterSet>\w)""");
                if (match.Success)
                {
                    string characterSet = match.Groups["characterSet"].Value;
                    return characterSet;
                }
            }
            return default;
        }

        public virtual async Task<CommandStatus> SetCharacterSetAsync(string characterSet)
        {
            AtResponse response = await channel.SendCommand($"AT+CSCS=\"{characterSet}\"");

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }
        #endregion

        #region _3GPP_TS_27_005
        public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        public virtual async Task<CommandStatus> SetSmsMessageFormatAsync(SmsTextFormat format)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGF={(int)format}");

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual async Task<CommandStatus> SetNewSmsIndication(int mode, int mt, int bm, int ds, int bfr)
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

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual async Task<SmsReference> SendSmsAsync(PhoneNumber phoneNumber, string message)
        {
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
                    return new SmsReference(mr);
                }
            }
            return default;
        }

        public virtual async Task<SupportedPreferredMessageStorages> GetSupportedPreferredMessageStoragesAsync()
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

                    return new SupportedPreferredMessageStorages(s1Storages, s2Storages, s3Storages);
                }
            }
            return default;
        }

        public virtual async Task<PreferredMessageStorages> GetPreferredMessageStoragesAsync()
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

                    return new PreferredMessageStorages(
                        new PreferredMessageStorage(s1Split[0].Trim('"'), int.Parse(s1Split[1]), int.Parse(s1Split[2])),
                        new PreferredMessageStorage(s2Split[0].Trim('"'), int.Parse(s2Split[1]), int.Parse(s2Split[2])),
                        new PreferredMessageStorage(s3Split[0].Trim('"'), int.Parse(s3Split[1]), int.Parse(s3Split[2])));
                }
            }
            return default;
        }

        public virtual async Task<PreferredMessageStorages> SetPreferredMessageStorageAsync(string storage1Name, string storage2Name, string storage3Name)
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

                    return new PreferredMessageStorages(
                        new PreferredMessageStorage(storage1Name, s1Used, s1Total),
                        new PreferredMessageStorage(storage2Name, s2Used, s2Total),
                        new PreferredMessageStorage(storage3Name, s3Used, s3Total));
                }
            }
            return default;
        }

        public virtual async Task<Sms> ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                //case SmsTextFormat.PDU:
                //    AtResponse pduResponse = await channel.SendMultilineCommand($"AT+CMGR={index},0", null);

                //    if (pduResponse.Success)
                //    {
                //        string statusLine = pduResponse.Intermediates.First();
                //        var statusLineMatch = Regex.Match(statusLine, @"\+CMGR:\s(?<status>\d),""(?<alpha>\w*)"",(?<length>\d+)");
                //        string pduLine = pduResponse.Intermediates.ElementAt(1);
                //        var pduLineMatch = Regex.Match(statusLine, @"(?<status>\d*)");
                //        if (statusLineMatch.Success && pduLineMatch.Success)
                //        {
                //            return default;
                //        }
                //    }
                //    break;
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
                            return new Sms(status, sender, received, message);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("The format is not supported");
            }
            return default;
        }

        public virtual async Task<IList<SmsWithIndex>> ListSmssAsync(SmsStatus smsStatus)
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
            return smss;
        }

        public virtual async Task<CommandStatus> DeleteSmsAsync(int index)
        {
            AtResponse response = await channel.SendCommand($"AT+CMGD={index}");
            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }
        #endregion

        #region _3GPP_TS_27_007
        public event EventHandler<UssdResponseEventArgs> UssdResponseReceived;

        public virtual async Task<SimStatus> GetSimStatusAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+CPIN?", "+CPIN:");

            if (!response.Success)
            {
                CmeError cmeError = AtErrorParsers.GetCmeError(response);
                if (cmeError != null)
                    throw new CmeException(cmeError.ToString());
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.First();
            var match = Regex.Match(cpinLine, @"\+CPIN:\s(?<pinresult>.*)");
            if (match.Success)
            {
                string cpinResult = match.Groups["pinresult"].Value;
                return cpinResult switch
                {
                    "SIM PIN" => SimStatus.SIM_PIN,
                    "SIM PUK" => SimStatus.SIM_PUK,
                    "PH-NET PIN" => SimStatus.SIM_NETWORK_PERSONALIZATION,
                    "READY" => SimStatus.SIM_READY,
                    _ => SimStatus.SIM_ABSENT,// Treat unsupported lock types as "sim absent"
                };
            }

            return SimStatus.SIM_NOT_READY;
        }

        public virtual async Task<CommandStatus> EnterSimPinAsync(PersonalIdentificationNumber pin)
        {
            AtResponse response = await channel.SendCommand($"AT+CPIN={pin}");

            Thread.Sleep(1500); // Without it, the reader loop crashes

            if (response.Success)
                return CommandStatus.OK;
            else return CommandStatus.ERROR;
        }

        public virtual async Task<SignalStrength> GetSignalStrengthAsync()
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
                    return new SignalStrength(rssi, ber);
                }
            }
            return default;
        }

        public virtual async Task<BatteryStatus> GetBatteryStatusAsync()
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
                    return new BatteryStatus((BatteryChargeStatus)bcs, bcl);
                }
            }
            return default;
        }

        public virtual async Task<CommandStatus> SetDateTimeAsync(DateTimeOffset value)
        {
            var sb = new StringBuilder("AT+CCLK=\"");
            int offsetQuarters = value.Offset.Hours * 4;
            sb.Append(value.ToString(@"yy/MM/dd,HH:mm:ss", CultureInfo.InvariantCulture));
            sb.Append(offsetQuarters.ToString("+00;-#", CultureInfo.InvariantCulture));
            sb.Append("\"");
            AtResponse response = await channel.SendCommand(sb.ToString());

            if (response.Success)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual async Task<DateTimeOffset?> GetDateTimeAsync()
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
                    return time;
                }
            }
            return default;
        }

        public virtual async Task<CommandStatus> SendUssdAsync(string code, int codingScheme = 15)
        {
            AtResponse response = await channel.SendCommand($"AT+CUSD=1,\"{code}\",{codingScheme}");

            if (response.Success)
                return CommandStatus.OK;
            else return CommandStatus.ERROR;
        }
        #endregion

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
