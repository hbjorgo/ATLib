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
            byte year = ((byte)(value.Year % 100)).SwapNibbles();
            byte month = ((byte)value.Month).SwapNibbles();
            byte day = ((byte)value.Day).SwapNibbles();
            byte hour = ((byte)value.Hour).SwapNibbles();
            byte minute = ((byte)value.Minute).SwapNibbles();
            byte second = ((byte)value.Second).SwapNibbles();

            byte timeZoneQuarters = ((byte)(Math.Abs(value.Offset.TotalMinutes) / 15)).SwapNibbles();
            if (value.Offset.TotalMinutes < 0)
                timeZoneQuarters |= 0b0000_1000;

            return new ValidityPeriod(ValidityPeriodFormat.Absolute, new byte[] { year, month, day, hour, minute, second, timeZoneQuarters });
        }
    }
}
