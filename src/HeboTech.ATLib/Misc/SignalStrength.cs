using UnitsNet;

namespace HeboTech.ATLib.Misc
{
    public class SignalStrength
    {
        public SignalStrength(PowerRatio rssi, Ratio ber)
        {
            Rssi = rssi;
            Ber = ber;
        }

        /// <summary>
        /// Received Signal Strength Indicator. (99 - not known or not detectable)
        /// </summary>
        public PowerRatio Rssi { get; }

        /// <summary>
        /// Bit Error Rate. (99 - not known or not detectable)
        /// </summary>
        public Ratio Ber { get; }

        public override string ToString()
        {
            return $"RSSI: {Rssi}, BER: {Ber}";
        }
    }
}
