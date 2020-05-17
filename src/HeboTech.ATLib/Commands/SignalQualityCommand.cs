using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class SignalQualityCommand
    {
        public static async ValueTask<ATResult<SignalQualityResult>> GetSignalQualityAsync(
            this ICommunicator<string> comm,
            ResponseFormat responseFormat,
            CancellationToken cancellationToken = default)
        {
            await comm.Write("AT+CSQ\r", cancellationToken);
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
            if (SignalQualityParser.TryParse(message, responseFormat, out ATResult<SignalQualityResult> signalQualityResult))
            {
                message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF, cancellationToken);
                if (OkParser.TryParse(message, responseFormat, out ATResult<OkResult> _))
                    return signalQualityResult;
                else if (ErrorParser.TryParse(message, responseFormat, out ATResult<ErrorResult> errorResult))
                    return ATResult.Error<SignalQualityResult>(errorResult.ToString());
            }
            return ATResult.Error<SignalQualityResult>(Constants.PARSING_FAILED);
        }
    }
}
