using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class CommandEchoCommand
    {
        public static async ValueTask<ATResult<OkResult>> EnableCommandEchoAsync(
            this ICommunicator<string> comm,
            ResponseFormat responseFormat,
            bool enable,
            CancellationToken cancellationToken = default)
        {
            byte parameter = (byte)(enable ? 1 : 0);
            await comm.Write($"ATE{parameter}\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync((byte)'\n', cancellationToken);
            if (OkParser.TryParse(message, responseFormat, out ATResult<OkResult> okResult))
                return okResult;
            else if (ErrorParser.TryParse(message, responseFormat, out ATResult<ErrorResult> errorResult))
                return ATResult.Error<OkResult>(errorResult.ErrorMessage);
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}
