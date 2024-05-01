using System;

namespace HeboTech.ATLib.PDU
{
    internal static class ReceivedMessageTypeParser
    {
        public static MessageTypeIndicatorInbound Parse(ReadOnlySpan<byte> bytes)
        {
            byte smsc_length = bytes[0]; // Get length of SMSC
            byte headerByte = bytes[1 + smsc_length]; // Skip over SMSC length byte and SMSC to get to header

            byte mti = (byte)(headerByte & 0b0000_0011);
            if (Enum.IsDefined(typeof(MessageTypeIndicatorInbound), mti))
                return (MessageTypeIndicatorInbound)mti;
            throw new ArgumentOutOfRangeException(nameof(mti));
        }
    }
}
