using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set specifically to support more character sets than default
            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine($"Console input encoding: {Console.InputEncoding}.");
            Console.WriteLine($"Console output encoding: {Console.OutputEncoding}.");

            // Because of multi targeting, print out current framework target for information
            var targetFrameworkAttribute = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .SingleOrDefault() as TargetFrameworkAttribute;
            Console.WriteLine($"Current target: {targetFrameworkAttribute.FrameworkName}");



            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string address = args[0];
            int port = int.Parse(args[1]);
            string pin = args[2];



            /* ######## UNCOMMENT THIS SECTION TO USE SERIAL PORT ######## */
            //using (SerialPort serialPort = new("COM1", 9600, Parity.None, 8, StopBits.One)
            //    {
            //        Handshake = Handshake.RequestToSend
            //    })
            //{
            //    serialPort.Open();
            //    Console.WriteLine("Serialport opened");
            //    Stream stream;
            //    stream = serialPort.BaseStream;

            //    // ### Choose what to run
            //    await FunctionalityTest.RunAsync(stream, pin);
            //    //await StressTest.RunAsync(stream, pin);
            //}


            /* ######## UNCOMMENT THIS SECTION TO USE NETWORK SOCKET ######## */
            using (TcpClient tcpClient = new TcpClient(address, port))
            {
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    Console.WriteLine("Network socket opened");
                    // ### Choose what to run
                    await FunctionalityTest.RunAsync(stream, pin);
                    //await StressTest.RunAsync(stream, pin);
                }
            }
        }
    }
}
