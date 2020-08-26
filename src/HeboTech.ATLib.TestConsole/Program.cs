using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Inputs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SerialPort serialPort = new SerialPort(args[0], 9600, Parity.None, 8, StopBits.One))
            {
                Console.WriteLine("Opening serial port...");
                serialPort.Open();
                Console.WriteLine("Serialport opened");

                ICommunicator comm = new SerialPortCommunicator(serialPort);

                AtChannel atChannel = new AtChannel(comm);

                AdafruitFona3G modem = new AdafruitFona3G(atChannel);
                modem.IncomingCall += Modem_IncomingCall;
                modem.MissedCall += Modem_MissedCall;

                modem.DisableEcho();

                var simStatus = modem.GetSimStatus();
                Console.WriteLine($"SIM Status: {simStatus}");

                var remainingCodeAttemps = modem.GetRemainingPinPukAttempts();
                Console.WriteLine($"Remaining attempts: {remainingCodeAttemps}");

                if (simStatus == SimStatus.SIM_PIN)
                {
                    var simPinStatus = modem.EnterSimPin(new PersonalIdentificationNumber(args[1]));
                    Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                    simStatus = modem.GetSimStatus();
                    Console.WriteLine($"SIM Status: {simStatus}");
                }

                var signalStrength = modem.GetSignalStrength();
                Console.WriteLine($"Signal Strength: {signalStrength}");

                var batteryStatus = modem.GetBatteryStatus();
                Console.WriteLine($"Battery Status: {batteryStatus}");

                var productInfo = modem.GetProductIdentificationInformation();
                Console.WriteLine($"Product Information:{Environment.NewLine}{productInfo}");

                Console.WriteLine("Done. Press 'a' to answer call, 'h' to hang up, 's' to send SMS and 'q' to exit...");
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
                        case ConsoleKey.S:
                            Console.WriteLine("Sending SMS...");
                            var smsReference = modem.SendSMS(new PhoneNumber(args[2]), "Hello ATLib!");
                            Console.WriteLine($"SMS Reference: {smsReference}");
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
