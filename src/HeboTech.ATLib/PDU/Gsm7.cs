using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    public static class Gsm7
    {
        public static string Encode(string text)
        {
            byte[] octets = Encoding.ASCII.GetBytes(text);

            int septetCount = (int)Math.Floor((octets.Length * 8.0) / 7);

            byte[] septets = new byte[septetCount];
            for (int i = 0; i < septets.Length; i++)
            {
                septets[i] = GetSeptet(octets, i * 7);
            }

            return Encoding.ASCII.GetString(septets.ToArray());
        }

        public static string Decode(string strGsm7bit)
        {
            if (strGsm7bit.Length % 2 != 0)
                throw new ArgumentException("Input length must be an even number");

            byte[] octets = ConvertToBytes(strGsm7bit).Reverse().ToArray();

            int septetCount = (int)Math.Floor((octets.Length * 8.0) / 7);

            byte[] septets = new byte[septetCount];
            for (int i = 0; i < septets.Length; i++)
            {
                septets[i] = GetSeptet(octets, i * 7);
            }

            return Encoding.ASCII.GetString(septets.ToArray());
        }

        private static byte GetSeptet(byte[] data, int start)
        {
            byte result = 0;
            int startByte = data.Length - 1 - start / 8;
            int startBit = start % 8;
            byte mask;
            byte high = data[startByte];
            byte low = startByte == 0 ? (byte)0 : data[startByte - 1];
            if (startBit == 0)
            {
                mask = (byte)((0x7F));
                result = (byte)(high & mask);
            }
            else if (startBit == 7)
            {
                mask = 0x80;
                result = (byte)((high & mask) >> 7);
                mask = 0x3F;
                result |= (byte)((low & mask) << 1);
            }
            else if (startBit == 6)
            {
                mask = 0xC0;
                result = (byte)((high & mask) >> 6);
                mask = 0x1F;
                result |= (byte)((low & mask) << 2);
            }
            else if (startBit == 5)
            {
                mask = 0xE0;
                result = (byte)((high & mask) >> 5);
                mask = 0x0F;
                result |= (byte)((low & mask) << 3);
            }
            else if (startBit == 4)
            {
                mask = 0xF0;
                result = (byte)((high & mask) >> 4);
                mask = 0x07;
                result |= (byte)((low & mask) << 4);
            }
            else if (startBit == 3)
            {
                mask = 0xF8;
                result = (byte)((high & mask) >> 3);
                mask = 0x03;
                result |= (byte)((low & mask) << 5);
            }
            else if (startBit == 2)
            {
                mask = 0xFC;
                result = (byte)((high & mask) >> 2);
                mask = 0x01;
                result |= (byte)((low & mask) << 6);
            }
            else if (startBit == 1)
            {
                mask = 0xFE;
                result = (byte)((high & mask) >> 1);
                mask = 0x01;
                result |= (byte)((low & mask) << 7);
            }

            return result;
        }

        private static IEnumerable<byte> ConvertToBytes(string input)
        {
            if (input.Length % 2 != 0)
                yield break;

            for (int i = 0; i < input.Length / 2; i++)
            {
                yield return byte.Parse(input.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
        }
    }
}
