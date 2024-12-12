using HeboTech.ATLib.Extensions;
using HeboTech.ATLib.Sys;
using Xunit;

namespace HeboTech.ATLib.Tests.System
{
    public class BatteryChargeStatusTests
    {
        [Theory]
        [InlineData(BatteryChargeStatus.PoweredByBattery, 0)]
        [InlineData(BatteryChargeStatus.Charging, 1)]
        [InlineData(BatteryChargeStatus.ChargingFinished, 2)]
        [InlineData(BatteryChargeStatus.PowerFault, 3)]
        internal void Has_correct_values(BatteryChargeStatus batteryChargeStatus, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)batteryChargeStatus);
        }

        [Theory]
        [InlineData(BatteryChargeStatus.PoweredByBattery, "Powered by battery")]
        [InlineData(BatteryChargeStatus.Charging, "Charging")]
        [InlineData(BatteryChargeStatus.ChargingFinished, "Charging finished")]
        [InlineData(BatteryChargeStatus.PowerFault, "Power fault")]
        internal void Has_correct_descriptions(BatteryChargeStatus batteryChargeStatus, string expectedValue)
        {
            Assert.Equal(expectedValue, batteryChargeStatus.GetDescription());
        }
    }
}
