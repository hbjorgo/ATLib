using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class CommandEchoCommand
    {
        public static async Task<ATResult> EnableCommandEcho(this ICommunicator<string> comm, bool enable)
        {
            byte parameter = (byte)(enable ? 1 : 0);
            await comm.Write($"ATE{parameter}\r");
            var message = await comm.ReadSingleMessageAsync((byte)'\n');
            if (OkParser.TryParseNumeric(message, out OkResult okResult))
                return okResult;
            else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                return errorResult;
            return new UnknownResult();
        }
    }
}
