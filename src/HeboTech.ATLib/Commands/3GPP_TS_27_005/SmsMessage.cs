using HeboTech.ATLib.States;
using System;

namespace HeboTech.ATLib.Commands._3GPP_TS_27_005
{
    public class SmsMessage
    {
        public SmsMessage(string message, Mode mode)
        {
            Message = message;
            Mode = mode;
            if (mode == Mode.PDU)
                throw new NotImplementedException();
        }

        public string Message { get; }
        public Mode Mode { get; }
    }
}
