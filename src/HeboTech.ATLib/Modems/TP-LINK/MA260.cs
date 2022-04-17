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

        public Task<ModemResponse<SmsReference>> SendSmsAsync(PhoneNumber phoneNumber, string message, SmsTextFormat smsTextFormat)
        {
            return SendSmsAsync(phoneNumber, message, smsTextFormat, false);
        }
    }
}
