using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class BatteryCommands
    {
        public static async Task<ATResult> GetBatteryStatusAsync(this ICommunicator<string> comm)
        {
            await comm.Write($"AT+CBC\r\n");
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
            if (BatteryStatusParser.TryParseNumeric(message, out BatteryStatusResult batteryResult))
            {
                message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
                if (OkParser.TryParseNumeric(message, out OkResult _))
                    return batteryResult;
                else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                    return errorResult;
            }
            return new UnknownResult();
        }
    }
}
