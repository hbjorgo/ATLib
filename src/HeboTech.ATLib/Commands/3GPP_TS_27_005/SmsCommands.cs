using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands._3GPP_TS_27_005
{
    public static class SmsCommands // TODO: Make if equal to the other commands (ATResult<T>)
    {
        public static async ValueTask<ATResult<OkResult>> SendSmsAsync(
            this ICommunicator<string> comm,
            ResponseFormat responseFormat,
            PhoneNumber phoneNumber,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (message.Length > 160)
                throw new ArgumentOutOfRangeException($"Message exceeded maximum length of 160");

            await comm.Write($"AT+CMGS=\"{phoneNumber}\"\r", cancellationToken);
            Thread.Sleep(100);
            var response = await comm.ReadSingleMessageAsync((byte)'>', cancellationToken);
            await comm.Write($"{message}\r{0x1A}");
            Thread.Sleep(2000);
            response = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
            var result = OkParser.TryParse(response, responseFormat, out _);
            return default;
        }
    }
}
