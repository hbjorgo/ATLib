using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class PhoneNumberDtoTests
    {
        [Fact]
        internal void Sets_properties()
        {
            PhoneNumberDto sut = new("12345678", TypeOfNumber.National, NumberingPlanIdentification.ISDN);

            Assert.Equal("12345678", sut.Number);
            Assert.Equal(TypeOfNumber.National, sut.Ton);
            Assert.Equal(NumberingPlanIdentification.ISDN, sut.Npi);
        }
    }
}
