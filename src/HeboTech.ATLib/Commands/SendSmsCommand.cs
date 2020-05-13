using HeboTech.ATLib.SuperPower.Parsers;
using HeboTech.MessageReader;
using System;
using System.Text;
using System.Threading;

namespace HeboTech.ATLib.Commands
{
    public static class SendSmsCommand
    {
        public static bool SendSms(this ICommunicator<string> comm, PhoneNumber phoneNumber, string message)
        {
            if (message.Length > 160)
                throw new ArgumentOutOfRangeException($"Message exceeded maximum length of 160");

            comm.Write($"AT+CMGS=\"+47{phoneNumber}\"\r");
            Thread.Sleep(4000);
            var response = comm.GetResponse(Encoding.UTF8.GetBytes(">"));
            comm.Write($"{message}{0x1A}");
            Thread.Sleep(4000);
            response = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            var result = OkParser.TryParse(response, out _);
            return result;
        }
    }
}
