using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class InitializationCommands
    {
        public static async ValueTask<ATResult<OkResult>> InitializeAsync(
            this ICommunicator comm,
            ResponseFormat responseFormat,
            CancellationToken cancellationToken = default)
        {
            await comm.Write("AT\r", cancellationToken);

            var message = await comm.ReadLineAsync(cancellationToken);
            switch (responseFormat)
            {
                case ResponseFormat.Numeric:
                    if (Regex.Match(message, Regexes.Numeric.OK).Success)
                        return ATResult.Value(new OkResult());
                    else if (Regex.Match(message, Regexes.Numeric.ERROR).Success)
                        return ATResult.Error<OkResult>(Constants.ERROR);
                    break;
                case ResponseFormat.Verbose:
                    if (Regex.Match(message, Regexes.Verbose.OK).Success)
                        return ATResult.Value(new OkResult());
                    else if (Regex.Match(message, Regexes.Verbose.ERROR).Success)
                        return ATResult.Error<OkResult>(Constants.ERROR);
                    break;
            }
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}
