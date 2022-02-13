﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
        public const byte DataCodingSchemeCode = 0x08;

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
            IEnumerable<byte> bytes = CodingHelpers.StringToByteArray(input);
            return Encoding.BigEndianUnicode.GetString(bytes.ToArray());
        }
    }
}
