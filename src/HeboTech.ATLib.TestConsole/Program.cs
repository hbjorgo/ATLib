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
            using (GsmStream gsmStream = new GsmStream(stream))
            {
                Gsm g = new Gsm(gsmStream);
                g.InitializeAsync(Gsm.Mode.Text).Wait();
                g.SendSmsAsync("12345678", "Msg").Wait();

                Console.WriteLine(Encoding.Default.GetString(stream.ToArray()));
            }

            Console.ReadKey();
        }
    }
}
