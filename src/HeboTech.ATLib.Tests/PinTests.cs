using System;
using Xunit;

namespace HeboTech.ATLib.Tests
{
    public class PinTests
    {
        [Fact]
        public void ToStringReturnsCorrectStringTest()
        {
            Pin pin = new Pin(1, 2, 3, 4);
            Assert.Equal("1234", pin.ToString());
        }

        [Fact]
        public void ThrowsExceptionIfDigit0IsNegativeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(-1, 2, 3, 4));
        }

        [Fact]
        public void ThrowsExceptionIfDigit1IsNegativeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(1, -2, 3, 4));
        }

        [Fact]
        public void ThrowsExceptionIfDigit2IsNegativeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(1, 2, -3, 4));
        }

        [Fact]
        public void ThrowsExceptionIfDigit3IsNegativeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(1, 2, 3, -4));
        }

        [Fact]
        public void ThrowsExceptionIfDigit0IsGreaterThan9Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(10, 2, 3, 4));
        }

        [Fact]
        public void ThrowsExceptionIfDigit1IsGreaterThan9Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(1, 20, 3, 4));
        }

        [Fact]
        public void ThrowsExceptionIfDigit2IsGreaterThan9Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(1, 2, 30, 4));
        }

        [Fact]
        public void ThrowsExceptionIfDigit3IsGreaterThan9Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pin(1, 2, 3, 40));
        }

        [Fact]
        public void PinAsStringIsSet()
        {
            Pin pin = new Pin("1234");
            Assert.Equal("1234", pin.ToString());
        }

        [Fact]
        public void PinAsStringIsTooShort()
        {
            Assert.Throws<ArgumentException>(() =>
                new Pin("123")
            );
        }

        [Fact]
        public void PinAsStringIsTooLong()
        {
            Assert.Throws<ArgumentException>(() =>
                new Pin("12345")
            );
        }

        [Fact]
        public void PinAsStringHasInvalidCharacters()
        {
            Assert.Throws<ArgumentException>(() =>
                new Pin("12a4")
            );
        }
    }
}
