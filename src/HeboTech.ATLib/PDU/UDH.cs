using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.PDU
{
    public class UDH
    {
        private UDH(byte length, IEnumerable<InformationElement> informationElements)
        {
            Length = length;
            InformationElements = informationElements;
        }

        public byte Length { get; }
        public IEnumerable<InformationElement> InformationElements { get; }

        public static UDH Empty() => new UDH(0, Array.Empty<InformationElement>());

        public static UDH Parse(byte totalLength, ReadOnlySpan<byte> data)
        {
            List<InformationElement> informationElements = new List<InformationElement>();
            for (int i = 0; i < totalLength;)
            {
                byte iei = data[i];
                byte length = data[i + 1];
                ReadOnlySpan<byte> payload = data[(i + 1)..(i + 1 + length)];
                informationElements.Add(new InformationElement(iei, length, payload.ToArray()));
                i += 2 + length;
            }

            return new UDH(totalLength, informationElements);
        }
    }

    public class InformationElement
    {
        public InformationElement(byte iei, byte length, byte[] data)
        {
            IEI = iei;
            Length = length;
            Data = data;
        }

        public byte IEI { get; }
        public byte Length { get; }
        public byte[] Data { get; }
    }
}
