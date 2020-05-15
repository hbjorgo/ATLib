using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System.Threading.Tasks;
namespace HeboTech.ATLib.Commands
{
    public static class SignalQualityCommand
    {
        public static async Task<ATResult> GetSignalQuality(this ICommunicator<string> comm)
        {
            await comm.Write("AT+CSQ\r");
            var message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
            if (SignalQualityParser.TryParseNumeric(message, out SignalQualityResult batteryResult))
            {
                message = await comm.ReadSingleMessageAsync(Constants.BYTE_LF);
                if (OkParser.TryParseNumeric(message, out OkResult _))
                    return batteryResult;
                else if (ErrorParser.TryParseNumeric(message, out ErrorResult errorResult))
                    return errorResult;
            }
            return new UnknownResult();
        }
    }
}
