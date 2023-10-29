using System;
using System.Linq;

namespace HeboTech.ATLib.Extensions
{
    internal static class StringExtensions
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
