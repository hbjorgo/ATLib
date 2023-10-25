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
        public static ValidityPeriod Relative(byte value) => new ValidityPeriod(ValidityPeriodFormat.Relative, new byte[] { value });
    }
}
