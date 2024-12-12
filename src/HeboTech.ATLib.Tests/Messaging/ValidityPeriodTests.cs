using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Messaging;
using System;
using System.Linq;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
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
        [InlineData("2013-03-25 23:01:56 +07:00", new byte[] { 0x31, 0x30, 0x52, 0x32, 0x10, 0x65, 0x82 })]
        public void Absolute_with_positive_timezone_is_correct(string dateTimeString, byte[] value)
        {
            DateTimeOffset dateTime = DateTimeOffset.Parse(dateTimeString);
            ValidityPeriod dut = ValidityPeriod.Absolute(dateTime);

            Assert.Equal(ValidityPeriodFormat.Absolute, dut.Format);
            Assert.Equal(value, dut.Value);
        }

        [Theory]
        [InlineData("2013-03-25 23:01:56 -07:00", new byte[] { 0x31, 0x30, 0x52, 0x32, 0x10, 0x65, 0x8A })]
        public void Absolute_with_negative_timezone_is_correct(string dateTimeString, byte[] value)
        {
            DateTimeOffset dateTime = DateTimeOffset.Parse(dateTimeString);
            ValidityPeriod dut = ValidityPeriod.Absolute(dateTime);

            Assert.Equal(ValidityPeriodFormat.Absolute, dut.Format);
            Assert.Equal(value, dut.Value);
        }

        [Theory]
        [InlineData("2013-03-25 23:01:56 +07:00", "31305232106582")]
        public void Absolute_with_positive_timezone_is_correct_as_string(string dateTimeString, string value)
        {
            DateTimeOffset dateTime = DateTimeOffset.Parse(dateTimeString);
            ValidityPeriod dut = ValidityPeriod.Absolute(dateTime);

            Assert.Equal(ValidityPeriodFormat.Absolute, dut.Format);
            Assert.Equal(value, string.Join("", dut.Value.Select(x => x.ToString("X2"))));
        }

        [Theory]
        [InlineData("2013-03-25 23:01:56 -07:00", "3130523210658A")]
        public void Absolute_with_negative_timezone_is_correct_as_string(string dateTimeString, string value)
        {
            DateTimeOffset dateTime = DateTimeOffset.Parse(dateTimeString);
            ValidityPeriod dut = ValidityPeriod.Absolute(dateTime);

            Assert.Equal(ValidityPeriodFormat.Absolute, dut.Format);
            Assert.Equal(value, string.Join("", dut.Value.Select(x => x.ToString("X2"))));
        }
    }
}
