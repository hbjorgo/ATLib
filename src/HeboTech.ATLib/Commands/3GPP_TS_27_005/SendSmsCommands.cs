using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands._3GPP_TS_27_005
{
    public static class SendSmsCommands
    {
        public static ValueTask<ATResult<SmsSentResult>> SendSmsAsync(
            this ICommunicator<string> comm,
            ResponseFormat responseFormat,
            PhoneNumber phoneNumber,
            SmsMessage message,
            CancellationToken cancellationToken = default)
        {
            var resultTask = message.Mode switch
            {
                Mode.Text => SendSmsAsTextAsync(comm, responseFormat, phoneNumber, message, cancellationToken),
                Mode.PDU => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
            };
            return resultTask;
        }

        private static async ValueTask<ATResult<SmsSentResult>> SendSmsAsTextAsync(
            ICommunicator<string> comm,
            ResponseFormat responseFormat,
            PhoneNumber phoneNumber,
            SmsMessage smsMessage,
            CancellationToken cancellationToken)
        {
            await comm.Write($"AT+CMGS=\"{phoneNumber}\",{(byte)phoneNumber.Format}\r", cancellationToken);

            var message = await comm.ReadSingleMessageAsync(new byte[] { (byte)'>', (byte)'\r' }, cancellationToken);
            switch (responseFormat)
            {
                case ResponseFormat.Numeric:
                    if (Regex.Match(message, ">").Success)
                    {
                        await comm.Write($"{smsMessage}\r{0x1A}");

                        message = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
                        Match match;
                        if ((match = Regex.Match(message, @"\r\n\+CMGS: (?<mr>\d+)\r\n")).Success)
                            return ATResult.Value(new SmsSentResult(int.Parse(match.Groups["mr"].Value)));
                        else if (Regex.Match(message, Regexes.Numeric.ERROR).Success)
                            return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    }
                    else if (Regex.Match(message, Regexes.Numeric.ERROR).Success)
                        return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    break;
                case ResponseFormat.Verbose:
                    if(Regex.Match(message, ">").Success)
                    {
                        await comm.Write($"{smsMessage}\r{0x1A}");

                        message = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
                        Match match;
                        if ((match = Regex.Match(message, @"\+CMGS: (?<mr>\d+)\r\n")).Success)
                            return ATResult.Value(new SmsSentResult(int.Parse(match.Groups["mr"].Value)));
                        else if (Regex.Match(message, Regexes.Verbose.ERROR).Success)
                            return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    }
                    else if (Regex.Match(message, Regexes.Verbose.ERROR).Success)
                        return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    break;
            }


            await comm.Write($"{smsMessage}\r{0x1A}");
            message = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
            if (SmsStatusParser.TryParse(message, responseFormat, out ATResult<SmsSentResult> result))
                return result;
            return default;
        }

        public static async ValueTask<ATResult<OkResult>> TestSendSmsAsync(
            this ICommunicator<string> comm,
            ResponseFormat responseFormat,
            CancellationToken cancellationToken = default)
        {
            await comm.Write($"AT+CMGS=?\r", cancellationToken);
            var response = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
            if (OkParser.TryParse(response, responseFormat, out ATResult<OkResult> result))
                return result;
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}