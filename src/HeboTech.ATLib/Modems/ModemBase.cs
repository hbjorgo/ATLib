using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Modems
{
    public abstract class ModemBase
    {
        public event EventHandler<IncomingCallEventArgs> IncomingCall;
        public event EventHandler<MissedCallEventArgs> MissedCall;

        private readonly AtChannel atChannel;

        public ModemBase(ICommunicator communicator)
        {
            atChannel = new AtChannel(communicator)
            {
                UnsolicitedHandler = new Action<string, string>((a, b) =>
                {
                    if (a == "RING")
                        IncomingCall?.Invoke(this, new IncomingCallEventArgs());
                    else if (a.StartsWith("MISSED_CALL: "))
                    {
                        MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(a.Substring(13)));
                    }
                })
            };
        }

        public virtual SimStatus GetSimStatus()
        {
            var error = atChannel.SendSingleLineCommand("AT+CPIN?", "+CPIN:", out AtResponse response);

            if (error != AtChannel.AtError.NO_ERROR)
            {
                Console.WriteLine("Error :(");
                return SimStatus.SIM_NOT_READY;
            }

            switch (ResponseParsers.GetCmeError(response))
            {
                case ResponseParsers.AtCmeError.CME_SUCCESS:
                    break;
                case ResponseParsers.AtCmeError.CME_SIM_NOT_INSERTED:
                    return SimStatus.SIM_ABSENT;
                default:
                    return SimStatus.SIM_NOT_READY;
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.First();
            if (!ImprovedTokenizer.TokenizeStart(cpinLine, out cpinLine))
            {
                return SimStatus.SIM_NOT_READY;
            }

            if (!ImprovedTokenizer.TokenizeNextString(cpinLine, out _, out string cpinResult))
            {
                return SimStatus.SIM_NOT_READY;
            }

            return cpinResult switch
            {
                "SIM PIN" => SimStatus.SIM_PIN,
                "SIM PUK" => SimStatus.SIM_PUK,
                "PH-NET PIN" => SimStatus.SIM_NETWORK_PERSONALIZATION,
                "READY" => SimStatus.SIM_READY,
                _ => SimStatus.SIM_ABSENT,// Treat unsupported lock types as "sim absent"
            };
        }

        public virtual SignalStrength GetSignalStrength()
        {
            var error = atChannel.SendSingleLineCommand("AT+CSQ", "+CSQ:", out AtResponse response);

            if (error != AtChannel.AtError.NO_ERROR)
                return null;

            string line = response.Intermediates.First();
            var match = Regex.Match(line, @"\+CSQ:\s(?<rssi>\d+),(?<ber>\d+)");
            if (match.Success)
            {
                int rssi = int.Parse(match.Groups["rssi"].Value);
                int ber = int.Parse(match.Groups["ber"].Value);
                return new SignalStrength(rssi, ber);
            }
            return null;
        }

        public virtual BatteryStatus GetBatteryStatus()
        {
            var error = atChannel.SendSingleLineCommand("AT+CBC", "+CBC:", out AtResponse response);

            if (error != AtChannel.AtError.NO_ERROR)
                return null;

            string line = response.Intermediates.First();
            var match = Regex.Match(line, @"\+CBC:\s(?<bcs>\d+),(?<bcl>\d+),(?<voltage>\d+(?:\.\d+)?)V");
            if (match.Success)
            {
                int bcs = int.Parse(match.Groups["bcs"].Value);
                int bcl = int.Parse(match.Groups["bcl"].Value);
                double voltage = double.Parse(match.Groups["voltage"].Value, CultureInfo.InvariantCulture);
                return new BatteryStatus((BatteryChargeStatus)bcs, bcl, voltage);
            }
            return null;
        }

        public virtual SmsReference SendSMS(PhoneNumber phoneNumber, string message)
        {
            string cmd1 = $"AT+CMGS=\"{phoneNumber}\"";
            string cmd2 = message;
            var error = atChannel.SendSms(cmd1, cmd2, "+CMGS:", out AtResponse response);

            string line = response.Intermediates.First();
            var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
            if (match.Success)
            {
                int mr = int.Parse(match.Groups["mr"].Value);
                return new SmsReference(mr);
            }

            return null;
        }

        public void Close()
        {
            atChannel.OnReadClosed();
        }
    }
}
