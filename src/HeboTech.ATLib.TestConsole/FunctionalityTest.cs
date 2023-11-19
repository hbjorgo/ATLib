using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Events;
using HeboTech.ATLib.Modems.Cinterion;
using HeboTech.ATLib.Modems.Generic;
using HeboTech.ATLib.Modems.SIMCOM;
using HeboTech.ATLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    public static class FunctionalityTest
    {
        public static async Task RunAsync(System.IO.Stream stream, string pin)
        {
            using AtChannel atChannel = AtChannel.Create(stream);
            //atChannel.EnableDebug((string line) => Console.WriteLine(line));
            using IMC55i modem = new MC55i(atChannel);
            modem.Open();

            modem.IncomingCall += Modem_IncomingCall;
            modem.MissedCall += Modem_MissedCall;
            modem.CallStarted += Modem_CallStarted;
            modem.CallEnded += Modem_CallEnded;
            modem.SmsReceived += Modem_SmsReceived;
            modem.UssdResponseReceived += Modem_UssdResponseReceived;
            modem.ErrorReceived += Modem_ErrorReceived;
            modem.GenericEvent += Modem_GenericEvent;

            // Configure modem with required settings before PIN
            var requiredSettingsBeforePin = await modem.SetRequiredSettingsBeforePinAsync();
            Console.WriteLine($"Successfully set required settings before PIN: {requiredSettingsBeforePin}");

            //{
            //    if (modem is IMC55i mc55i)
            //    {
            //        var indicateSimDataReady = await mc55i.IndicateSimDataReady(true);
            //        Console.WriteLine($"MC55i Indicate SIM data ready: {indicateSimDataReady}");
            //    }
            //}

            var simStatus = await modem.GetSimStatusAsync();
            Console.WriteLine($"SIM Status: {simStatus}");

            {
                if (modem is ISIM5320 sim5320)
                    await sim5320.ReInitializeSimAsync();
            }

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
                    await Task.Delay(TimeSpan.FromSeconds(1));
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
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            // Configure modem with required settings after PIN
            var requiredSettingsAfterPin = await modem.SetRequiredSettingsAfterPinAsync();
            Console.WriteLine($"Successfully set required settings after PIN: {requiredSettingsAfterPin}");

            var signalStrength = await modem.GetSignalStrengthAsync();
            Console.WriteLine($"Signal Strength: {signalStrength}");

            var batteryStatus = await modem.GetBatteryStatusAsync();
            Console.WriteLine($"Battery Status: {batteryStatus}");

            {
                if (modem is IMC55i mc55i)
                {
                    var mc55iBatteryStatus = await mc55i.MC55i_GetBatteryStatusAsync();
                    Console.WriteLine($"MC55i Battery Status: {mc55iBatteryStatus}");
                }
            }

            var productInfo = await modem.GetProductIdentificationInformationAsync();
            Console.WriteLine($"Product Information:{Environment.NewLine}{productInfo}");

            var setDateTimeResult = await modem.SetDateTimeAsync(DateTimeOffset.Now);
            Console.WriteLine($"Setting date and time: {setDateTimeResult}");

            var dateTime = await modem.GetDateTimeAsync();
            Console.WriteLine($"Date and time: {dateTime}");

            var newSmsIndicationResult = await modem.SetNewSmsIndication(2, 1, 0, 0, 1);
            Console.WriteLine($"Setting new SMS indication: {newSmsIndicationResult}");

            var supportedStorages = await modem.GetSupportedPreferredMessageStoragesAsync();
            Console.WriteLine($"Supported storages:{Environment.NewLine}{supportedStorages}");
            var currentStorages = await modem.GetPreferredMessageStoragesAsync();
            Console.WriteLine($"Current storages:{Environment.NewLine}{currentStorages}");
            var setPreferredStorages = await modem.SetPreferredMessageStorageAsync(MessageStorage.SM, MessageStorage.SM, MessageStorage.SM);
            Console.WriteLine($"Storages set:{Environment.NewLine}{setPreferredStorages}");

            Console.WriteLine("Done. Press 'a' to answer call, 'd' to dial, 'h' to hang up, 's' to send SMS, 'r' to read an SMS, 'l' to list all SMSs, 'u' to send USSD code, 'x' to send raw command, 'z' to send raw command with response, '+' to enable debug, '-' to disable debug and 'q' to exit...");
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
                            var rawStatus = await modem.RawCommand(rawCommand);
                            Console.WriteLine($"Raw command status: {rawStatus}");
                        }
                        break;
                    case ConsoleKey.Z:
                        {
                            Console.WriteLine("Enter command:");
                            string rawCommand = Console.ReadLine();
                            Console.WriteLine("Enter response:");
                            string rawResponse = Console.ReadLine();
                            var rawStatus = await modem.RawCommandWithResponse(rawCommand.ToUpperInvariant(), rawResponse.ToUpperInvariant());
                            if (rawStatus.Success)
                                Console.WriteLine($"Raw command status: {string.Join(',', rawStatus.Result)}");
                            else
                                Console.WriteLine($"Raw command status: {rawStatus}");
                        }
                        break;
                    case ConsoleKey.D:
                        {
                            Console.WriteLine("Please enter phone number:");
                            string phoneNumberString = Console.ReadLine();
                            PhoneNumber phoneNumber = new(phoneNumberString);

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
                            string phoneNumberString = Console.ReadLine();
                            PhoneNumber phoneNumber = new(phoneNumberString);

                            Console.WriteLine("Please enter SMS message:");
                            string smsMessage = Console.ReadLine();

                            Console.WriteLine("Sending SMS...");
                            IEnumerable<ModemResponse<SmsReference>> smsReferences = await modem.SendSmsAsync(phoneNumber, smsMessage, CharacterSet.UCS2);
                            foreach (var smsReference in smsReferences)
                                Console.WriteLine($"SMS Reference: {smsReference}");
                            break;
                        }
                    case ConsoleKey.R:
                        Console.WriteLine("Enter SMS index:");
                        if (int.TryParse(Console.ReadLine(), out int smsIndex))
                        {
                            var sms = await modem.ReadSmsAsync(smsIndex);
                            Console.WriteLine(sms);
                        }
                        else
                            Console.WriteLine("Invalid SMS index");
                        break;
                    case ConsoleKey.U:
                        Console.WriteLine("Enter USSD Code:");
                        var ussd = Console.ReadLine();
                        var ussdResult = await modem.SendUssdAsync(ussd);
                        Console.WriteLine($"USSD Status: {ussdResult}");
                        break;
                    case ConsoleKey.L:
                        Console.WriteLine("List all SMSs:");
                        var smss = await modem.ListSmssAsync(SmsStatus.ALL);
                        if (smss.Success && smss.Result.Any())
                        {
                            foreach (var sms in smss.Result)
                                Console.WriteLine($"------------------------------------------------{Environment.NewLine}{sms}");
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
