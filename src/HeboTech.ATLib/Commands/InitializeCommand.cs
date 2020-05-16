using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using HeboTech.MessageReader;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class InitializeCommand
    {
        public static async ValueTask<ATResult<OkResult>> InitializeAsync(this ICommunicator<string> comm, CancellationToken cancellationToken = default)
        {
            await comm.Write("AT\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
            if (OkParser.TryParse(message, ResponseFormat.Numeric, out ATResult<OkResult> okResult))
                return okResult;
            else if (ErrorParser.TryParse(message, ResponseFormat.Numeric, out ATResult<ErrorResult> errorResult))
                return ATResult.Error<OkResult>(errorResult.ErrorMessage);
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}
