using HeboTech.ATLib.DTOs;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems.SIMCOM
{
    public interface ISIM5320 : IModem
    {
        Task<RemainingPinPukAttempts> GetRemainingPinPukAttemptsAsync();
    }
}