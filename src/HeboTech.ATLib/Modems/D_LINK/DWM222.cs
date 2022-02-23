using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Qualcomm;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.PDU;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.D_LINK
{
    public class DWM222 : MDM9225, IModem
    {
        /// <summary>
        /// Based on Qualcomm MDM9225 chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.RequestToSend
        /// </summary>
        public DWM222(AtChannel channel)
            : base(channel)
        {
        }

        public override async Task<ModemResponse<SmsReference>> SendSmsAsync(PhoneNumber phoneNumber, string message, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                case SmsTextFormat.PDU:
                    {
                        string pdu = Pdu.Encode(phoneNumber, Gsm7.Encode(message), Gsm7.DataCodingSchemeCode, false);
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
                                return ModemResponse.ResultSuccess(new SmsReference(mr));
                            }
                        }
                        else
                        {
                            if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
                                return ModemResponse.ResultError<SmsReference>(error.ToString());
                        }
                        return ModemResponse.ResultError<SmsReference>();
                    }
                case SmsTextFormat.Text:
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
                                return ModemResponse.ResultSuccess(new SmsReference(mr));
                            }
                        }
                        return ModemResponse.ResultError<SmsReference>();
                    }
                default:
                    throw new NotSupportedException($"Text format {smsTextFormat} is not supported");
            }
        }
    }
}
