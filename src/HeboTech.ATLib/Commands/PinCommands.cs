using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using HeboTech.MessageReader;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class PinCommands
    {
        public static async ValueTask<ATResult<PinStatusResult>> GetPinStatusAsync(this ICommunicator<string> comm, CancellationToken cancellationToken = default)
        {
            await comm.Write($"AT+CPIN?\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
            if (PinStatusParser.TryParse(message, ResponseFormat.Numeric, out ATResult<PinStatusResult> pinResult))
            {
                message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
                if (OkParser.TryParse(message, ResponseFormat.Numeric, out ATResult<OkResult> _))
                    return pinResult;
                else if (ErrorParser.TryParse(message, ResponseFormat.Numeric, out ATResult<ErrorResult> errorResult))
                    return ATResult.Error<PinStatusResult>(errorResult.ErrorMessage);
            }
            else if (ErrorParser.TryParse(message, ResponseFormat.Numeric, out ATResult<ErrorResult> errorResult))
                return ATResult.Error<PinStatusResult>(errorResult.ErrorMessage);
            return ATResult.Error<PinStatusResult>(Constants.PARSING_FAILED);
        }
    }
}
