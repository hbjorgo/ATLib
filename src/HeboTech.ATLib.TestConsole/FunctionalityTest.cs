using HeboTech.ATLib.Events;
using HeboTech.ATLib.Messaging;
using HeboTech.ATLib.Modems.D_LINK;
using HeboTech.ATLib.Numbering;
using HeboTech.ATLib.Parsing;
using HeboTech.ATLib.Storage;
using HeboTech.ATLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeboTech.ATLib.Modems.Cinterion;

namespace HeboTech.ATLib.TestConsole
{
    public static class FunctionalityTest
    {
        private static readonly string debugPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "atlog",
            "log.txt"
            );

        private static void Log(string message)
        {
            string formattedLine = $"({DateTime.Now}) {message}";
            try
            {
                System.IO.File.AppendAllLines(debugPath, [formattedLine]);
            }
            catch (Exception e)
            {
                //Console.WriteLine($"Error while logging: {e}");
            }
        }

        public static async Task RunAsync(System.IO.Stream stream, string pin)
        {
            using AtChannel atChannel = AtChannel.Create(stream);
            atChannel.EnableDebug(Log);
            //using IMC55i modem = new MC55i(atChannel);
            using IDWM222 modem = new DWM222(atChannel);
            atChannel.Open();
            await atChannel.ClearAsync();

            modem.ErrorReceived += Modem_ErrorReceived;

            modem.GenericEvent += Modem_GenericEvent;

            modem.IncomingCall += Modem_IncomingCall;
            modem.MissedCall += Modem_MissedCall;
            modem.CallStarted += Modem_CallStarted;
            modem.CallEnded += Modem_CallEnded;

            modem.SmsReceived += Modem_SmsReceived;
            modem.SmsStorageReferenceReceived += Modem_SmsStorageReferenceReceived;

            modem.SmsStatusReportReceived += Modem_SmsStatusReportReceived;
            modem.SmsStatusReportStorageReferenceReceived += Modem_SmsStatusReportStorageReferenceReceived;

            modem.UssdResponseReceived += Modem_UssdResponseReceived;

            // Configure modem with required settings before PIN
            var requiredSettingsBeforePin = await modem.SetRequiredSettingsBeforePinAsync();
            Console.WriteLine($"Successfully set required settings before PIN: {requiredSettingsBeforePin}");

            var simStatus = await modem.GetSimStatusAsync();
            Console.WriteLine($"SIM Status: {simStatus}");

            simStatus = await modem.GetSimStatusAsync();
            Console.WriteLine($"SIM Status: {simStatus}");

            if (simStatus.Success && simStatus.Result == SimStatus.SIM_READY)
            {
            }
            else if (simStatus.Success && simStatus.Result == SimStatus.SIM_PIN)
            {
                var simPinStatus = await modem.EnterSimPinAsync(new PersonalIdentificationNumber(pin));
                Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                for (int i = 0; i < 10; i++)
                {
                    simStatus = await modem.GetSimStatusAsync();
                    Console.WriteLine($"SIM Status: {simStatus}");
                    if (simStatus.Success && simStatus.Result == SimStatus.SIM_READY)
                        break;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
            else
            {
                Console.Write(simStatus);
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                var imsi = await modem.GetImsiAsync();
                Console.WriteLine($"IMSI: {imsi}");
                if (imsi.Success)
                    break;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            // Configure modem with required settings after PIN
            var requiredSettingsAfterPin = await modem.SetRequiredSettingsAfterPinAsync();
            Console.WriteLine($"Successfully set required settings after PIN: {requiredSettingsAfterPin}");

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

            var newSmsIndicationResult = await modem.SetNewSmsIndicationAsync(2, 1, 0, 2, 0); // 2, 1, 0, 2, 0 (CSMS=0)
            Console.WriteLine($"Setting new SMS indication: {newSmsIndicationResult}");

            var supportedStorages = await modem.GetSupportedPreferredMessageStoragesAsync();
            Console.WriteLine($"Supported storages:{Environment.NewLine}{supportedStorages}");
            var currentStorages = await modem.GetPreferredMessageStoragesAsync();
            Console.WriteLine($"Current storages:{Environment.NewLine}{currentStorages}");
            var setPreferredStorages = await modem.SetPreferredMessageStorageAsync(MessageStorage.MT, MessageStorage.MT, MessageStorage.MT);
            Console.WriteLine($"Storages set:{Environment.NewLine}{setPreferredStorages}");

            Log("Initialization done");
            Console.WriteLine("Done. Press 'a' to answer call, 'd' to dial, 'h' to hang up, 's' to send SMS, 'r' to read an SMS, 'l' to list all SMSs, 'p' to delete an SMS, 'u' to send USSD code, 'x' to send raw command, 'z' to send raw command with response, '+' to enable debug, '-' to disable debug and 'q' to exit...");
            ConsoleKey key;
            while ((key = Console.ReadKey().Key) != ConsoleKey.Q)
            {
                Console.WriteLine();
                switch (key)
                {
                    case ConsoleKey.A:
                        var answerStatus = await modem.AnswerIncomingCallAsync();
                        Console.WriteLine($"Answer Status: {answerStatus}");
                        break;
                    case ConsoleKey.X:
                        {
                            string rawCommand = Console.ReadLine();
                            var rawStatus = await modem.RawCommandAsync(rawCommand);
                            Console.WriteLine($"Raw command status: {rawStatus}");
                        }
                        break;
                    case ConsoleKey.Z:
                        {
                            Console.WriteLine("Enter command:");
                            string rawCommand = Console.ReadLine();
                            Console.WriteLine("Enter response:");
                            string rawResponse = Console.ReadLine();
                            var rawStatus = await modem.RawCommandWithResponseAsync(rawCommand.ToUpperInvariant(), rawResponse.ToUpperInvariant());
                            if (rawStatus.Success)
                                Console.WriteLine($"Raw command status: {string.Join(',', rawStatus.Result)}");
                            else
                                Console.WriteLine($"Raw command status: {rawStatus}");
                        }
                        break;
                    case ConsoleKey.D:
                        {
                            Console.WriteLine("Please enter phone number:");
                            string number = Console.ReadLine();
                            PhoneNumber phoneNumber = PhoneNumberFactory.Create(number);

                            var dialStatus = await modem.DialAsync(phoneNumber);
                            Console.WriteLine($"Dial Status: {dialStatus}");
                        }
                        break;
                    case ConsoleKey.H:
                        var hangupStatus = await modem.HangupAsync();
                        Console.WriteLine($"Hangup Status: {hangupStatus}");
                        break;
                    case ConsoleKey.S:
                        {
                            Console.WriteLine("Please enter phone number:");
                            string number = Console.ReadLine();
                            PhoneNumber phoneNumber = PhoneNumberFactory.Create(number);

                            Console.WriteLine("Please enter SMS message:");
                            string smsMessage = Console.ReadLine();

                            Console.WriteLine("Sending SMS...");
                            IEnumerable<ModemResponse<SmsReference>> smsReferences = await modem.SendSmsAsync(new SmsSubmitRequest(phoneNumber, smsMessage) { EnableStatusReportRequest = true, ValidityPeriod = ValidityPeriod.Relative(RelativeValidityPeriods.Minutes_5) });
                            foreach (var smsReference in smsReferences)
                                Console.WriteLine($"SMS Reference: {smsReference}");
                            break;
                        }
                    case ConsoleKey.R:
                        {
                            Console.WriteLine("Enter SMS index:");
                            if (int.TryParse(Console.ReadLine(), out int smsIndex))
                            {
                                var sms = await modem.ReadSmsAsync(smsIndex);
                                if (sms.Success)
                                {
                                    Console.WriteLine(sms.Result);
                                }
                            }
                            else
                                Console.WriteLine("Invalid SMS index");
                            break;
                        }
                    case ConsoleKey.P:
                        {
                            Console.WriteLine("Enter SMS index:");
                            if (int.TryParse(Console.ReadLine(), out int smsIndex))
                            {
                                var deleteResponse = await modem.DeleteSmsAsync(smsIndex);
                                Console.WriteLine(deleteResponse);
                            }
                            else
                                Console.WriteLine("Invalid SMS index");
                            break;
                        }
                    case ConsoleKey.U:
                        Console.WriteLine("Enter USSD Code:");
                        var ussd = Console.ReadLine();
                        var ussdResult = await modem.SendUssdAsync(ussd);
                        Console.WriteLine($"USSD Status: {ussdResult}");
                        break;
                    case ConsoleKey.L:
                        Console.WriteLine("List all SMSs:");
                        var smss = await modem.ListSmssAsync(SmsStatus.ALL);
                        Console.WriteLine($"{smss.Result.Count} SMSs:");
                        if (smss.Success && smss.Result.Any())
                        {
                            foreach (var sms in smss.Result)
                            {
                                Console.WriteLine($"------------------------------------------------");
                                Console.WriteLine($"Index: {sms.Index}, {sms.Sms}");
                            }
                            Console.WriteLine($"------------------------------------------------");
                        }

                        break;
                    case ConsoleKey.OemPlus:
                        atChannel.EnableDebug((string line) => Console.WriteLine(line));
                        Console.WriteLine("Debug enabled");
                        break;
                    case ConsoleKey.OemMinus:
                        atChannel.DisableDebug();
                        Console.WriteLine("Debug disabled");
                        break;
                }
            }
        }

        private static void Modem_BroadcastMessageStorageReferenceReceived(object sender, BroadcastMessageStorageReferenceReceivedEventArgs e)
        {
            Console.WriteLine($"Broadcast Message. Index {e.Index} at storage location {e.Storage}");
        }

        private static void Modem_SmsStorageReferenceReceived(object sender, SmsStorageReferenceReceivedEventArgs e)
        {
            Console.WriteLine($"SMS Deliver. Index {e.Index} at storage location {e.Storage}");
        }

        private static void Modem_SmsStatusReportStorageReferenceReceived(object sender, SmsStatusReportStorageReferenceEventArgs e)
        {
            Console.WriteLine($"SMS Status Report. Index {e.Index} at storage location {e.Storage}");
        }

        private static void Modem_BroadcastMessageReceived(object sender, BroadcastMessageReceivedEventArgs e)
        {
            Console.WriteLine($"Broadcast Message: {e.BroadcastMessage}");
        }

        private static void Modem_SmsReceived(object sender, SmsReceivedEventArgs e)
        {
            Console.WriteLine($"SMS Deliver: {e.SmsDeliver}");
        }

        private static void Modem_SmsStatusReportReceived(object sender, SmsStatusReportEventArgs e)
        {
            Console.WriteLine($"SMS Status Report: {e.SmsStatusReport}");
        }

        private static void Modem_GenericEvent(object sender, GenericEventArgs e)
        {
            Console.WriteLine($"Generic event: {e.Message}");
        }

        private static void Modem_ErrorReceived(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"ERROR EVENT: {e.Error}");
        }

        private static void Modem_UssdResponseReceived(object sender, UssdResponseEventArgs e)
        {
            if (e != null)
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
