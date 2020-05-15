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
            var message = await comm.ReadSingleMessageAsync((byte)'\n');
            if (BatteryStatusParser.TryParseNumeric(message, out BatteryStatusResult result))
                return result;
            return new UnknownResult();
        }
    }
}
