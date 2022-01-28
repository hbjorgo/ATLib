using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Modems.D_LINK;
using HeboTech.ATLib.Parsers;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    public static class FunctionalityTest
    {
        public static async Task Run(string port, string pin, string phoneNumber)
        {
            using SerialPort serialPort = new(port, 9600, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.RequestToSend
            };
            Console.WriteLine("Opening serial port...");
            serialPort.Open();
            Console.WriteLine("Serialport opened");

            PhoneNumber recipient = new(phoneNumber);

            using AtChannel atChannel = AtChannel.Create(serialPort.BaseStream);
            using IModem modem = new DWM222(atChannel);
            atChannel.Open();

            modem.IncomingCall += Modem_IncomingCall;
            modem.MissedCall += Modem_MissedCall;
            modem.CallStarted += Modem_CallStarted;
            modem.CallEnded += Modem_CallEnded;
            modem.SmsReceived += Modem_SmsReceived;
            modem.UssdResponseReceived += Modem_UssdResponseReceived;
            modem.ErrorReceived += Modem_ErrorReceived;

            await modem.DisableEchoAsync();

            var simStatus = await modem.GetSimStatusAsync();
            Console.WriteLine($"SIM Status: {simStatus}");

            // SIMCOM SIM5320
            //var remainingCodeAttemps = await modem.GetRemainingPinPukAttempts();
            //Console.WriteLine($"Remaining attempts: {remainingCodeAttemps}");

            if (simStatus == SimStatus.SIM_PIN)
            {
                var simPinStatus = await modem.EnterSimPinAsync(new PersonalIdentificationNumber(pin));
                Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                simStatus = await modem.GetSimStatusAsync();
                Console.WriteLine($"SIM Status: {simStatus}");
            }

            var imsi = await modem.GetImsiAsync();
            Console.WriteLine($"IMSI: {imsi}");

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

            var newSmsIndicationResult = await modem.SetNewSmsIndication(2, 1, 0, 0, 0);
            Console.WriteLine($"Setting new SMS indication: {newSmsIndicationResult}");

            var singleSms = await modem.ReadSmsAsync(2);
            Console.WriteLine($"Single SMS: {singleSms}");

            var smss = await modem.ListSmssAsync(SmsStatus.ALL);
            foreach (var sms in smss)
            {
                Console.WriteLine($"SMS: {sms}");
                var smsDeleteStatus = await modem.DeleteSmsAsync(sms.Index);
                Console.WriteLine($"Delete SMS #{sms.Index} - {smsDeleteStatus}");
            }

            Console.WriteLine("Done. Press 'a' to answer call, 'd' to dial, 'h' to hang up, 's' to send SMS, 'u' to send USSD code and 'q' to exit...");
            ConsoleKey key;
            while ((key = Console.ReadKey().Key) != ConsoleKey.Q)
            {
                switch (key)
                {
                    case ConsoleKey.A:
                        var answerStatus = await modem.AnswerIncomingCallAsync();
                        Console.WriteLine($"Answer Status: {answerStatus}");
                        break;
                    case ConsoleKey.D:
                        var dialStatus = await modem.DialAsync(recipient);
                        Console.WriteLine($"Dial Status: {dialStatus}");
                        break;
                    case ConsoleKey.H:
                        var hangupStatus = await modem.HangupAsync();
                        Console.WriteLine($"Hangup Status: {hangupStatus}");
                        break;
                    case ConsoleKey.S:
                        Console.WriteLine("Sending SMS...");
                        var smsReference = await modem.SendSmsAsync(recipient, "Hello ATLib!");
                        Console.WriteLine($"SMS Reference: {smsReference}");
                        break;
                    case ConsoleKey.U:
                        Console.WriteLine("Enter USSD Code:");
                        var ussd = Console.ReadLine();
                        var ussdResult = await modem.SendUssdAsync(ussd);
                        Console.WriteLine($"USSD Status: {ussdResult}");
                        break;
                }
            }
        }

        private static void Modem_ErrorReceived(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"ERROR: {e.Error}");
        }

        private static void Modem_UssdResponseReceived(object sender, UssdResponseEventArgs e)
        {
            Console.WriteLine($"USSD Response: {e.Status} - {e.Response} - ({e.CodingScheme})");
        }

        private static void Modem_CallEnded(object sender, CallEndedEventArgs e)
        {
            Console.WriteLine($"Call ended. Duration: {e.Duration}");
        }

        private static void Modem_CallStarted(object sender, CallStartedEventArgs e)
        {
            Console.WriteLine("Call started");
        }

        private static void Modem_SmsReceived(object sender, SmsReceivedEventArgs e)
        {
            Console.WriteLine($"SMS received. Index {e.Index} at storage location {e.Storage}");
        }

        private static void Modem_MissedCall(object sender, MissedCallEventArgs e)
        {
            Console.WriteLine($"Missed call at {e.Time} from {e.PhoneNumber}");
        }

        private static void Modem_IncomingCall(object sender, IncomingCallEventArgs e)
        {
            Console.WriteLine("Incoming call...");
        }
    }
}
