using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class InitializeCommand
    {
        public static async Task<ATResult> InitializeAsync(this ICommunicator<string> comm)
        {
            await comm.Write("AT\r");
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
            if (OkParser.TryParseNumeric(message, out OkResult okResult))
                return okResult;
            else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                return errorResult;
            return new UnknownResult();
        }
    }
}
