using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Parsers;

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
    }
}
