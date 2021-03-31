using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Inputs;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HeboTech.ATLib.Modems.SIMCOM
{
    public class SIM5320 : IModem
    {
        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;

        private readonly AtChannel channel;

        public SIM5320(AtChannel channel)
        {
            this.channel = channel;
            RegisterHandlers();
        }

        protected virtual void RegisterHandlers()
        {
            channel.UnsolicitedHandler = new Action<string, string>((line1, line2) =>
            {
                if (line1 == "RING")
                    IncomingCall?.Invoke(this, new IncomingCallEventArgs());
                else if (line1.StartsWith("MISSED_CALL: "))
                    MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(line1));
            });
        }

        public virtual void Close()
        {
            channel.Close();
        }

        public virtual CommandStatus DisableEcho()
        {
            var error = channel.SendCommand("ATE0");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

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

        public virtual CommandStatus AnswerIncomingCall()
        {
            var error = channel.SendCommand("ATA");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
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

        public virtual BatteryStatus GetBatteryStatus()
        {
            var error = channel.SendSingleLineCommand("AT+CBC", "+CBC:", out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+CBC:\s(?<bcs>\d+),(?<bcl>\d+),(?<voltage>\d+(?:\.\d+)?)V");
                if (match.Success)
                {
                    int bcs = int.Parse(match.Groups["bcs"].Value);
                    int bcl = int.Parse(match.Groups["bcl"].Value);
                    double voltage = double.Parse(match.Groups["voltage"].Value, CultureInfo.InvariantCulture);
                    return new BatteryStatus((BatteryChargeStatus)bcs, bcl, voltage);
                }
            }
            return null;
        }

        public virtual SmsReference SendSMS(PhoneNumber phoneNumber, string message)
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

        public virtual RemainingPinPukAttempts GetRemainingPinPukAttempts()
        {
            var error = channel.SendSingleLineCommand("AT+SPIC", "+SPIC:", out AtResponse response);

            if (error == AtError.NO_ERROR)
            {
                string line = response.Intermediates.First();
                var match = Regex.Match(line, @"\+SPIC:\s(?<pin1>\d+),(?<pin2>\d+),(?<puk1>\d+),(?<puk2>\d+)");
                if (match.Success)
                {
                    int pin1 = int.Parse(match.Groups["pin1"].Value);
                    int pin2 = int.Parse(match.Groups["pin2"].Value);
                    int puk1 = int.Parse(match.Groups["puk1"].Value);
                    int puk2 = int.Parse(match.Groups["puk2"].Value);
                    return new RemainingPinPukAttempts(pin1, pin2, puk1, puk2);
                }
            }
            return null;
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

        public CommandStatus SetSmsMessageFormat(SmsTextFormat format)
        {
            var error = channel.SendCommand($"AT+CMGF={(int)format}");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }

        public Sms ReadSMS(int index)
        {
            throw new NotImplementedException();
        }

        public IList<Sms> ListSMSs(SmsStatus smsStatus)
        {
            throw new NotImplementedException();
        }
    }
}
