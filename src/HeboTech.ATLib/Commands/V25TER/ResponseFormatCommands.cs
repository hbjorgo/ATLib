using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands.V25TER
{
    public static class ResponseFormatCommands
    {
        public static async ValueTask<ATResult<OkResult>> SetResponseFormatAsync(
            this ICommunicator<string> comm,
            ResponseFormat currentResponseFormat,
            ResponseFormat targetResponseFormat,
            CancellationToken cancellationToken = default)
        {
            await comm.Write($"ATV{targetResponseFormat}\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
            if (OkParser.TryParse(message, currentResponseFormat, out ATResult<OkResult> okResult))
                return okResult;
            else if (ErrorParser.TryParse(message, currentResponseFormat, out ATResult<ErrorResult> errorResult))
                return ATResult.Error<OkResult>(errorResult.ErrorMessage);
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}
