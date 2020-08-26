using HeboTech.ATLib.Extensions;

namespace HeboTech.ATLib.Results
{
    public class BatteryStatus
    {
        public BatteryChargeStatus Status { get; }
        public int ChargeLevel { get; }
        public double Voltage { get; }

        public BatteryStatus(BatteryChargeStatus status, int chargeLevel, double voltage)
        {
            Status = status;
            ChargeLevel = chargeLevel;
            Voltage = voltage;
        }

        public override string ToString()
        {
            return $"Charge Status: {Status.GetDescription()}, Charge Level: {ChargeLevel}%, Voltage: {Voltage }V";
        }
    }
}
