using HeboTech.ATLib.Extensions;
using System;
using System.Linq;

namespace HeboTech.ATLib.PDU
{
    public static class TpduTime
    {
        public static byte[] EncodeTimestamp(DateTimeOffset value)
        {
            byte year = ((byte)(value.Year % 100)).DecimalToBcd().SwapNibbles();
            byte month = ((byte)value.Month).DecimalToBcd().SwapNibbles();
            byte day = ((byte)value.Day).DecimalToBcd().SwapNibbles();
            byte hour = ((byte)value.Hour).DecimalToBcd().SwapNibbles();
            byte minute = ((byte)value.Minute).DecimalToBcd().SwapNibbles();
            byte second = ((byte)value.Second).DecimalToBcd().SwapNibbles();

            byte timeZoneQuarters = ((byte)(Math.Abs(value.Offset.TotalMinutes) / 15)).DecimalToBcd().SwapNibbles();
            if (value.Offset.TotalMinutes < 0)
                timeZoneQuarters |= 0b0000_1000;

            return new byte[] { year, month, day, hour, minute, second, timeZoneQuarters };
        }

        public static DateTimeOffset DecodeTimestamp(ReadOnlySpan<byte> data, int timestampYearOffset = 2000)
        {
            byte[] swappedData = data.ToArray().Select(x => x.SwapNibbles()).ToArray();

            byte year = swappedData[0].BcdToDecimal();
            byte month = swappedData[1].BcdToDecimal();
            byte day = swappedData[2].BcdToDecimal();
            byte hour = swappedData[3].BcdToDecimal();
            byte minute = swappedData[4].BcdToDecimal();
            byte second = swappedData[5].BcdToDecimal();
            byte offsetQuarters = ((byte)(swappedData[6] & 0b0111_1111)).BcdToDecimal();
            bool isOffsetPositive = (swappedData[6] & 0b1000_0000) == 0;

            DateTimeOffset timestamp = new DateTimeOffset(
                year + timestampYearOffset,
                month,
                day,
                hour,
                minute,
                second,
                TimeSpan.FromMinutes(offsetQuarters * 15)); // Offset in quarter of hours
            return timestamp;
        }
    }
}
