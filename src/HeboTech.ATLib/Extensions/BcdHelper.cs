namespace HeboTech.ATLib.Extensions
{
    public static class BcdHelper
    {
        // 0x28 => 0x82
        // 0x35 => 0x53
        // etc.
        public static byte SwapNibbles(this byte value)
        {
            return (byte)(((value % 10) * 10) + (value / 10));
        }

        public static string BcdToString(this byte value)
        {
            return (value / 10).ToString("X1") + (value % 10).ToString("X1");
        }
    }
}
