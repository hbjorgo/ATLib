using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Qualcomm
{
    public class MDM9225 : ModemBase, IModem, IMDM9225
    {
        public MDM9225(IAtChannel channel)
            : base(channel)
        {
        }

        public override Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message)
        {
            return base.SendSmsInPduFormatAsync(phoneNumber, message, false);
        }

        public override Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(PhoneNumber phoneNumber, string message, CharacterSet codingScheme)
        {
            return base.SendSmsInPduFormatAsync(phoneNumber, message, codingScheme, false);
        }
    }
}
