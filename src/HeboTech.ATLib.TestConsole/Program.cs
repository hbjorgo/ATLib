using HeboTech.ATLib.Commands;
using HeboTech.MessageReader;
using System;
using System.IO;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial;
using HeboTech.ATLib.Pipelines;
using System.Net.Sockets;
using System.Net;

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

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        static void Main2(string[] args)
        {
            using (MemoryStream stream = new MemoryStream())
            using (GsmStream gsmStream = new GsmStream(stream, Encoding.ASCII))
            {
                Gsm g = new Gsm(gsmStream);
                if (!g.InitializeAsync().Result)
                    Console.WriteLine("Initialization failed");
                if (!g.SetModeAsync(Mode.Text).Result)
                    Console.WriteLine("Set mode failed");
                if (!g.SendSmsAsync(new PhoneNumber("12345678"), "Msg").Result)
                    Console.WriteLine("Sending SMS failed");

                Console.WriteLine(Encoding.Default.GetString(stream.ToArray()));
            }

            Console.ReadKey();
        }
    }
}
