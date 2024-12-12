using HeboTech.ATLib.Numbering;
using Xunit;

namespace HeboTech.ATLib.Tests.Numbering
{
    public class NumberplanIdentificationTests
    {
        [Theory]
        [InlineData(NumberPlanIdentification.Unknown, 0x00)]
        [InlineData(NumberPlanIdentification.ISDN, 0x01)]
        [InlineData(NumberPlanIdentification.DataNumbering, 0x03)]
        [InlineData(NumberPlanIdentification.Telex, 0x04)]
        [InlineData(NumberPlanIdentification.ServiceCentreSpecific1, 0x05)]
        [InlineData(NumberPlanIdentification.ServiceCentreSpecific2, 0x06)]
        [InlineData(NumberPlanIdentification.NationalNumbering, 0x08)]
        [InlineData(NumberPlanIdentification.PrivateNumbering, 0x09)]
        [InlineData(NumberPlanIdentification.ErmesNumbering, 0x0A)]
        [InlineData(NumberPlanIdentification.ReservedForExtension, 0x0F)]
        internal void Has_correct_values(NumberPlanIdentification numberplanIdentification, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)numberplanIdentification);
        }
    }
}
