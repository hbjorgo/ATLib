using HeboTech.ATLib.SuperPower.Parsers;
using HeboTech.MessageReader;
using System.Text;
using System.Threading;

namespace HeboTech.ATLib.Commands
{
    public static class InitializeCommand
    {
        public static bool Initialize(this ICommunicator<string> comm)
        {
            comm.Write("AT\r\n");
            Thread.Sleep(2000);
            var message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            Thread.Sleep(1000);
            message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            var result = OkParser.TryParse(message, out _);
            return result;
        }
    }
}
