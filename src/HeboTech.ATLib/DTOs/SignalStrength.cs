namespace HeboTech.ATLib.DTOs
{
    public class SignalStrength
    {
        public SignalStrength(int rssi, int ber)
        {
            Rssi = rssi;
            Ber = ber;
        }

        public int Rssi { get; }
        public int Ber { get; }

        public override string ToString()
        {
            return $"RSSI: {Rssi}, BER: {Ber}";
        }
    }
}
