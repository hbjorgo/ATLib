using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using HeboTech.MessageReader;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class SignalQualityCommand
    {
        public static async ValueTask<ATResult<SignalQualityResult>> GetSignalQualityAsync(this ICommunicator<string> comm, CancellationToken cancellationToken = default)
        {
            await comm.Write("AT+CSQ\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
            if (SignalQualityParser.TryParse(message, ResponseFormat.Numeric, out ATResult<SignalQualityResult> signalQualityResult))
            {
                message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
                if (OkParser.TryParse(message, ResponseFormat.Numeric, out ATResult<OkResult> _))
                    return signalQualityResult;
                else if (ErrorParser.TryParse(message, ResponseFormat.Numeric, out ATResult<ErrorResult> errorResult))
                    return ATResult.Error<SignalQualityResult>(errorResult.ToString());
            }
            return ATResult.Error<SignalQualityResult>(Constants.PARSING_FAILED);
        }
    }
}
