using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class NumberingPlanIdentificationTests
    {
        [Theory]
        [InlineData(NumberingPlanIdentification.Unknown, 0x00)]
        [InlineData(NumberingPlanIdentification.ISDN, 0x01)]
        [InlineData(NumberingPlanIdentification.DataNumbering, 0x03)]
        [InlineData(NumberingPlanIdentification.Telex, 0x04)]
        [InlineData(NumberingPlanIdentification.ServiceCentreSpecific1, 0x05)]
        [InlineData(NumberingPlanIdentification.ServiceCentreSpecific2, 0x06)]
        [InlineData(NumberingPlanIdentification.NationalNumbering, 0x08)]
        [InlineData(NumberingPlanIdentification.PrivateNumbering, 0x09)]
        [InlineData(NumberingPlanIdentification.ErmesNumbering, 0x0A)]
        [InlineData(NumberingPlanIdentification.ReservedForExtension, 0x0F)]
        internal void Has_correct_value(NumberingPlanIdentification numberplanIdentification, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)numberplanIdentification);
        }
    }
}
