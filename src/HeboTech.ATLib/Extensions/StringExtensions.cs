using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.Extensions
{
    internal static class StringExtensions
    {
        public static IEnumerable<string> SplitByLength(this string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        public static string ToHexString(this byte value)
        {
            return value.ToString("X2");
        }

        public static string ToByteHexString(this int value)
        {
            return value.ToString("X2");
        }

        public static byte[] FromHexStringToByteArray(this string hexString)
        {
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
