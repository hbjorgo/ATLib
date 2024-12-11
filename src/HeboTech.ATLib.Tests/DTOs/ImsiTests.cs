using HeboTech.ATLib.DTOs;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class ImsiTests
    {
        [Fact]
        public void Sets_properties()
        {
            Imsi sut = new("123451234512345");

            Assert.Equal("123451234512345", sut.Value);
        }
    }
}
