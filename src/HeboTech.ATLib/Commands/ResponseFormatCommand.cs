using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using HeboTech.MessageReader;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class ResponseFormatCommand
    {
        public static async Task<ATResult> SetResponseFormatAsync(this ICommunicator<string> comm, ResponseFormat format)
        {
            await comm.Write($"ATV{format}\r");
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
            if (OkParser.TryParseNumeric(message, out OkResult okResult))
                return okResult;
            else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                return errorResult;
            return new UnknownResult();
        }
    }
}
