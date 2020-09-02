using HeboTech.ATLib.Modems.SIMCOM;
using HeboTech.ATLib.Parsers;

namespace HeboTech.ATLib.Modems.Adafruit
{
    public class AdafruitFona3G : SIMCOM_SIM5320
    {
        public AdafruitFona3G(AtChannel channel)
            : base(channel)
        {
        }
    }
}
