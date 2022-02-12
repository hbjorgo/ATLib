using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Parsers;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems
{
    public static class ModemExtensions
    {
        public static async Task<bool> SetRequiredSettingsAsync(this IModem modem)
        {
            ModemResponse echo = await modem.DisableEchoAsync();
            ModemResponse errorFormat = await modem.SetErrorFormat(1);
            ModemResponse smsTextFormatResult = await modem.SetSmsMessageFormatAsync(SmsTextFormat.Text);
            return echo.IsSuccess && errorFormat.IsSuccess && smsTextFormatResult.IsSuccess;
        }
    }
}
