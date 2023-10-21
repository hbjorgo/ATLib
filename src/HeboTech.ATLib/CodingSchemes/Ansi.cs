using System;
using System.Text;

namespace HeboTech.ATLib.CodingSchemes
{
    /// <summary>
    /// Encode / decode ANSI 8-bit strings
    /// </summary>
    public class Ansi
    {
        public const CodingScheme DataCodingSchemeCode = CodingScheme.Ansi;

        /// <summary>
        /// Encode to ANSI 8-bit
        /// </summary>
        /// <param name="input">String to encode</param>
        /// <returns>ANSI encoded string</returns>
        public static string Encode(string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static byte[] EncodeToBytes(string input)
        {
            return Encoding.ASCII.GetBytes(input);
        }

        /// <summary>
        /// Decode from ANSI 8-bit
        /// </summary>
        /// <param name="input">ANSI encoded string</param>
        /// <returns>Decoded string</returns>
        public static string Decode(string input)
        {
            byte[] bytes = CodingHelpers.StringToByteArray(input);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
