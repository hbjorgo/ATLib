using HeboTech.ATLib.Modems.Qualcomm;
using HeboTech.ATLib.Parsers;

namespace HeboTech.ATLib.Modems.D_LINK
{
    public class DWM_222 : MDM9225
    {
        /// <summary>
        /// Based on Qualcomm MDM9225 chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.RequestToSend
        /// </summary>
        public DWM_222(AtChannel channel)
            : base(channel)
        {
        }
    }
}
