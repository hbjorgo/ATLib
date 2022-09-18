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
            // Because of multi targeting, print out current framework target for information
            var targetFrameworkAttribute = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .SingleOrDefault() as TargetFrameworkAttribute;
            Console.WriteLine($"Current target: {targetFrameworkAttribute.FrameworkName}");



            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string pin = args[0];



            /* ######## UNCOMMENT THIS SECTION TO USE SERIAL PORT ######## */
            using SerialPort serialPort = new("COM1", 9600, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.RequestToSend
            };
            serialPort.Open();
            Console.WriteLine("Serialport opened");
            Stream stream;
            stream = serialPort.BaseStream;


            /* ######## UNCOMMENT THIS SECTION TO USE NETWORK SOCKET ######## */
            //using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect("192.168.10.144", 7000);
            //Console.WriteLine("Network socket opened");
            //Stream stream;
            //stream = new NetworkStream(socket);


            // ### Choose what to run
            await FunctionalityTest.Run(stream, pin);
            //await StressTest.Run(stream, pin);
        }
    }
}
