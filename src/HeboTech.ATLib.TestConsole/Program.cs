using HeboTech.ATLib.Commands;
using HeboTech.ATLib.Pipelines;
using HeboTech.MessageReader;
using Pipelines.Sockets.Unofficial;
using System;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (SerialPort serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One))
            {
                Console.WriteLine("Opening serial port...");
                serialPort.Open();
                Console.WriteLine("Serialport opened");

                var stream = serialPort.BaseStream;

                IDuplexPipe duplexPipe = StreamConnection.GetDuplex(stream);
                ICommunicator<string> comm = new Communicator(duplexPipe);

                // Initialize
                var initializeResult = await comm.InitializeAsync();
                Console.WriteLine($"Initialize: {initializeResult}");
                Thread.Sleep(1000);

                // Set command echo
                var echoResult = await comm.EnableCommandEcho(false);
                Console.WriteLine($"Echo disabled: {echoResult}");

                // PIN status
                var pinResult = await comm.GetPinStatus();
                Console.WriteLine(pinResult);

                // Signal quality
                var signalQualityResult = await comm.GetSignalQuality();
                Console.WriteLine(signalQualityResult);

                // Read Mode
                //await comm.ReadModeAsync();
                //Thread.Sleep(1000);

                // Set Mode
                //var setModeResult = await comm.SetModeAsync(Mode.Text);
                //Console.WriteLine($"Set Mode: {setModeResult}");
                //Thread.Sleep(1000);

                // Get battery status
                var batteryStatus = await comm.GetBatteryStatusAsync();
                Console.WriteLine(batteryStatus);

                // Send SMS
                //var smsStatus = await comm.SendSmsAsync(new PhoneNumber("NUMBER"), "Im sending you an SMS!");
                //Console.WriteLine($"Send SMS: {smsStatus}");
            }

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
