using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsing;
using HeboTech.ATLib.Messaging;
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

        public override Task<IEnumerable<ModemResponse<SmsReference>>> SendSmsAsync(SmsSubmitRequest request)
        {
            return SendSmsAsync(request, false);
        }
    }
}
