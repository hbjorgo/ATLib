using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Parsers;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            using (SerialPort serialPort = new SerialPort("COM7", 9600, Parity.None, 8, StopBits.One))
            {
                Console.WriteLine("Opening serial port...");
                serialPort.Open();
                Console.WriteLine("Serialport opened");

                ICommunicator comm = new SerialPortCommunicator(serialPort);

                AtChannel atChannel = new AtChannel(comm);

                AdafruitFona modem = new AdafruitFona(atChannel);
                modem.IncomingCall += Modem_IncomingCall;
                modem.MissedCall += Modem_MissedCall;

                modem.DisableEcho();

                var simStatus = modem.GetSimStatus();
                Console.WriteLine($"SIM Status: {simStatus}");

                if (simStatus == States.SimStatus.SIM_PIN)
                {
                    var simPinStatus = modem.EnterSimPin(new Pin(args[0]));
                    Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                    simStatus = modem.GetSimStatus();
                    Console.WriteLine($"SIM Status: {simStatus}");
                }

                var signalStrength = modem.GetSignalStrength();
                Console.WriteLine($"Signal Strength: {signalStrength}");

                var batteryStatus = modem.GetBatteryStatus();
                Console.WriteLine($"Battery Status: {batteryStatus}");

                var smsReference = modem.SendSMS(new PhoneNumber(args[1]), "Hello ATLib!");
                Console.WriteLine($"SMS Reference: {smsReference}");

                Console.WriteLine("Done. Press any key to exit...");
                ConsoleKey key;
                while ((key = Console.ReadKey().Key) != ConsoleKey.Q)
                {
                    switch (key)
                    {
                        case ConsoleKey.A:
                            var answerStatus = modem.AnswerIncomingCall();
                            Console.WriteLine($"Answer Status: {answerStatus}");
                            break;
                        case ConsoleKey.H:
                            var callDetails = modem.Hangup();
                            Console.WriteLine($"Call Details: {callDetails}");
                            break;
                    }
                }
                modem.Close();
            }
        }

        private static void Modem_MissedCall(object sender, Events.MissedCallEventArgs e)
        {
            Console.WriteLine($"Missed call at {e.Time} from {e.PhoneNumber}");
        }

        private static void Modem_IncomingCall(object sender, Events.IncomingCallEventArgs e)
        {
            Console.WriteLine("Incoming call...");
        }
    }
}
