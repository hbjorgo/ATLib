namespace HeboTech.ATLib
{
    public enum BatteryChargeStatus
    {
        PoweredByBattery = 0,
        BatteryConnectedAndCharging = 1,
        NoBatteryConnected = 2,
        PowerFault = 3
    }

    public class BatteryStatus
    {
        public BatteryStatus(BatteryChargeStatus chargeStatus, double chargeLevel)
        {
            this.ChargeStatus = chargeStatus;
            this.ChargeLevel = chargeLevel;
        }

        public BatteryChargeStatus ChargeStatus { get; }
        public double ChargeLevel { get; }
    }
}
