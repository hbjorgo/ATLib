using System;

namespace HeboTech.ATLib.Messaging
{
    public class SmsDecoder
    {
        public static Sms Decode(ReadOnlySpan<byte> bytes, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = bytes[offset++];
            // Skip SMSC information - handle it in each individual parser.
            offset += smsc_length;
            // Header
            byte headerByte = bytes[offset++];

            MessageTypeIndicatorInbound mti = (MessageTypeIndicatorInbound)(headerByte & 0b0000_0011);
            switch (mti)
            {
                case MessageTypeIndicatorInbound.SMS_DELIVER:
                    return SmsDeliverDecoder.Decode(bytes, timestampYearOffset);
                case MessageTypeIndicatorInbound.SMS_SUBMIT_REPORT:
                    break;
                case MessageTypeIndicatorInbound.SMS_STATUS_REPORT:
                    return SmsStatusReportDecoder.Decode(bytes, timestampYearOffset);
                case MessageTypeIndicatorInbound.Reserved:
                    break;
                default:
                    break;
            }

            throw new NotImplementedException();
        }
    }
}
