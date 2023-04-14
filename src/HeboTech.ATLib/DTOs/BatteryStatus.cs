using HeboTech.ATLib.Extensions;
using UnitsNet;

namespace HeboTech.ATLib.DTOs
{
    public class BatteryStatus
    {
        public BatteryChargeStatus Status { get; }
        public Ratio ChargeLevel { get; }
        public ElectricPotential? Voltage { get; }

        public BatteryStatus(BatteryChargeStatus status, Ratio chargeLevel, ElectricPotential? voltage = null)
        {
            Status = status;
            ChargeLevel = chargeLevel;
            Voltage = voltage;
        }

        public override string ToString()
        {
            if (Voltage.HasValue)
                return $"Charge Status: {Status.GetDescription()}, Charge Level: {ChargeLevel}, Voltage: {Voltage}";
            return $"Charge Status: {Status.GetDescription()}, Charge Level: {ChargeLevel}";
        }
    }
}
