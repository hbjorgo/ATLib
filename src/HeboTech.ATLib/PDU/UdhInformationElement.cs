using System;
using System.Collections.Generic;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    internal class UdhInformationElement
    {
    }

    internal class ConcatenatedShortMessages : UdhInformationElement
    {
        private readonly string message;
        private readonly byte messageReference;

        public ConcatenatedShortMessages(string message, byte messageReference)
        {
            this.message = message;
            this.messageReference = messageReference;
        }
    }
}
