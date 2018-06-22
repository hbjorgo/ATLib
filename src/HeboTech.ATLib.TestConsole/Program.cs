using System;
using System.IO;
using System.Text;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MemoryStream stream = new MemoryStream())
            using (GsmStream gsmStream = new GsmStream(stream, Encoding.ASCII))
            {
                Gsm g = new Gsm(gsmStream);
                if (!g.InitializeAsync().Result)
                    Console.WriteLine("Initialization failed");
                if (!g.SetModeAsync(Mode.Text).Result)
                    Console.WriteLine("Set mode failed");
                if (!g.SendSmsAsync("12345678", "Msg").Result)
                    Console.WriteLine("Sending SMS failed");

                Console.WriteLine(Encoding.Default.GetString(stream.ToArray()));
            }

            Console.ReadKey();
        }
    }
}
