using System;
using System.Collections.Generic;
using System.Linq;

namespace HeboTech.ATLib.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string hexString)
        {
            return Enumerable.Range(0, hexString.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
