using System;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.CodingSchemes
{
    /// <summary>
    /// Encode / decode GSM 7-bit strings
    /// </summary>
    public class Gsm7
    {
        public const byte DataCodingSchemeCode = 0x00;

        public static string Encode(string text)
        {
            byte[] textBytes = Encoding.ASCII.GetBytes(text).Reverse().ToArray();
            bool[] bits = new bool[textBytes.Length * 7];
            for (int i = 0; i < textBytes.Length; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    bits[i * 7 + j] = (textBytes[i] & (0x40 >> j)) != 0;
                }
            }

            byte[] octets = new byte[(int)Math.Ceiling(bits.Length / 8.0)];
            int offset = octets.Length * 8 - bits.Length;
            int bitShift = 0;
            for (int i = bits.Length - 1; i >= 0; i--)
            {
                octets[(i + offset) / 8] |= (byte)(bits[i] ? 0x01 << bitShift : 0x00);
                bitShift++;
                bitShift %= 8;
            }
            octets = octets.Reverse().ToArray();

            string str = BitConverter.ToString(octets).Replace("-", "");
            return str;
        }

        public static string Decode(string strGsm7bit)
        {
            if (strGsm7bit.Length % 2 != 0)
                throw new ArgumentException("Input length must be an even number");

            byte[] octets = CodingHelpers.StringToByteArray(strGsm7bit).Reverse().ToArray();

            bool[] bits = new bool[octets.Length * 8];
            for (int i = 0; i < octets.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bits[i * 8 + j] = (octets[i] & (0x80 >> j)) != 0;
                }
            }

            byte[] septets = new byte[(int)Math.Floor(bits.Length / 7.0)];
            int offset = bits.Length - septets.Length * 7;
            int bitShift = 0;
            for (int i = bits.Length - 1; i >= 0; i--)
            {
                septets[(i - offset) / 7] |= (byte)(bits[i] ? 0x01 << bitShift : 0x00);
                bitShift++;
                bitShift %= 7;
            }
            septets = septets.Reverse().ToArray();

            string str = Encoding.ASCII.GetString(septets);
            return str;
        }
    }
}
