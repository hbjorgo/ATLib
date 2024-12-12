using System.ComponentModel;

namespace HeboTech.ATLib.Misc
{
    public enum BatteryChargeStatus : byte
    {
        [Description("Powered by battery")]
        PoweredByBattery = 0,
        [Description("Charging")]
        Charging = 1,
        [Description("Charging finished")]
        ChargingFinished = 2,
        [Description("Power fault")]
        PowerFault = 3
    }
}
