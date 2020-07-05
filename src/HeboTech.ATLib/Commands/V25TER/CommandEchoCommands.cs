using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands.V25TER
{
    public static class CommandEchoCommands
    {
        public static async ValueTask EnableCommandEchoAsync(
            this ICommunicator comm,
            bool enable,
            CancellationToken cancellationToken = default)
        {
            byte parameter = (byte)(enable ? 1 : 0);
            await comm.Write($"ATE{parameter}\r", cancellationToken);
        }

        public static async ValueTask<ATResult<OkResult>> EnableCommandEchoAsync(
            this ICommunicator comm,
            ResponseFormat responseFormat,
            bool enable,
            CancellationToken cancellationToken = default)
        {
            byte parameter = (byte)(enable ? 1 : 0);
            await comm.Write($"ATE{parameter}\r", cancellationToken);
            var message = await comm.ReadLineAsync(cancellationToken);
            if (OkParser.TryParse(message, responseFormat, out ATResult<OkResult> okResult))
                return okResult;
            else if (ErrorParser.TryParse(message, responseFormat, out ATResult<ErrorResult> errorResult))
                return ATResult.Error<OkResult>(errorResult.ErrorMessage);
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}
