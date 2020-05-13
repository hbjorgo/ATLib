using HeboTech.MessageReader;
using System.Text;
using System.Threading;

namespace HeboTech.ATLib.Commands
{
    public static class BatteryCommands
    {
        public static bool GetBatteryStatus(this ICommunicator<string> comm)
        {
            comm.Write($"AT+CBC\r\n");
            Thread.Sleep(2000);
            var message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            Thread.Sleep(1000);
            message = comm.GetResponse(Encoding.UTF8.GetBytes("\r\n"));
            return false;
        }
    }
}
