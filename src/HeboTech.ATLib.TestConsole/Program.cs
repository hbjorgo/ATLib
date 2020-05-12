using HeboTech.ATLib.Pipelines;
using System;
using System.Buffers;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < 100; i++)
            {
                stream.Write(Encoding.UTF8.GetBytes($"{i}_OK\r"));
                if (i % 3 == 0)
                    stream.Write(Encoding.UTF8.GetBytes("###"));
            }
            stream.Position = 0;

            PipeReader reader = PipeReader.Create(stream, new StreamPipeReaderOptions(bufferSize: 32));

            ATMessageParser aTMessageReader = new ATMessageParser(reader);
            ATResult result;
            do
            {
                try
                {
                    result = await aTMessageReader.ReadSingleMessageAsync();
                    Console.WriteLine(result);
                }
                catch (InvalidDataException ide)
                {
                    Console.WriteLine(ide.Message);
                    break;
                }
            } while (result != null);

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
