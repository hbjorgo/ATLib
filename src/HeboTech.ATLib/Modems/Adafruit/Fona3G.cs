using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.SIMCOM;
using HeboTech.ATLib.Parsers;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Adafruit
{
    public class Fona3G : SIM5320, IModem, IFona3G
    {
        /// <summary>
        /// Based on SIMCOM SIM5320 chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.None
        /// </summary>
        public Fona3G(IAtChannel channel)
            : base(channel)
        {
        }

        public override async Task<bool> SetRequiredSettingsBeforePinAsync()
        {
            ModemResponse echo = await DisableEchoAsync();
            ModemResponse errorFormat = await SetErrorFormatAsync(1);
            return echo.Success && errorFormat.Success;
        }

        public override async Task<bool> SetRequiredSettingsAfterPinAsync()
        {
            ModemResponse currentCharacterSet = await SetCharacterSetAsync(CharacterSet.UCS2);
            ModemResponse smsMessageFormat = await SetSmsMessageFormatAsync(SmsTextFormat.PDU);
            return currentCharacterSet.Success && smsMessageFormat.Success;
        }

        //public override async Task<ModemResponse<Sms>> ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        //{
        //    switch (smsTextFormat)
        //    {
        //        case SmsTextFormat.PDU:
        //            AtResponse pduResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

        //            if (pduResponse.Success)
        //            {
        //                string line1 = pduResponse.Intermediates[0];
        //                var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d),(""(?<alpha>\w*)"")*,(?<length>\d+)");
        //                string line2 = pduResponse.Intermediates[1];
        //                var line2Match = Regex.Match(line2, @"(?<status>[0-9A-Z]*)");
        //                if (line1Match.Success && line2Match.Success)
        //                {
        //                    int statusCode = int.Parse(line1Match.Groups["status"].Value);
        //                    SmsStatus status = (SmsStatus)statusCode;

        //                    string pdu = line2Match.Groups["status"].Value;
        //                    SmsDeliver pduMessage = SmsDeliverDecoder.Decode(pdu.ToByteArray());

        //                    return ModemResponse.IsResultSuccess(new Sms(status, pduMessage.SenderNumber, pduMessage.Timestamp, pduMessage.Message));
        //                }
        //                if (AtErrorParsers.TryGetError(pduResponse.FinalResponse, out Error pduError))
        //                    return ModemResponse.HasResultError<Sms>(pduError);
        //            }
        //            break;
        //        case SmsTextFormat.Text:
        //            AtResponse textResponse = await channel.SendMultilineCommand($"AT+CMGR={index}", null);

        //            if (textResponse.Success && textResponse.Intermediates.Count > 0)
        //            {
        //                string line = textResponse.Intermediates.First();
        //                var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+\d+)"","""",""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))"",(?<addressType>\d+),(?<tpduFirstOctet>\d),(?<pid>\d),(?<dcs>\d),(?<serviceCenterAddress>""\+\d+""),(?<serviceCenterAddressType>\d+),(?<length>\d+)");
        //                if (match.Success)
        //                {
        //                    SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
        //                    PhoneNumberDTO sender = new PhoneNumberDTO(match.Groups["sender"].Value);
        //                    int year = int.Parse(match.Groups["year"].Value);
        //                    int month = int.Parse(match.Groups["month"].Value);
        //                    int day = int.Parse(match.Groups["day"].Value);
        //                    int hour = int.Parse(match.Groups["hour"].Value);
        //                    int minute = int.Parse(match.Groups["minute"].Value);
        //                    int second = int.Parse(match.Groups["second"].Value);
        //                    int zone = int.Parse(match.Groups["zone"].Value);

        //                    int addressType = int.Parse(match.Groups["addressType"].Value);
        //                    int tpduFirstOctet = int.Parse(match.Groups["tpduFirstOctet"].Value);
        //                    int protocolIdentifier = int.Parse(match.Groups["pid"].Value);
        //                    int dataCodingScheme = int.Parse(match.Groups["dcs"].Value);
        //                    string serviceCenterAddress = match.Groups["serviceCenterAddress"].Value;
        //                    int serviceCenterAddressType = int.Parse(match.Groups["serviceCenterAddressType"].Value);
        //                    int length = int.Parse(match.Groups["length"].Value);

        //                    DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));
        //                    string message = textResponse.Intermediates.Last();

        //                    CodingScheme dcs = (CodingScheme)dataCodingScheme;
        //                    if (dcs == CodingScheme.UCS2)
        //                        message = UCS2.Decode(message);

        //                    return ModemResponse.IsResultSuccess(new Sms(status, sender, received, message));
        //                }
        //            }
        //            if (AtErrorParsers.TryGetError(textResponse.FinalResponse, out Error textError))
        //                return ModemResponse.HasResultError<Sms>(textError);
        //            break;
        //        default:
        //            throw new NotSupportedException("The format is not supported");
        //    }
        //    return ModemResponse.HasResultError<Sms>();
        //}

        //public override async Task<ModemResponse<List<SmsWithIndex>>> ListSmssAsync(SmsStatus smsStatus)
        //{
        //    string command = currentSmsTextFormat switch
        //    {
        //        CurrentSmsTextFormat.Text => $"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\"",
        //        CurrentSmsTextFormat.PDU => $"AT+CMGL={(int)smsStatus}",
        //        _ => throw new Exception("Unknown SMS Text Format")
        //    };

        //    AtResponse response = await channel.SendMultilineCommand(command, null);

        //    List<SmsWithIndex> smss = new List<SmsWithIndex>();
        //    if (response.Success)
        //    {
        //        switch (currentSmsTextFormat)
        //        {
        //            case CurrentSmsTextFormat.PDU:
        //                if ((response.Intermediates.Count % 2) != 0)
        //                    return ModemResponse.HasResultError<List<SmsWithIndex>>();

        //                for (int i = 0; i < response.Intermediates.Count; i += 2)
        //                {
        //                    string metaDataLine = response.Intermediates[i];
        //                    string messageLine = response.Intermediates[i + 1];
        //                    var match = Regex.Match(metaDataLine, @"\+CMGL:\s(?<index>\d+),(?<status>\d+),"""",(?<length>\d+)");
        //                    if (match.Success)
        //                    {
        //                        int index = int.Parse(match.Groups["index"].Value);
        //                        SmsStatus status = (SmsStatus)int.Parse(match.Groups["status"].Value);

        //                        // Sent when AT+CSDH=1 is set
        //                        int length = int.Parse(match.Groups["length"].Value);

        //                        SmsDeliver sms = SmsDeliverDecoder.Decode(messageLine.ToByteArray());
        //                        smss.Add(new SmsWithIndex(index, status, sms.SenderNumber, sms.Timestamp, sms.Message));
        //                    }
        //                }
        //                break;
        //            case CurrentSmsTextFormat.Text:
        //                if ((response.Intermediates.Count % 2) != 0)
        //                    return ModemResponse.HasResultError<List<SmsWithIndex>>();

        //                for (int i = 0; i < response.Intermediates.Count; i += 2)
        //                {
        //                    string metaDataLine = response.Intermediates[i];
        //                    string messageLine = response.Intermediates[i + 1];
        //                    var match = Regex.Match(metaDataLine, @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+*\d+)"","""",""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))"",(?<addressType>\d+),(?<tpduFirstOctet>\d),(?<pid>\d),(?<dcs>\d),(?<serviceCenterAddress>""\+\d+""),(?<serviceCenterAddressType>\d+),(?<length>\d+)");
        //                    if (match.Success)
        //                    {
        //                        int index = int.Parse(match.Groups["index"].Value);
        //                        SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
        //                        PhoneNumberDTO sender = new PhoneNumberDTO(match.Groups["sender"].Value);
        //                        int year = int.Parse(match.Groups["year"].Value);
        //                        int month = int.Parse(match.Groups["month"].Value);
        //                        int day = int.Parse(match.Groups["day"].Value);
        //                        int hour = int.Parse(match.Groups["hour"].Value);
        //                        int minute = int.Parse(match.Groups["minute"].Value);
        //                        int second = int.Parse(match.Groups["second"].Value);
        //                        int zone = int.Parse(match.Groups["zone"].Value);

        //                        // Sent when AT+CSDH=1 is set
        //                        int addressType = int.Parse(match.Groups["addressType"].Value);
        //                        int dataLength = int.Parse(match.Groups["length"].Value);

        //                        DateTimeOffset received = new DateTimeOffset(2000 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(15 * zone));

        //                        string message = messageLine;
        //                        if (messageLine.Length != dataLength)
        //                            message = UCS2.Decode(messageLine);

        //                        smss.Add(new SmsWithIndex(index, status, sender, received, message));
        //                    }
        //                }
        //                break;
        //            case CurrentSmsTextFormat.Unknown:
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    return ModemResponse.IsResultSuccess(smss);
        //}
    }
}
