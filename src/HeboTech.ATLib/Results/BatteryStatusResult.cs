using System.ComponentModel;

namespace HeboTech.ATLib.Results
{
    public class BatteryStatusResult : ATResult
    {
        public BatteryChargeStatus Status { get; }
        public int ChargeLevel { get; }
        // Millivolt
        public int Voltage { get; }

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

        public BatteryStatusResult(BatteryChargeStatus status, int chargeLevel, int voltage)
        {
            Status = status;
            ChargeLevel = chargeLevel;
            Voltage = voltage;
        }

        public override string ToString()
        {
            return $"Charge Status: {Status.GetDescription()}, Charge Level: {ChargeLevel}%, Voltage: {Voltage }mV";
        }
    }
}
