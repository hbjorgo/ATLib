using HeboTech.ATLib.Modems.SIMCOM;
using HeboTech.ATLib.Parsers;

namespace HeboTech.ATLib.Modems.Adafruit
{
    public class AdafruitFona3G : SIMCOM_SIM5320
    {
        /// <summary>
        /// Based on SIMCOM SIM5320 chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.None
        /// </summary>
        public AdafruitFona3G(AtChannel channel)
            : base(channel)
        {
        }
    }
}
