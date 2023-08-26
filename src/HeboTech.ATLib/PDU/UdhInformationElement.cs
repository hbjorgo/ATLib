using HeboTech.ATLib.Extensions;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    internal abstract class UdhInformationElement
    {
        public byte Length { get; protected set; }
        public abstract string Build();
    }

    internal class ConcatenatedShortMessages : UdhInformationElement
    {
        private readonly byte messageReferenceNumber;
        private readonly byte numberOfMessageParts;
        private readonly byte partNumber;

        public ConcatenatedShortMessages(byte messageReferenceNumber, byte numberOfMessageParts, byte partNumber)
        {
            this.messageReferenceNumber = messageReferenceNumber;
            this.numberOfMessageParts = numberOfMessageParts;
            this.partNumber = partNumber;
            Length = 5;
        }

        public override string Build()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(0x00.ToByteHexString());
            sb.Append(0x03.ToByteHexString());
            sb.Append(messageReferenceNumber.ToHexString());
            sb.Append(numberOfMessageParts.ToHexString());
            sb.Append(partNumber.ToHexString());

            return sb.ToString();
        }
    }
}
