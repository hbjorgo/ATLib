using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.TP_LINK
{
    public class MA260 : ModemBase, IModem
    {
        /// <summary>
        /// Based on some Qualcomm chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.RequestToSend
        /// </summary>
        public MA260(AtChannel channel)
            : base(channel)
        {
        }

        public Task<ModemResponse<SmsReference>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme)
        {
            return SendSmsInPduFormatAsync(phoneNumber, message, codingScheme, false);
        }
    }
}
