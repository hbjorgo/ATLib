using HeboTech.ATLib.DTOs;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class ValidityPeriodTests
    {
        [Fact]
        public void NotPresent_is_correct()
        {
            ValidityPeriod dut = ValidityPeriod.NotPresent();

            Assert.Equal(ValidityPeriodFormat.NotPresent, dut.Format);
            Assert.Equal(Array.Empty<byte>(), dut.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(127)]
        [InlineData(255)]
        public void Relative_is_correct(byte value)
        {
            ValidityPeriod dut = ValidityPeriod.Relative(value);

            Assert.Equal(ValidityPeriodFormat.Relative, dut.Format);
            Assert.Equal(new byte[] { value }, dut.Value);
        }

        [Theory]
        [InlineData("2013-03-25 23:01:56 -07:00", new byte[] { 31, 30, 52, 32, 10, 65, 138 })]
        public void Absolute_is_correct(string dateTimeString, byte[] value)
        {
            DateTimeOffset dateTime = DateTimeOffset.Parse(dateTimeString);
            ValidityPeriod dut = ValidityPeriod.Absolute(dateTime);

            Assert.Equal(ValidityPeriodFormat.Absolute, dut.Format);
            Assert.Equal(value, dut.Value);
        }
    }
}
