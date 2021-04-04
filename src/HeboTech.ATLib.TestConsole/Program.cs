using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Modems.D_LINK;
using HeboTech.ATLib.Parsers;
using System;
using System.IO.Ports;

namespace HeboTech.ATLib.TestConsole
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            using SerialPort serialPort = new SerialPort(args[0], 9600, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.RequestToSend
            };
            Console.WriteLine("Opening serial port...");
            serialPort.Open();
            Console.WriteLine("Serialport opened");

            using AtChannel atChannel = new AtChannel(serialPort.BaseStream, serialPort.BaseStream);
            using IModem modem = new DWM222(atChannel);

            modem.IncomingCall += Modem_IncomingCall;
            modem.MissedCall += Modem_MissedCall;
            modem.SmsReceived += Modem_SmsReceived;

            await modem.DisableEchoAsync();

            var simStatus = await modem.GetSimStatusAsync();
            Console.WriteLine($"SIM Status: {simStatus}");

            // SIMCOM SIM5320
            //var remainingCodeAttemps = await modem.GetRemainingPinPukAttempts();
            //Console.WriteLine($"Remaining attempts: {remainingCodeAttemps}");

            if (simStatus == SimStatus.SIM_PIN)
            {
                var simPinStatus = await modem.EnterSimPinAsync(new PersonalIdentificationNumber(args[1]));
                Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                simStatus = await modem.GetSimStatusAsync();
                Console.WriteLine($"SIM Status: {simStatus}");
            }

            var signalStrength = await modem.GetSignalStrengthAsync();
            Console.WriteLine($"Signal Strength: {signalStrength}");

            var batteryStatus = await modem.GetBatteryStatusAsync();
            Console.WriteLine($"Battery Status: {batteryStatus}");

            var productInfo = await modem.GetProductIdentificationInformationAsync();
            Console.WriteLine($"Product Information:{Environment.NewLine}{productInfo}");

            var setDateTimeResult = await modem.SetDateTimeAsync(DateTimeOffset.Now);
            Console.WriteLine($"Setting date and time: {setDateTimeResult}");

            var dateTime = await modem.GetDateTimeAsync();
            Console.WriteLine($"Date and time: {dateTime}");

            var smsTextFormatResult = await modem.SetSmsMessageFormatAsync(SmsTextFormat.Text);
            Console.WriteLine($"Setting SMS text format: {smsTextFormatResult}");

            var singleSms = await modem.ReadSmsAsync(2);
            Console.WriteLine($"Single SMS: {singleSms}");

            var smss = await modem.ListSmssAsync(SmsStatus.ALL);
            foreach (var sms in smss)
            {
                Console.WriteLine($"SMS: {sms}");
                var smsDeleteStatus = await modem.DeleteSmsAsync(sms.Index);
                Console.WriteLine($"Delete SMS #{sms.Index} - {smsDeleteStatus}");
            }

            Console.WriteLine("Done. Press 'a' to answer call, 'h' to hang up, 's' to send SMS and 'q' to exit...");
            ConsoleKey key;
            while ((key = Console.ReadKey().Key) != ConsoleKey.Q)
            {
                switch (key)
                {
                    case ConsoleKey.A:
                        var answerStatus = await modem.AnswerIncomingCallAsync();
                        Console.WriteLine($"Answer Status: {answerStatus}");
                        break;
                    case ConsoleKey.H:
                        var callDetails = await modem.HangupAsync();
                        Console.WriteLine($"Call Details: {callDetails}");
                        break;
                    case ConsoleKey.S:
                        Console.WriteLine("Sending SMS...");
                        var smsReference = await modem.SendSmsAsync(new PhoneNumber(args[2]), "Hello ATLib!");
                        Console.WriteLine($"SMS Reference: {smsReference}");
                        break;
                }
            }
        }

        private static void Modem_SmsReceived(object sender, Events.SmsReceivedEventArgs e)
        {
            Console.WriteLine($"SMS received. Index {e.Index} at storage location {e.Storage}");
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
