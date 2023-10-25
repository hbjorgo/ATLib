using HeboTech.ATLib.Extensions;
using System;

namespace HeboTech.ATLib.DTOs
{
    public enum ValidityPeriodFormat : byte
    {
        NotPresent = 0x00,
        Enhanced = 0x01,
        Relative = 0x02,
        Absolute = 0x03
    }

    public class ValidityPeriod
    {
        private ValidityPeriod(ValidityPeriodFormat format, byte[] value)
        {
            Format = format;
            Value = value;
        }

        public ValidityPeriodFormat Format { get; }
        public byte[] Value { get; }

        public static ValidityPeriod NotPresent() => new ValidityPeriod(ValidityPeriodFormat.NotPresent, Array.Empty<byte>());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">
        /// Value 0-143: (Value + 1) x 5 minutes. Possible values: 5, 10, 15 minutes ... 11:55, 12:00 hours
        /// Value 144-167: (12 + (Value - 143) / 2) hours. Possible values: 12:30, 13:00, ... 23:30, 24:00 hours
        /// Value 168-196: (Value - 166) days. Possible values: 2, 3, 4, ... 30 days
        /// Value 197-255: (Value - 192) weeks. Possible values: 5, 6, 7, ... 63 weeks
        /// </param>
        /// <returns></returns>
        public static ValidityPeriod Relative(byte value) => new ValidityPeriod(ValidityPeriodFormat.Relative, new byte[] { value });

        public static ValidityPeriod Absolute(DateTimeOffset value)
        {
            byte year = SwapDecimalDigits((byte)(value.Year % 100));
            byte month = SwapDecimalDigits((byte)value.Month);
            byte day = SwapDecimalDigits((byte)value.Day);
            byte hour = SwapDecimalDigits((byte)value.Hour);
            byte minute = SwapDecimalDigits((byte)value.Minute);
            byte second = SwapDecimalDigits((byte)value.Second);

            byte timeZoneQuarters = SwapNibbles((byte)(Math.Abs(value.Offset.TotalMinutes) / 15));
            if (value.Offset.TotalMinutes < 0)
                timeZoneQuarters |= 0b0000_1000;

            return new ValidityPeriod(ValidityPeriodFormat.Absolute, new byte[] { year, month, day, hour, minute, second, timeZoneQuarters });
        }

        private static byte SwapDecimalDigits(byte value)
        {
            return (byte)(((value % 10) * 10) + (value / 10));
        }

        private static byte SwapNibbles(byte x)
        {
            return (byte)((x & 0x0F) << 4 | (x & 0xF0) >> 4);
        }
    }
}
