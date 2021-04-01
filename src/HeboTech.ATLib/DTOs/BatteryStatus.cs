using HeboTech.ATLib.Extensions;

namespace HeboTech.ATLib.DTOs
{
    public class BatteryStatus
    {
        public BatteryChargeStatus Status { get; }
        public int ChargeLevel { get; }
        public double? Voltage { get; }

        public BatteryStatus(BatteryChargeStatus status, int chargeLevel, double? voltage = null)
        {
            Status = status;
            ChargeLevel = chargeLevel;
            Voltage = voltage;
        }

        public override string ToString()
        {
            if (Voltage.HasValue)
                return $"Charge Status: {Status.GetDescription()}, Charge Level: {ChargeLevel}%, Voltage: {Voltage}V";
            return $"Charge Status: {Status.GetDescription()}, Charge Level: {ChargeLevel}%";
        }
    }
}
