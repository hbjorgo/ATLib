using HeboTech.ATLib.Commands._3GPP_TS_27_005;
using HeboTech.ATLib.Commands._3GPP_TS_27_007;
using HeboTech.ATLib.Commands.V25TER;
using HeboTech.ATLib.Communication;
using HeboTech.ATLib.States;
using Pipelines.Sockets.Unofficial;
using System;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Threading;
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

                var stream = serialPort.BaseStream;

                IDuplexPipe duplexPipe = StreamConnection.GetDuplex(stream);
                ICommunicator comm = new Communicator(duplexPipe);

                var responseFormat = ResponseFormat.Numeric;
                await comm.EnableCommandEchoAsync(false);
                await comm.SetResponseFormatAsync(responseFormat);

                var pinResult = await comm.GetPinStatusAsync(responseFormat, new CancellationTokenSource(60000).Token);
                if (!pinResult.HasValue)
                {
                    Console.WriteLine("PIN error");
                    Environment.Exit(0);
                }
                switch (pinResult.Value.Status)
                {
                    case PinStatus.READY:
                        break;
                    case PinStatus.SIM_PIN:
                        var enterPinResult = await comm.EnterPinAsync(responseFormat, new Pin("<PIN>"), new CancellationTokenSource(2000).Token);
                        if (!enterPinResult.HasValue)
                        {
                            Console.WriteLine("PIN Enter error");
                            Environment.Exit(0);
                        }
                        break;
                    case PinStatus.SIM_PUK:
                        break;
                    case PinStatus.PH_SIM_PIN:
                        break;
                    case PinStatus.PH_FSIM_PIN:
                        break;
                    case PinStatus.PH_FSIM_PUK:
                        break;
                    case PinStatus.SIM_PIN2:
                        break;
                    case PinStatus.SIM_PUK2:
                        break;
                    case PinStatus.PH_NET_PIN:
                        break;
                    case PinStatus.PH_NET_PUK:
                        break;
                    case PinStatus.PH_NETSUB_PIN:
                        break;
                    case PinStatus.PH_NETSUB_PUK:
                        break;
                    case PinStatus.PH_SP_PIN:
                        break;
                    case PinStatus.PH_SP_PUK:
                        break;
                    case PinStatus.PH_CORP_PIN:
                        break;
                    case PinStatus.PH_CORP_PUK:
                        break;
                }

                var smsResponse = await comm.SendSmsAsync(responseFormat, new PhoneNumber("<NUMBER>"), new SmsMessage("I'm sending you an SMS!", Mode.Text), new CancellationTokenSource(2000).Token);
                if (smsResponse.HasValue)
                    Console.WriteLine(smsResponse.Value);
            }

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
