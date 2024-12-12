using HeboTech.ATLib.Parsing;
using HeboTech.ATLib.Misc;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.SIMCOM
{
    public interface ISIM5320 : IModem
    {
        /// <summary>
        /// Get remaining PIN and PUK attempts
        /// </summary>
        /// <returns>Remaining PIN and PUK attempts</returns>
        Task<RemainingPinPukAttempts> GetRemainingPinPukAttemptsAsync();

        /// <summary>
        /// Reload and initialize the SIM card
        /// </summary>
        /// <returns>Command status</returns>
        Task<ModemResponse> ReInitializeSimAsync();
    }
}