using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.PDU;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.SIMCOM
{
    public class SIM5320 : ModemBase, IModem, ISIM5320
    {
        public SIM5320(IAtChannel channel)
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

        public Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message)
        {
            return base.SendSmsAsync(phoneNumber, message, false);
        }

        public Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message, CharacterSet codingScheme)
        {
            return base.SendSmsAsync(phoneNumber, message, codingScheme, false);
        }

        public override async Task<ModemResponse<List<SmsWithIndex>>> ListSmssAsync(SmsStatus smsStatus)
        {
            string command = $"AT+CMGL={(int)smsStatus}";

            AtResponse response = await channel.SendMultilineCommand(command, null);

            List<SmsWithIndex> smss = new List<SmsWithIndex>();
            if (response.Success)
            {
                if ((response.Intermediates.Count % 2) != 0)
                    return ModemResponse.HasResultError<List<SmsWithIndex>>();

                for (int i = 0; i < response.Intermediates.Count; i += 2)
                {
                    string metaDataLine = response.Intermediates[i];
                    string messageLine = response.Intermediates[i + 1];
                    var match = Regex.Match(metaDataLine, @"\+CMGL:\s(?<index>\d+),(?<status>\d+),"""",(?<length>\d+)");
                    if (match.Success)
                    {
                        int index = int.Parse(match.Groups["index"].Value);
                        SmsStatus status = (SmsStatus)int.Parse(match.Groups["status"].Value);

                        // Sent when AT+CSDH=1 is set
                        int length = int.Parse(match.Groups["length"].Value);

                        SmsDeliver sms = SmsDeliverDecoder.Decode(messageLine.ToByteArray());
                        smss.Add(new SmsWithIndex(index, status, sms.SenderNumber, sms.Timestamp, sms.Message));
                    }
                }
            }
            return ModemResponse.IsResultSuccess(smss);
        }
        #endregion

        public virtual async Task<ModemResponse> ReInitializeSimAsync()
        {
            AtResponse response = await channel.SendCommand($"AT+CRFSIM");

            if (response.Success)
                return ModemResponse.IsSuccess();

            AtErrorParsers.TryGetError(response.FinalResponse, out Error error);
            return ModemResponse.HasError(error);
        }
    }
}
