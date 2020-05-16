using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using HeboTech.MessageReader;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class SendSmsCommand // TODO: Make if equal to the other commands (ATResult<T>)
    {
        public static async ValueTask<ATResult<OkResult>> SendSmsAsync(this ICommunicator<string> comm, PhoneNumber phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            if (message.Length > 160)
                throw new ArgumentOutOfRangeException($"Message exceeded maximum length of 160");

            await comm.Write($"AT+CMGS=\"{phoneNumber}\"\r", cancellationToken);
            Thread.Sleep(100);
            var response = await comm.ReadSingleMessageAsync((byte)'>', cancellationToken);
            await comm.Write($"{message}\r{0x1A}");
            Thread.Sleep(2000);
            response = await comm.ReadSingleMessageAsync((byte)'\r', cancellationToken);
            var result = OkParser.TryParse(response, ResponseFormat.Numeric, out _);
            return default;
        }
    }
}
