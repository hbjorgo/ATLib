using HeboTech.ATLib.Extensions;
using System.ComponentModel;
using Xunit;

namespace HeboTech.ATLib.Tests.Extensions
{
    public class EnumExtensionsTests
    {
        public enum TestEnum
        {
            [Description("Description1")]
            Item1,
            [Description("Description2")]
            Item2
        }

        [Theory]
        [InlineData(TestEnum.Item1, "Description1")]
        [InlineData(TestEnum.Item2, "Description2")]
        public void GetDescription_returns_description(TestEnum testEnum, string expectedDescription)
        {
            string description = testEnum.GetDescription();
            Assert.Equal(expectedDescription, description);
        }
    }
}
