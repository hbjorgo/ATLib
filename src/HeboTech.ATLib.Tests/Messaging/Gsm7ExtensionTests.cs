using HeboTech.ATLib.Messaging;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class Gsm7ExtensionTests
    {
        [Theory]
        [InlineData(Gsm7Extension.Default, 0x00)]
        [InlineData(Gsm7Extension.Turkish, 0x01)]
        [InlineData(Gsm7Extension.Spanish, 0x02)]
        [InlineData(Gsm7Extension.Portugese, 0x03)]
        [InlineData(Gsm7Extension.BengaliAndAssamese, 0x04)]
        [InlineData(Gsm7Extension.Gujarati, 0x05)]
        [InlineData(Gsm7Extension.Hindi, 0x06)]
        [InlineData(Gsm7Extension.Kannada, 0x07)]
        [InlineData(Gsm7Extension.Malayalam, 0x08)]
        [InlineData(Gsm7Extension.Oriya, 0x09)]
        [InlineData(Gsm7Extension.Punjabi, 0x0A)]
        [InlineData(Gsm7Extension.Tamil, 0x0B)]
        [InlineData(Gsm7Extension.Telugu, 0x0C)]
        [InlineData(Gsm7Extension.Urdu, 0x0D)]
        internal void Has_correct_values(Gsm7Extension gsm7Extension, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)gsm7Extension);
        }
    }
}
