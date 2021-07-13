using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.Parsers
{
    /// <summary>
    /// Encode / decode UCS2 strings
    /// </summary>
    public class UCS2
    {
        /// <summary>
        /// Encode to UCS2
        /// </summary>
        /// <param name="input">String to encode</param>
        /// <returns>UCS2 encoded string</returns>
        public static string Encode(string input)
        {
            byte[] bytes = Encoding.BigEndianUnicode.GetBytes(input);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// Decode from UCS2
        /// </summary>
        /// <param name="input">UCS2 encoded string</param>
        /// <returns>Decoded string</returns>
        public static string Decode(string input)
        {
            IEnumerable<byte> bytes = ConvertToBytes(input);
            return Encoding.BigEndianUnicode.GetString(bytes.ToArray());
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
