using HeboTech.ATLib.Commands;
using HeboTech.MessageReader;
using System;
using System.IO;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static MemoryStream memStream = new MemoryStream();

        static async Task Main(string[] args)
        {
            SerialPort serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            //serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();

            PipeReader reader = PipeReader.Create(serialPort.BaseStream, new StreamPipeReaderOptions(bufferSize: 2));
            ICommunicator<string> comm = new Communicator<string>(serialPort.BaseStream, new Pipelines.MessageReader(reader));

            // Initialize
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine($"Initialize: {comm.Initialize()}");
                Thread.Sleep(1000);
            }

            // Read Mode
            comm.ReadMode();
            Thread.Sleep(1000);

            // Set Mode
            Console.WriteLine($"Set Mode: {comm.SetMode(Mode.Text)}");
            Thread.Sleep(1000);

            // Get battery status
            var batteryStatus = comm.GetBatteryStatus();

            // Send SMS
            var smsStatus = comm.SendSms(new PhoneNumber("41501790"), "I'm sending you an SMS!");
            Console.WriteLine($"Send SMS: {smsStatus}");

            serialPort.Close();

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            string available = serialPort.ReadExisting();
            //Console.WriteLine($"SP:{available}");
            long originalPosition = memStream.Position;
            memStream.Write(Encoding.UTF8.GetBytes(available));
            memStream.Position = originalPosition;
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
