using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Parsers;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HeboTech.ATLib.Modems.Generic
{
    public class _3GPP_TS_27_007
    {
        private readonly AtChannel channel;

        public _3GPP_TS_27_007(AtChannel channel)
        {
            this.channel = channel;
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
    }
}
