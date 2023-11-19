using HeboTech.ATLib.Parsers;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.Cinterion
{
    public interface IMC55i : IModem
    {
        /// <summary>
        /// Get the current battery status
        /// </summary>
        /// <returns>Command status with battery status</returns>
        Task<ModemResponse<MC55iBatteryStatus>> MC55i_GetBatteryStatusAsync();
        
        /// <summary>
        /// Indicate SIM data ready URC
        /// </summary>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>Command status</returns>
        //Task<ModemResponse> IndicateSimDataReady(bool enable);
    }
}