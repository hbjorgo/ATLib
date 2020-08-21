using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Parsers;
using System;
using System.IO.Ports;
using System.Threading;
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
                serialPort.ReadTimeout = 60_000;
                Console.WriteLine("Opening serial port...");
                serialPort.Open();
                Console.WriteLine("Serialport opened");

                ICommunicator comm = new SerialPortCommunicator(serialPort);

                AdafruitFona modem = new AdafruitFona(comm);
                var simStatus = modem.GetSimStatus();
                Console.WriteLine($"SIM Status: {simStatus}");

                modem.GetSignalStrength();
                modem.GetBatteryStatus();

                modem.Close();

                Thread.Sleep(1000);
            }

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
