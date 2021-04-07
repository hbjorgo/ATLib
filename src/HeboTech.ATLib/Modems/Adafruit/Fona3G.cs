using HeboTech.ATLib.Modems.SIMCOM;
using HeboTech.ATLib.Parsers;

namespace HeboTech.ATLib.Modems.Adafruit
{
    public class Fona3G : SIM5320, IModem
    {
        /// <summary>
        /// Based on SIMCOM SIM5320 chipset
        /// 
        /// Serial port settings:
        /// 9600 8N1 Handshake.None
        /// </summary>
        public Fona3G(AtChannel channel)
            : base(channel)
        {
        }
    }
}
