using HeboTech.ATLib.Communication;
using Pipelines.Sockets.Unofficial;
using System;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            using (SerialPort serialPort = new SerialPort("COM7", 9600, Parity.None, 8, StopBits.One))
            {
                Console.WriteLine("Opening serial port...");
                serialPort.Open();
                Console.WriteLine("Serialport opened");

                var stream = serialPort.BaseStream;

                IDuplexPipe duplexPipe = StreamConnection.GetDuplex(stream);
                ICommunicator comm = new Communicator(duplexPipe);

            }

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
