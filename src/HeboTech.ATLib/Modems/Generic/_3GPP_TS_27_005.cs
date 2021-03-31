using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Modems.Generic
{
    public class _3GPP_TS_27_005
    {
        private readonly AtChannel channel;

        public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        public _3GPP_TS_27_005(AtChannel channel)
        {
            this.channel = channel;
            channel.UnsolicitedEvent += Channel_UnsolicitedEvent;
        }

        private void Channel_UnsolicitedEvent(object sender, UnsolicitedEventArgs e)
        {
            if (e.Line1.StartsWith("+CMTI: "))
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

        public virtual CommandStatus SetSmsMessageFormat(SmsTextFormat format)
        {
            var error = channel.SendCommand($"AT+CMGF={(int)format}");

            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
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

        public virtual Sms ReadSMS(int index)
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

        public virtual IList<SmsWithIndex> ListSMSs(SmsStatus smsStatus)
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

        public virtual CommandStatus DeleteSMS(int index)
        {
            var error = channel.SendCommand($"AT+CMGD={index}");
            if (error == AtError.NO_ERROR)
                return CommandStatus.OK;
            return CommandStatus.ERROR;
        }
    }
}
