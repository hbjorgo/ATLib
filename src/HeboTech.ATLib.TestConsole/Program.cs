using System;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string pin = args[0];
            string phoneNumber = args[1];


            Stream stream;


            /*
            // ### Uncomment this section to use serial port
            using SerialPort serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.RequestToSend
            };
            serialPort.Open();
            Console.WriteLine("Serialport opened");
            stream = serialPort.BaseStream;
            */

            // ### Uncomment this section to use network socket
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("192.168.10.144", 7000);
            Console.WriteLine("Network socket opened");
            stream = new NetworkStream(socket);


            // ### Choose what to run
            await FunctionalityTest.Run(stream, pin, phoneNumber);
            //await StressTest.Run(stream, pin, phoneNumber);
        }
    }
}
