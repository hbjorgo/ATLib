using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Qualcomm
{
    public class MDM9225 : ModemBase, IModem
    {
        public MDM9225(AtChannel channel)
            : base(channel)
        {
        }

        public override Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsInPduFormatAsync(PhoneNumber phoneNumber, byte[] message, CodingScheme codingScheme)
        {
            return base.SendSmsInPduFormatAsync(phoneNumber, message, codingScheme, false);
        }
    }
}
