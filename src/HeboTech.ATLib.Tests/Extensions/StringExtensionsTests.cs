﻿using HeboTech.ATLib.Extensions;
using Xunit;

namespace HeboTech.ATLib.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("00", new byte[] { 0x00 })]
        [InlineData("01", new byte[] { 0x01 })]
        [InlineData("0001", new byte[] { 0x00, 0x01 })]
        [InlineData("000102030405060708090A0B0C0D0E0F1011", new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11 })]
        public static void ToByteArray_tests(string hexString, byte[] expected)
        {
            Assert.Equal(expected, hexString.ToByteArray());
        }
    }
}
