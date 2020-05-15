using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using HeboTech.MessageReader;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Commands
{
    public static class SendSmsCommand
    {
        public static async Task<ATResult> SendSmsAsync(this ICommunicator<string> comm, PhoneNumber phoneNumber, string message)
        {
            if (message.Length > 160)
                throw new ArgumentOutOfRangeException($"Message exceeded maximum length of 160");

            await comm.Write($"AT+CMGS=\"{phoneNumber}\"\r");
            Thread.Sleep(100);
            var response = await comm.ReadSingleMessageAsync((byte)'>');
            await comm.Write($"{message}\r{0x1A}");
            Thread.Sleep(2000);
            response = await comm.ReadSingleMessageAsync((byte)'\r');
            var result = OkParser.TryParseNumeric(response, out _);
            return default;
        }
    }
}
