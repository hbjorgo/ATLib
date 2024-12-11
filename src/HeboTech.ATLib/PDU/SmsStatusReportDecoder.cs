using HeboTech.ATLib.DTOs;
using System;

namespace HeboTech.ATLib.PDU
{
    internal static class SmsStatusReportDecoder
    {
        private class SmsStatusReportHeader
        {
            private SmsStatusReportHeader()
            {
            }

            public SmsStatusReportHeader(MessageTypeIndicatorInbound mti, bool mms, bool lp, bool sri, bool udhi)
            {
                MTI = mti;
                MMS = mms;
                LP = lp;
                SRQ = sri;
                UDHI = udhi;
            }

            public MessageTypeIndicatorInbound MTI { get; private set; }
            public bool MMS { get; private set; }
            public bool LP { get; private set; }
            /// <summary>
            /// If set - this is a result of an SMS-COMMAND; otherwise it is a result of an SMS-SUBMIT
            /// 9 2 2 3
            /// </summary>
            public bool SRQ { get; private set; }
            public bool UDHI { get; private set; }

            public static SmsStatusReportHeader Parse(byte header)
            {
                SmsStatusReportHeader parsedHeader = new SmsStatusReportHeader();

                parsedHeader.MTI = (MessageTypeIndicatorInbound)(header & (3 << 0));
                if (parsedHeader.MTI != MessageTypeIndicatorInbound.SMS_STATUS_REPORT)
                    throw new ArgumentException("Invalid SMS_STATUS_REPORT data");

                parsedHeader.MMS = (header & (1 << 2)) != 0;
                parsedHeader.LP = (header & (1 << 3)) != 0;
                parsedHeader.SRQ = (header & (1 << 5)) != 0;
                parsedHeader.UDHI = (header & (1 << 6)) != 0;

                return parsedHeader;
            }
        }

        public static SmsStatusReport Decode(ReadOnlySpan<byte> bytes, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = bytes[offset++];
            PhoneNumberDto serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = PhoneNumberDecoder.DecodePhoneNumber(bytes[offset..(offset += smsc_length)]);
            }

            // SMS-STATUS-REPORT start
            byte headerByte = bytes[offset++];
            SmsStatusReportHeader header = SmsStatusReportHeader.Parse(headerByte);

            byte tp_mr = bytes[offset++];

            byte tp_ra_nibbles_length = bytes[offset++];
            byte tp_ra_bytes_length = (byte)(tp_ra_nibbles_length % 2 == 0 ? tp_ra_nibbles_length / 2 : (tp_ra_nibbles_length + 1) / 2);
            tp_ra_bytes_length++;
            ReadOnlySpan<byte> tp_ra = bytes[offset..(offset += tp_ra_bytes_length)];

            //byte tp_ra_length = (byte)((bytes[offset++] / 2) + 1);
            //ReadOnlySpan<byte> tp_ra = bytes[offset..(offset += tp_ra_length)];
            ReadOnlySpan<byte> tp_scts = bytes[offset..(offset += 7)];
            ReadOnlySpan<byte> tp_dt = bytes[offset..(offset += 7)];
            byte tp_st = bytes[offset++];

            return new SmsStatusReport(tp_mr, PhoneNumberDecoder.DecodePhoneNumber(tp_ra), serviceCenterNumber, TpduTime.DecodeTimestamp(tp_scts, timestampYearOffset), TpduTime.DecodeTimestamp(tp_dt, timestampYearOffset), (SmsDeliveryStatus) tp_st);
        }
    }
}
