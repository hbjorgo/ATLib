using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.SIMCOM
{
    public class SIM5320 : ModemBase, IModem
    {
        public SIM5320(AtChannel channel)
            : base(channel)
        {
        }

        #region Custom
        public virtual async Task<RemainingPinPukAttempts> GetRemainingPinPukAttemptsAsync()
        {
            AtResponse response = await channel.SendSingleLineCommandAsync("AT+SPIC", "+SPIC:");

            if (response.Success)
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
        #endregion

        #region _3GPP_TS_27_005

        public override async Task<Sms> ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                case SmsTextFormat.Text:
                    AtResponse response = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

                    if (response.Success && response.Intermediates.Count > 0)
                    {
                        string line = response.Intermediates.First();
                        var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+?\d+)"",("""")?,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
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
                default:
                    throw new NotSupportedException("The format is not supported");
            }
        }

        public override async Task<IList<SmsWithIndex>> ListSmssAsync(SmsStatus smsStatus)
        {
            AtResponse response = await channel.SendMultilineCommand($"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\"", null);

            List<SmsWithIndex> smss = new List<SmsWithIndex>();
            if (response.Success)
            {
                for (int i = 0; i < response.Intermediates.Count; i += 2)
                {
                    string metaData = response.Intermediates[i];
                    var match = Regex.Match(metaData, @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+?\d+)"",("""")?,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
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
        #endregion
    }
}
