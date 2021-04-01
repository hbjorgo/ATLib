﻿using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HeboTech.ATLib.Modems.Generic
{
    public abstract class ModemBase : IModem
    {
        protected readonly AtChannel channel;
        private bool disposedValue;

        public ModemBase(AtChannel channel)
        {
            this.channel = channel;
            channel.UnsolicitedEvent += Channel_UnsolicitedEvent;
        }

        private void Channel_UnsolicitedEvent(object sender, UnsolicitedEventArgs e)
        {
            if (e.Line1 == "RING")
                IncomingCall?.Invoke(this, new IncomingCallEventArgs());
            else if (e.Line1.StartsWith("MISSED_CALL: "))
                MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(e.Line1));
            else if (e.Line1.StartsWith("+CMTI: "))
            {
                var match = Regex.Match(e.Line1, @"\+CMTI:\s""(?<storage>[A-Z]+)"",(?<index>\d+)");
                if (match.Success)
                {
                    string storage = match.Groups["storage"].Value;
                    int index = int.Parse(match.Groups["index"].Value);
                    SmsReceived?.Invoke(this, new SmsReceivedEventArgs(storage, index));
                }
            }
        }

        #region _V_25TER
        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;

        public virtual CommandStatus AnswerIncomingCall()
        {
            var error = channel.SendCommand("ATA");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual CommandStatus DisableEcho()
        {
            var error = channel.SendCommand("ATE0");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual ProductIdentificationInformation GetProductIdentificationInformation()
        {
            var error = channel.SendMultilineCommand("ATI", null, out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string line in response.Intermediates)
                {
                    builder.AppendLine(line);
                }

                return new ProductIdentificationInformation(builder.ToString());
            }
            return null;
        }

        public virtual CallDetails Hangup()
        {
            var error = channel.SendSingleLineCommand("AT+CHUP", "VOICE CALL:", out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"VOICE CALL: END: (?<duration>\d+)");
                if (match.Success)
                {
                    int duration = int.Parse(match.Groups["duration"].Value);
                    return new CallDetails(TimeSpan.FromSeconds(duration));
                }
            }
            return null;
        }
        #endregion

        #region _3GPP_TS_27_005
        public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        public virtual CommandStatus SetSmsMessageFormat(SmsTextFormat format)
        {
            var error = channel.SendCommand($"AT+CMGF={(int)format}");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual SmsReference SendSms(PhoneNumber phoneNumber, string message)
        {
            string cmd1 = $"AT+CMGS=\"{phoneNumber}\"";
            string cmd2 = message;
            var error = channel.SendSms(cmd1, cmd2, "+CMGS:", out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
                if (match.Success)
                {
                    int mr = int.Parse(match.Groups["mr"].Value);
                    return new SmsReference(mr);
                }
            }
            return null;
        }

        public virtual Sms ReadSms(int index)
        {
            var error = channel.SendMultilineCommand($"AT+CMGR={index},0", null, out AtResponse response);

            if (error == AtError.NO_ERROR && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.First();
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
                    string message = response.Intermediates.Last();
                    return new Sms(status, sender, received, message);
                }
            }
            return null;
        }

        public virtual IList<SmsWithIndex> ListSmss(SmsStatus smsStatus)
        {
            var error = channel.SendMultilineCommand($"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\",0", null, out AtResponse response);

            List<SmsWithIndex> smss = new List<SmsWithIndex>();
            if (error == AtError.NO_ERROR)
            {
                for (int i = 0; i < response.Intermediates.Count; i += 2)
                {
                    string metaData = response.Intermediates[i];
                    var match = Regex.Match(metaData, @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
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
                        string message = response.Intermediates[i + 1];
                        smss.Add(new SmsWithIndex(index, status, sender, received, message));
                    }
                }
            }
            return smss;
        }

        public virtual CommandStatus DeleteSms(int index)
        {
            var error = channel.SendCommand($"AT+CMGD={index}");
            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }
        #endregion

        #region _3GPP_TS_27_007
        public virtual SimStatus GetSimStatus()
        {
            var error = channel.SendSingleLineCommand("AT+CPIN?", "+CPIN:", out AtResponse response);

            if (error != AtError.NO_ERROR)
                return SimStatus.SIM_NOT_READY;

            switch (AtErrorParsers.GetCmeError(response))
            {
                case AtErrorParsers.AtCmeError.CME_SUCCESS:
                    break;
                case AtErrorParsers.AtCmeError.CME_SIM_NOT_INSERTED:
                    return SimStatus.SIM_ABSENT;
                default:
                    return SimStatus.SIM_NOT_READY;
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.First();
            if (!AtTokenizer.TokenizeStart(cpinLine, out cpinLine))
                return SimStatus.SIM_NOT_READY;

            if (!AtTokenizer.TokenizeNextString(cpinLine, out _, out string cpinResult))
                return SimStatus.SIM_NOT_READY;

            return cpinResult switch
            {
                "SIM PIN" => SimStatus.SIM_PIN,
                "SIM PUK" => SimStatus.SIM_PUK,
                "PH-NET PIN" => SimStatus.SIM_NETWORK_PERSONALIZATION,
                "READY" => SimStatus.SIM_READY,
                _ => SimStatus.SIM_ABSENT,// Treat unsupported lock types as "sim absent"
            };
        }

        public virtual CommandStatus EnterSimPin(PersonalIdentificationNumber pin)
        {
            var error = channel.SendCommand($"AT+CPIN={pin}");

            Thread.Sleep(1500); // Without it, the reader loop crashes

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            else return CommandStatus.ERROR;
        }

        public virtual SignalStrength GetSignalStrength()
        {
            var error = channel.SendSingleLineCommand("AT+CSQ", "+CSQ:", out AtResponse response);

            if (error == AtError.NO_ERROR)
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
            return null;
        }

        public virtual BatteryStatus GetBatteryStatus()
        {
            var error = channel.SendSingleLineCommand("AT+CBC", "+CBC:", out AtResponse response);

            if (error == AtError.NO_ERROR)
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
            return null;
        }

        public virtual CommandStatus SetDateTime(DateTimeOffset value)
        {
            var sb = new StringBuilder("AT+CCLK=\"");
            int offsetQuarters = value.Offset.Hours * 4;
            sb.Append(value.ToString(@"yy/MM/dd,HH:mm:ss", CultureInfo.InvariantCulture));
            sb.Append(offsetQuarters.ToString("+00;-#", CultureInfo.InvariantCulture));
            sb.Append("\"");
            var error = channel.SendCommand(sb.ToString());

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public virtual DateTimeOffset? GetDateTime()
        {
            var error = channel.SendSingleLineCommand("AT+CCLK?", "+CCLK:", out AtResponse response);

            if (error == AtError.NO_ERROR)
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
            return null;
        }
        #endregion

        public void Close()
        {
            Dispose();
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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