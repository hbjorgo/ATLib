using HeboTech.ATLib.Messaging;
using HeboTech.ATLib.Misc;
using HeboTech.ATLib.Modems.Adafruit;
using HeboTech.ATLib.Numbering;
using HeboTech.ATLib.Parsing;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace HeboTech.ATLib.TestConsole
{
    public class GetStartedExample
    {
        public static async Task RunAsync(string portName, int baudRate, string pin, string recepientPhoneNumber)
        {
            // Set up serial port
            using SerialPort serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.RequestToSend
            };
            serialPort.Open();

            // Create AT channel
            using AtChannel atChannel = AtChannel.Create(serialPort.BaseStream);

            // Create the modem
            using IModem modem = new Fona3G(atChannel);

            // Open AT channel
            atChannel.Open();

            // Configure modem with required settings before PIN
            var requiredSettingsBeforePin = await modem.SetRequiredSettingsBeforePinAsync();

            // Get SIM status
            var simStatus = await modem.GetSimStatusAsync();
            Console.WriteLine($"SIM Status: {simStatus}");

            // Check if SIM needs PIN
            if (simStatus.Result == SimStatus.SIM_PIN)
            {
                var simPinStatus = await modem.EnterSimPinAsync(new PersonalIdentificationNumber(pin));
                Console.WriteLine($"SIM PIN Status: {simPinStatus}");
            }

            // Configure modem with required settings after PIN
            var requiredSettingsAfterPin = await modem.SetRequiredSettingsAfterPinAsync();

            // Send SMS to the specified number
            PhoneNumber phoneNumber = PhoneNumberFactory.CreateCommonIsdn(recepientPhoneNumber);
            string message = "Hello ATLib!";
            var smsReferences = await modem.SendSmsAsync(new SmsSubmitRequest(phoneNumber, message));
            Console.WriteLine($"SMS Reference: {smsReferences.First()}");
        }
    }
}
