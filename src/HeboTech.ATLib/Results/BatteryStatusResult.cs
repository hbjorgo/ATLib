using HeboTech.ATLib.States;

namespace HeboTech.ATLib.Results
{
    public class BatteryStatusResult : ATResult
    {
        public BatteryChargeStatus Status { get; }
        public int ChargeLevel { get; }
        // Millivolt
        public int Voltage { get; }

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
