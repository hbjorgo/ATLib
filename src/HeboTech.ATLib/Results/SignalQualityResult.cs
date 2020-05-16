using System;

namespace HeboTech.ATLib.Results
{
    public class SignalQualityResult
    {
        public SignalQualityResult(int rssi, int ber)
        {
            Rssi = rssi;
            Ber = ber;
        }

        public int Rssi { get; }
        public int Ber { get; }

        public string RssiDbm
        {
            get
            {
                string dbm;
                switch (Rssi)
                {
                    case 0:
                        dbm = "-115 dBm or less";
                        break;
                    case 1:
                        dbm = "-111 dBm";
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                        dbm = $"{-110 + (Rssi - 2) * 2} dBm";
                        break;
                    case 31:
                        dbm = "-52 dBm or greater";
                        break;
                    case 99:
                        dbm = "Not known or not detectable";
                        break;
                    default:
                        throw new ArgumentException(nameof(Rssi));

                }
                return dbm;
            }
        }

        public override string ToString()
        {
            return $"RSSI: {Rssi}, BER: {Ber}";
        }
    }
}
