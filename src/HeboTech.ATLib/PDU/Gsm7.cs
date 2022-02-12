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

        private static byte ToByte(IEnumerable<bool> value)
        {
            if (value.Count() != 8)
                throw new ArgumentException(nameof(value));
            byte retVal = 0x00;
            for (int i = 0; i < value.Count(); i++)
            {
                retVal |= (byte)(value.ElementAt(i) ? 0x80 >> i : 0x00);
            }
            return retVal;
        }

        public static string Encode2(string text)
        {
            byte[] octets = Encoding.ASCII.GetBytes(text).Reverse().ToArray();
            bool[] bits = new bool[octets.Length * 7];
            for (int i = 0; i < octets.Length; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    bits[i * 7 + j] = (octets[i] & (0x40 >> j)) != 0;
                }
            }

            byte[] septets = new byte[bits.Length / 8];
            for (int i = 0; i < septets.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    septets[i] |= (byte)(bits[i * 8 + j] ? 0x80 >> j : 0x00);
                }
            }
            septets = septets.Reverse().ToArray();

            string str = BitConverter.ToString(septets).Replace("-", "");
            return str;
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
