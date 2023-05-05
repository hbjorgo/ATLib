using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Qualcomm
{
    public class MDM9225 : ModemBase, IModem
    {
        public MDM9225(AtChannel channel)
            : base(channel)
        {
        }

        public Task<ModemResponse<SmsReference>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme)
        {
            return SendSmsInPduFormatAsync(phoneNumber, message, codingScheme, false);
        }
    }
}
