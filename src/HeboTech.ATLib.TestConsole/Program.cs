using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string port = args[0];
            string pin = args[1];
            string phoneNumber = args[2];

            await FunctionalityTest.Run(port, pin, phoneNumber);
            //await StressTest.Run(port, pin, phoneNumber);
        }
    }
}
