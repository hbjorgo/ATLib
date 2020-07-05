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
        public static Task<ATResult<SmsSentResult>> SendSmsAsync(
            this ICommunicator comm,
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

        private static async Task<ATResult<SmsSentResult>> SendSmsAsTextAsync(
            ICommunicator comm,
            ResponseFormat responseFormat,
            PhoneNumber phoneNumber,
            SmsMessage smsMessage,
            CancellationToken cancellationToken)
        {
            await comm.Write($"AT+CMGS=\"{phoneNumber}\",{(byte)phoneNumber.Format}\r", cancellationToken);
            var message = await comm.ReadLineAsync(new byte[] { (byte)'>' }, cancellationToken);
            switch (responseFormat)
            {
                case ResponseFormat.Numeric:
                    if (Regex.Match(message, "\r\n").Success)
                    {
                        await comm.Write($"{smsMessage}\x1A");

                        await comm.ReadLineAsync(cancellationToken);
                        message = await comm.ReadLineAsync(cancellationToken);
                        Match match;
                        if ((match = Regex.Match(message, @"\+CMGS: (?<mr>\d+)")).Success)
                            return ATResult.Value(new SmsSentResult(int.Parse(match.Groups["mr"].Value)));
                        else if (Regex.Match(message, Regexes.Numeric.ERROR).Success)
                            return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    }
                    else if (Regex.Match(message, Regexes.Numeric.ERROR).Success)
                        return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    break;
                case ResponseFormat.Verbose:
                    if (Regex.Match(message, "\r\n").Success)
                    {
                        await comm.Write($"{smsMessage}\x1A");

                        message = await comm.ReadLineAsync(cancellationToken);

                        Match match;
                        if ((match = Regex.Match(message, @"\+CMGS: (?<mr>\d+)")).Success)
                            return ATResult.Value(new SmsSentResult(int.Parse(match.Groups["mr"].Value)));
                        else if (Regex.Match(message, Regexes.Verbose.ERROR).Success)
                            return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    }
                    else if (Regex.Match(message, Regexes.Verbose.ERROR).Success)
                        return ATResult.Error<SmsSentResult>(Constants.ERROR);
                    break;
            }


            await comm.Write($"{smsMessage}\r{0x1A}");
            message = await comm.ReadLineAsync(cancellationToken);
            if (SmsStatusParser.TryParse(message, responseFormat, out ATResult<SmsSentResult> result))
                return result;
            return default;
        }

        public static async Task<ATResult<OkResult>> TestSendSmsAsync(
            this ICommunicator comm,
            ResponseFormat responseFormat,
            CancellationToken cancellationToken = default)
        {
            await comm.Write($"AT+CMGS=?\r", cancellationToken);
            var response = await comm.ReadLineAsync(cancellationToken);
            if (OkParser.TryParse(response, responseFormat, out ATResult<OkResult> result))
                return result;
            return ATResult.Error<OkResult>(Constants.PARSING_FAILED);
        }
    }
}