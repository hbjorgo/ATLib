using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class PhoneNumberDtoTests
    {
        [Fact]
        internal void Sets_properties()
        {
            PhoneNumber sut = PhoneNumber.Create("12345678", TypeOfNumber.National, NumberingPlanIdentification.ISDN);

            Assert.Equal("12345678", sut.ToString());
            Assert.Equal(TypeOfNumber.National, sut.TypeOfNumber);
            Assert.Equal(NumberingPlanIdentification.ISDN, sut.NumberingPlanIdentification);
        }
    }
}
