using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    public interface IGsm
    {
        Task<bool> InitializeAsync();
        Task<bool> SendSmsAsync(PhoneNumber phoneNumber, string message);
        Task<bool> SetModeAsync(Mode mode);
        Task<bool> UnlockSimAsync(Pin pin);
        Task<BatteryStatus> GetBatteryStatusAsync();
    }
}