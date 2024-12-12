using HeboTech.ATLib.Sys;
using UnitsNet;
using Xunit;

namespace HeboTech.ATLib.Tests.System
{
    public class BatteryStatusTests
    {
        [Fact]
        internal void Sets_properties()
        {
            BatteryStatus sut = new(BatteryChargeStatus.Charging, Ratio.FromPercent(69));

            Assert.Equal(BatteryChargeStatus.Charging, sut.Status);
            Assert.Equal(Ratio.FromPercent(69), sut.ChargeLevel);
            Assert.Null(sut.Voltage);
        }

        [Fact]
        internal void Sets_properties_including_voltage()
        {
            BatteryStatus sut = new(BatteryChargeStatus.Charging, Ratio.FromPercent(69), ElectricPotential.FromVolts(2.7));

            Assert.Equal(BatteryChargeStatus.Charging, sut.Status);
            Assert.Equal(Ratio.FromPercent(69), sut.ChargeLevel);
            Assert.Equal(ElectricPotential.FromVolts(2.7), sut.Voltage);
        }
    }
}
