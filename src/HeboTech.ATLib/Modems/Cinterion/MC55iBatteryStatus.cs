using UnitsNet;

namespace HeboTech.ATLib.Modems.Cinterion
{
    public class MC55iBatteryStatus
    {
        public MC55iBatteryStatus(ElectricCurrent powerConsumption)
        {
            PowerConsumption = powerConsumption;
        }

        public ElectricCurrent PowerConsumption { get; }

        public override string ToString()
        {
            return $"Average power consumption: {PowerConsumption}";
        }
    }
}
