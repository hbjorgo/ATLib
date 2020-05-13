using HeboTech.ATLib.SuperPower.Parsers;
using HeboTech.MessageReader;
using System.Text;
using System.Threading;

namespace HeboTech.ATLib.Commands
{
    public static class ModeCommands
    {
        public static void ReadMode(this ICommunicator<string> comm)
        {
            comm.Write($"AT+CMGF?\r");
            Thread.Sleep(2000);
            var message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            var result = OkParser.TryParse(message, out _);
        }

        public static bool SetMode(this ICommunicator<string> comm, Mode mode)
        {
            comm.Write($"AT+CMGF={(int)mode}\r\n");
            Thread.Sleep(2000);
            var message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            var result = OkParser.TryParse(message, out _);
            return result;
        }
    }
}
