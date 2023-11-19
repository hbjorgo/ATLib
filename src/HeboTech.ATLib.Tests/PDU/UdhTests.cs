using HeboTech.ATLib.PDU;
using System;
using System.Linq;
using Xunit;

namespace HeboTech.ATLib.Tests.PDU
{
    public class UdhTests
    {
        [Fact]
        public void Empty_returns_empty_udh()
        {
            Udh result = Udh.Empty();

            Assert.Equal(0, result.Length);
            Assert.Equal(Array.Empty<InformationElement>(), result.InformationElements);
        }

        [Fact]
        public void Parse_returns_udh()
        {
            Udh result = Udh.Parse(5, new byte[] { 0x00, 0x03, 0x04, 0x02, 0x01 });

            Assert.Equal(5, result.Length);
            Assert.Single(result.InformationElements);
            Assert.Equal(0x00, result.InformationElements.First().IEI);
            Assert.Equal(0x03, result.InformationElements.First().Length);
            Assert.Equal(0x04, result.InformationElements.First().Data[0]);
            Assert.Equal(0x02, result.InformationElements.First().Data[1]);
            Assert.Equal(0x01, result.InformationElements.First().Data[2]);
        }
    }
}
