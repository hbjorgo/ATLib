using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class ModeCommands
    {
        public static async Task<ATResult> ReadModeAsync(this ICommunicator<string> comm)
        {
            await comm.Write($"AT+CMGF?\r");
            var message = await comm.ReadSingleMessageAsync((byte)'\r');
            if (OkParser.TryParseNumeric(message, out OkResult okResult))
                return okResult;
            else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                return errorResult;
            return new UnknownResult();
        }

        public static async Task<ATResult> SetModeAsync(this ICommunicator<string> comm, Mode mode)
        {
            await comm.Write($"AT+CMGF={(int)mode}\r");
            var message = await comm.ReadSingleMessageAsync((byte)'\r');
            if (OkParser.TryParseNumeric(message, out OkResult okResult))
                return okResult;
            else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                return errorResult;
            return new UnknownResult();
        }
    }
}
