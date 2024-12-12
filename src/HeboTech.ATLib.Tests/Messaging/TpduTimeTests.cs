using HeboTech.ATLib.Messaging;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class TpduTimeTests
    {
        [Theory]
        [InlineData("2023.01.12 13:14:15 +02:00", new byte[] { 0x32, 0x10, 0x21, 0x31, 0x41, 0x51, 0x80 })]
        [InlineData("2023.01.12 13:14:15 -02:00", new byte[] { 0x32, 0x10, 0x21, 0x31, 0x41, 0x51, 0x88 })]
        public void EncodeTimestamp_returns_bytes_tests(string dateTime, byte[] expected)
        {
            DateTimeOffset timestamp = DateTimeOffset.Parse(dateTime);
            byte[] encoded = TpduTime.EncodeTimestamp(timestamp);

            Assert.Equal(expected, encoded);
        }

        [Theory]
        [InlineData(new byte[] { 0x32, 0x10, 0x21, 0x31, 0x41, 0x51, 0x80 }, "2023.01.12 13:14:15 +02:00")]
        [InlineData(new byte[] { 0x32, 0x10, 0x21, 0x31, 0x41, 0x51, 0x88 }, "2023.01.12 13:14:15 -02:00")]
        public void DecodeTimestamp_returns_DateTimeOffset_tests(byte[] bytes, string expected)
        {
            DateTimeOffset timestamp = TpduTime.DecodeTimestamp(bytes);

            DateTimeOffset expectedTimestamp = DateTimeOffset.Parse(expected);

            Assert.Equal(expectedTimestamp, timestamp);
        }
    }
}
