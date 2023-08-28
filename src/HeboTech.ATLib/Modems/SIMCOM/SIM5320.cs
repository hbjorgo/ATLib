using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.PDU;
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

        public override Task<ModemResponse<SmsReference>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, byte[] message, CodingScheme codingScheme)
        {
            return base.SendSmsInPduFormatAsync(phoneNumber, message, codingScheme, false);
        }

        public override async Task<ModemResponse<Sms>> ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                case SmsTextFormat.PDU:
                    AtResponse pduResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

                    if (pduResponse.Success && pduResponse.Intermediates.Count > 0)
                    {
                        string line1 = pduResponse.Intermediates[0];
                        string line2 = pduResponse.Intermediates[1];
                        var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d{1}),""(?<alphabet>.*)"",(?<length>\d+)");
                        var line2Match = Regex.Match(line2, @"(?<pdu>[0-9A-F]*)");
                        if (line1Match.Success && line2Match.Success)
                        {
                            int status = int.Parse(line1Match.Groups["status"].Value);
                            string alphabet = line1Match.Groups["alphabet"].Value;
                            int length = int.Parse(line1Match.Groups["length"].Value);
                            string pdu = line2Match.Groups["pdu"].Value;
#if NETSTANDARD2_0
                            SmsDeliver pduMessage = Pdu.DecodeSmsDeliver(pdu.AsSpan());
#elif NETSTANDARD2_1_OR_GREATER
                            SmsDeliver pduMessage = Pdu.DecodeSmsDeliver(pdu);
#endif
                            return ModemResponse.ResultSuccess(new Sms((SmsStatus)status, pduMessage.SenderNumber, pduMessage.Timestamp, pduMessage.Message));
                        }
                    }
                    return ModemResponse.ResultError<Sms>();
                case SmsTextFormat.Text:
                    AtResponse textResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

                    if (textResponse.Success && textResponse.Intermediates.Count > 0)
                    {
                        string line = textResponse.Intermediates.First();
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
                            string message = textResponse.Intermediates.Last();
                            return ModemResponse.ResultSuccess(new Sms(status, sender, received, message));
                        }
                    }
                    return ModemResponse.ResultError<Sms>();
                default:
                    throw new NotSupportedException("The format is not supported");
            }
        }

        public override async Task<ModemResponse<List<SmsWithIndex>>> ListSmssAsync(SmsStatus smsStatus)
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
            return ModemResponse.ResultSuccess(smss);
        }
#endregion
    }
}
