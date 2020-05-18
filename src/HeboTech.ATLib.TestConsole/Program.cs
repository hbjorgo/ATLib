using HeboTech.ATLib.Commands;
using HeboTech.ATLib.Communication;
using HeboTech.ATLib.States;
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

                ResponseFormat responseFormat = ResponseFormat.Numeric;

                // Initialize
                var initializeResult = await comm.InitializeAsync(responseFormat);
                if (initializeResult.HasValue)
                    Console.WriteLine($"Initialize: {initializeResult.Value}");
                Thread.Sleep(1000);

                // Set command echo
                var echoResult = await comm.EnableCommandEchoAsync(responseFormat, false);
                if (echoResult.HasValue)
                    Console.WriteLine($"Echo disabled: {echoResult.Value}");

                // PIN status
                var pinResult = await comm.GetPinStatusAsync(responseFormat);
                if (pinResult.HasValue)
                    Console.WriteLine(pinResult.Value);

                // Signal quality
                var signalQualityResult = await comm.GetSignalQualityAsync(responseFormat);
                if (signalQualityResult.HasValue)
                    Console.WriteLine(signalQualityResult.Value);

                // Get battery status
                var batteryStatus = await comm.GetBatteryStatusAsync(responseFormat);
                if (batteryStatus.HasValue)
                    Console.WriteLine(batteryStatus.Value);

                // Read Mode
                //await comm.ReadModeAsync();
                //Thread.Sleep(1000);

                // Set Mode
                //var setModeResult = await comm.SetModeAsync(Mode.Text);
                //Console.WriteLine($"Set Mode: {setModeResult}");
                //Thread.Sleep(1000);

                // Send SMS
                //var smsStatus = await comm.SendSmsAsync(new PhoneNumber("NUMBER"), "Im sending you an SMS!");
                //Console.WriteLine($"Send SMS: {smsStatus}");
            }

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
