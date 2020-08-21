using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.States;
using System;
using System.Diagnostics;
using System.Linq;

namespace HeboTech.ATLib.Modems
{
    public abstract class ModemBase
    {
        private readonly AtChannel atChannel;

        public ModemBase(ICommunicator communicator)
        {
            atChannel = new AtChannel(communicator);
        }

        public virtual SimStatus GetSimStatus()
        {
            var error = atChannel.SendSingleLineCommand("AT+CPIN?", "+CPIN:", out AtResponse response);

            if (error != AtChannel.AtError.NO_ERROR)
            {
                Console.WriteLine("Error :(");
                return SimStatus.SIM_NOT_READY;
            }

            switch (AtChannel.GetCmeError(response))
            {
                case AtChannel.AtCmeError.CME_SUCCESS:
                    break;
                case AtChannel.AtCmeError.CME_SIM_NOT_INSERTED:
                    return SimStatus.SIM_ABSENT;
                default:
                    return SimStatus.SIM_NOT_READY;
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.First();
            if (!AtTokenizer.TokenizeStart(cpinLine, out cpinLine))
            {
                return SimStatus.SIM_NOT_READY;
            }

            if (AtTokenizer.TokenizeNextString(cpinLine, out string cpinResult) == null)
            {
                return SimStatus.SIM_NOT_READY;
            }

            switch (cpinResult)
            {
                case "SIM PIN":
                    return SimStatus.SIM_PIN;
                case "SIM PUK":
                    return SimStatus.SIM_PUK;
                case "PH-NET PIN":
                    return SimStatus.SIM_NETWORK_PERSONALIZATION;
                case "READY":
                    return SimStatus.SIM_READY;
                default:
                    // Treat unsupported lock types as "sim absent"
                    return SimStatus.SIM_ABSENT;
            }
        }

        public virtual void GetSignalStrength()
        {
            var error = atChannel.SendSingleLineCommand("AT+CSQ", "+CSQ:", out AtResponse response);

            if (Debugger.IsAttached)
                Debugger.Break();
        }

        public virtual void GetBatteryStatus()
        {
            var error = atChannel.SendSingleLineCommand("AT+CBC", "+CBC:", out AtResponse response);

            if (Debugger.IsAttached)
                Debugger.Break();
        }

        public void Close()
        {
            atChannel.OnReadClosed();
        }
    }
}
