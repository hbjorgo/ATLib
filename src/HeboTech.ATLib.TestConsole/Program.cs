using System;
using System.CommandLine;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region generalOptions
                Option<string> pinOption = new("--pin")
                {
                    Description = "SIM PIN"
                };
            #endregion

            #region serialCommand
                Option<string> serialPortNameOption = new("--port")
                {
                    Description = "Serial port.",
                };
                Option<int> serialBaudRateOption = new("--baudrate")
                {
                    Description = "Serial baud rate. Default: 9600.",
                    DefaultValueFactory = parseResult => 9600
                };
                Option<Parity> serialParityOption = new("--parity")
                {
                    Description = "Serial parity. Default: none.",
                    DefaultValueFactory = parseResult => Parity.None
                };
                Option<int> serialDataBitsOption = new("--databits")
                {
                    Description = "Serial data bits. Default: 8",
                    DefaultValueFactory = parseResult => 8
                };
                Option<StopBits> serialStopBitsOption = new("--stopbits")
                {
                    Description = "Serial stop bits. Default: 1.",
                    DefaultValueFactory = parseResult => StopBits.One
                };
                
                Command serialCommand = new("--serial", "Serial connection")
                {
                    serialBaudRateOption,
                    serialPortNameOption,
                    serialParityOption,
                    serialDataBitsOption,
                    serialStopBitsOption,
                    pinOption,
                };
                serialCommand.SetAction(async result =>
                {
                    string serialPortName = result.GetRequiredValue(serialPortNameOption);
                    int serialBaudRate = result.GetRequiredValue(serialBaudRateOption);
                    string pin = result.GetRequiredValue(pinOption);
                    Parity parity = result.GetValue(serialParityOption);
                    int dataBits = result.GetValue(serialDataBitsOption);
                    StopBits stopBits = result.GetValue(serialStopBitsOption);
                
                    using SerialPort serialPort = new(serialPortName, serialBaudRate, parity, dataBits, stopBits);
                    serialPort.Handshake = Handshake.RequestToSend;
                    serialPort.Open();
                    Console.WriteLine("Serial port opened");
                    var stream = serialPort.BaseStream;
                
                    await FunctionalityTest.RunAsync(stream, pin);
                });
            #endregion
            
            #region streamCommand
            Option<string> streamAddressOption = new("--address")
            {
                Description = "Stream address.",
            };
            Option<int> streamPortOption = new("--port")
            {
                Description = "Stream port.",
            };

            Command streamCommand = new("--stream", "Stream connection")
            {
                streamAddressOption,
                streamPortOption,
                pinOption,
            };
            streamCommand.SetAction(async result =>
            {
                string address = result.GetRequiredValue(streamAddressOption);
                int port = result.GetRequiredValue(streamPortOption);
                string pin = result.GetRequiredValue(pinOption);

                using TcpClient tcpClient = new TcpClient(address, port);
                using NetworkStream stream = tcpClient.GetStream();
                Console.WriteLine("Network socket opened");
                
                await FunctionalityTest.RunAsync(stream, pin);
            });
            #endregion
            
            #region rootCommand
                RootCommand rootCommand = new("TestConsole");
                rootCommand.Subcommands.Add(streamCommand);
                rootCommand.Subcommands.Add(serialCommand);
                await rootCommand.Parse(args).InvokeAsync();
            #endregion

            Console.WriteLine("Goodbye!");
        }
    }
}
