using System;
using System.Collections.Generic;

namespace HeboTech.ATLib.PDU
{
    public enum IEI : byte
    {
        ConcatenatedShortMessages = 0x00,
        //NationalLanguageSingleShift = 0x24,
        //NationalLanguageLockingShift = 0x25,
    }

    public class Udh
    {
        private Udh(byte length, IEnumerable<InformationElement> informationElements)
        {
            Length = length;
            InformationElements = informationElements;
        }

        public byte Length { get; }
        public IEnumerable<InformationElement> InformationElements { get; }

        public static Udh Empty() => new Udh(0, Array.Empty<InformationElement>());

        public static Udh Parse(byte totalLength, ReadOnlySpan<byte> data)
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

            return new Udh(totalLength, informationElements);
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
