using HeboTech.ATLib.DTOs;
using System;

namespace HeboTech.ATLib.PDU
{
    public static class SmsStatusReportDecoder
    {
        private class SmsStatusReportHeader
        {
            private SmsStatusReportHeader()
            {
            }

            public SmsStatusReportHeader(MessageTypeIndicator mti, bool mms, bool lp, bool sri, bool udhi, bool rp)
            {
                MTI = mti;
                MMS = mms;
                LP = lp;
                SRI = sri;
                UDHI = udhi;
                RP = rp;
            }

            public MessageTypeIndicator MTI { get; private set; }
            public bool MMS { get; private set; }
            public bool LP { get; private set; }
            public bool SRI { get; private set; }
            public bool UDHI { get; private set; }
            public bool RP { get; private set; }

            public static SmsStatusReportHeader Parse(byte header)
            {
                SmsStatusReportHeader parsedHeader = new SmsStatusReportHeader();

                parsedHeader.MTI = (MessageTypeIndicator)(header & 0b0000_0011);
                if (parsedHeader.MTI != MessageTypeIndicator.SMS_STATUS_REPORT)
                    throw new ArgumentException("Invalid SMS_STATUS_REPORT data");

                parsedHeader.MMS = (header & 0b0000_0100) != 0;
                parsedHeader.SRI = (header & 0b0000_1000) != 0;
                parsedHeader.UDHI = (header & 0b0100_0000) != 0;
                parsedHeader.RP = (header & 0b1000_0000) != 0;

                return parsedHeader;
            }
        }

        public static SmsStatusReport Decode(byte length, ReadOnlySpan<byte> bytes, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = (byte)(bytes.Length - length);
            PhoneNumberDTO serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = PhoneNumberDecoder.DecodePhoneNumber(bytes[offset..(offset += smsc_length)]);
            }

            // SMS-STATUS-REPORT start
            byte headerByte = bytes[offset++];
            SmsStatusReportHeader header = SmsStatusReportHeader.Parse(headerByte);

            byte tp_mr = bytes[offset++];
            byte tp_ra_length = (byte)((bytes[offset++] / 2) + 1);
            ReadOnlySpan<byte> tp_ra = bytes[offset..(offset += tp_ra_length)];
            ReadOnlySpan<byte> tp_scts = bytes[offset..(offset += 7)];
            ReadOnlySpan<byte> tp_dt = bytes[offset..(offset += 7)];
            byte tp_st = bytes[offset++];

            return new SmsStatusReport(tp_mr, PhoneNumberDecoder.DecodePhoneNumber(tp_ra), TpduTime.DecodeTimestamp(tp_scts, timestampYearOffset), TpduTime.DecodeTimestamp(tp_dt, timestampYearOffset), (SmsDeliveryStatus) tp_st);
        }
    }
}
