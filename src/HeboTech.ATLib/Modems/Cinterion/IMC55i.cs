using HeboTech.ATLib.Parsers;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Cinterion
{
    public interface IMC55i : IModem
    {
        Task<ModemResponse<MC55iBatteryStatus>> MC55i_GetBatteryStatusAsync();
    }
}