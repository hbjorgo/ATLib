using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class ModeCommands
    {
        public static async ValueTask<ATResult<OkResult>> ReadModeAsync(this ICommunicator<string> comm, CancellationToken cancellationToken = default)
        {
            await comm.Write($"AT+CMGF?\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
            if (OkParser.TryParse(message, ResponseFormat.Numeric, out ATResult<OkResult> okResult))
                return okResult;
            else if (ErrorParser.TryParse(message, ResponseFormat.Numeric, out ATResult<ErrorResult> errorResult))
                return ATResult.Error<OkResult>(errorResult.ErrorMessage);
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }

        public static async ValueTask<ATResult<OkResult>> SetModeAsync(this ICommunicator<string> comm, Mode mode, CancellationToken cancellationToken = default)
        {
            await comm.Write($"AT+CMGF={(int)mode}\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
            if (OkParser.TryParse(message, ResponseFormat.Numeric, out ATResult<OkResult> okResult))
                return okResult;
            else if (ErrorParser.TryParse(message, ResponseFormat.Numeric, out ATResult<ErrorResult> errorResult))
                return ATResult.Error<OkResult>(errorResult.ErrorMessage);
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}
