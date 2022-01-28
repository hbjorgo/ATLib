using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Modems.D_LINK;
using HeboTech.ATLib.Parsers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    public static class StressTest
    {
        public static async Task Run(System.IO.Stream stream, string pin, string phoneNumber)
        {
            Console.WriteLine($"Test started ({DateTime.Now})");

            using AtChannel atChannel = AtChannel.Create(stream);
            using IModem modem = new DWM222(atChannel);
            atChannel.Open();

            modem.IncomingCall += Modem_IncomingCall;
            modem.MissedCall += Modem_MissedCall;
            modem.SmsReceived += Modem_SmsReceived;

            await modem.DisableEchoAsync();

            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine($"######## TEST No. {i} ########");

                var simStatus = await modem.GetSimStatusAsync();
                Console.WriteLine($"SIM Status: {simStatus}");

                if (simStatus == SimStatus.SIM_PIN)
                {
                    var simPinStatus = await modem.EnterSimPinAsync(new PersonalIdentificationNumber(pin));
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

                var singleSms = await modem.ReadSmsAsync(5);
                Console.WriteLine($"Single SMS: {singleSms}");

                var smss = await modem.ListSmssAsync(SmsStatus.ALL);
                foreach (var sms in smss)
                {
                    Console.WriteLine($"SMS: {sms}");
                }

                Thread.Sleep(500);
            }
            Console.WriteLine($"Test complete ({DateTime.Now})");
            Console.ReadKey();
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
