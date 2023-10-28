using HeboTech.ATLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.CodingSchemes
{
    /// <summary>
    /// Encode / decode UCS2 strings
    /// Unicode 16 bits.
    /// </summary>
    public class UCS2
    {
        public const CodingScheme DataCodingSchemeCode = CodingScheme.UCS2;

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

        public static byte[] EncodeToBytes(char[] input)
        {
            return Encoding.BigEndianUnicode.GetBytes(input);
        }

        /// <summary>
        /// Decode from UCS2
        /// </summary>
        /// <param name="input">UCS2 encoded string</param>
        /// <returns>Decoded string</returns>
        public static string Decode(string input)
        {
            IEnumerable<byte> bytes = input.ToByteArray();
            return Encoding.BigEndianUnicode.GetString(bytes.ToArray());
        }
    }
}
