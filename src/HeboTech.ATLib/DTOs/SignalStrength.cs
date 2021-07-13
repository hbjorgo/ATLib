namespace HeboTech.ATLib.DTOs
{
    public class SignalStrength
    {
        public SignalStrength(int rssi, int ber)
        {
            Rssi = rssi;
            Ber = ber;
        }

        /// <summary>
        /// Received Signal Strength Indicator. (99 - not known or not detectable)
        /// </summary>
        public int Rssi { get; }

        /// <summary>
        /// Bit Error Rate. (99 - not known or not detectable)
        /// </summary>
        public int Ber { get; }

        public override string ToString()
        {
            return $"RSSI: {Rssi}, BER: {Ber}";
        }
    }
}
