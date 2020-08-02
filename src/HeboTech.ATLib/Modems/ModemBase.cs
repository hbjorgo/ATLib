using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.States;
using System;
using System.Net.Cache;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems
{
    public abstract class ModemBase
    {
        private readonly ICommunicator comm;

        public enum Error
        {
            NO_ERROR = 0,
            TIMEOUT
        }

        public enum CME_ERROR
        {
            NO_ERROR
        }

        public ModemBase(ICommunicator comm)
        {
            this.comm = comm;
        }

        public virtual PinStatus ReadPinStatus()
        {
            var err = Write("AT+CPIN?", "+CPIN:", out string response);
            switch (err)
            {
                case Error.NO_ERROR:
                    switch (GetCmeError(response))
                    {
                        case CME_ERROR.NO_ERROR:
                            response = AtTokenizer.TokenizeStart(response);
                            response = AtTokenizer.TokenizeNextString(response, out string token);
                            if (token == "READY")
                                return PinStatus.READY;
                            break;
                        default:
                            break;
                    }
                    break;
                case Error.TIMEOUT:
                    break;
                default:
                    break;
            }
        }

        private CME_ERROR GetCmeError(string response)
        {
            return CME_ERROR.NO_ERROR;
        }

        private Error Write(string command, string responsePrefix, out string response)
        {
            comm.Write(command).GetAwaiter().GetResult();
            response = null;
            return Error.NO_ERROR;
        }
    }
}
