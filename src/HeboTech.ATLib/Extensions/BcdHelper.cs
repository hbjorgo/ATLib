using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.Extensions
{
    public static class BcdHelper
    {
        // 0x28 => 0x82
        // 0x35 => 0x53
        // etc.
        public static byte SwapNibbles(this byte value)
        {
            return (byte)((value & 0x0F) << 4 | (value & 0xF0) >> 4);
        }

        public static byte DecimalToBcd(this byte value)
        {
            return (byte)((value / 10 << 4) | (value % 10));
        }

        public static byte BcdToDecimal(this byte value)
        {
            return (byte)(((value & 0xF0) >> 4) * 10 + (value & 0x0F));
        }

        public static string BcdToString(this byte value)
        {
            return value.ToString("X2");
        }
    }
}
