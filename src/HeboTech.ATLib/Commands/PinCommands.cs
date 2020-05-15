using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class PinCommands
    {
        public static async Task<ATResult> GetPinStatus(this ICommunicator<string> comm)
        {
            await comm.Write($"AT+CPIN?\r");
            var message = await comm.ReadSingleMessageAsync((byte)'\n');
            if (PinStatusParser.TryParseNumeric(message, out PinStatusResult pinResult))
            {
                message = await comm.ReadSingleMessageAsync((byte)'\n');
                if (OkParser.TryParseNumeric(message, out OkResult okResult))
                    return pinResult;
                else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                    return errorResult;
            }
            else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                return errorResult;
            return default;
        }
    }
}
