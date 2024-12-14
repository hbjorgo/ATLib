using HeboTech.ATLib.Numbering;
using System;
using System.Linq;

namespace HeboTech.ATLib.Messaging
{
    public class SmsDeliverDecoder
    {
        private class SmsDeliverHeader
        {
            private SmsDeliverHeader()
            {
            }

            public SmsDeliverHeader(MessageTypeIndicatorInbound mti, bool mms, bool lp, bool sri, bool udhi, bool rp)
            {
                MTI = mti;
                MMS = mms;
                LP = lp;
                SRI = sri;
                UDHI = udhi;
                RP = rp;
            }

            public MessageTypeIndicatorInbound MTI { get; private set; }
            public bool MMS { get; private set; }
            public bool LP { get; private set; }
            public bool SRI { get; private set; }
            public bool UDHI { get; private set; }
            public bool RP { get; private set; }

            public static SmsDeliverHeader Parse(byte header)
            {
                SmsDeliverHeader parsedHeader = new SmsDeliverHeader();

                parsedHeader.MTI = (MessageTypeIndicatorInbound)(header & 0b0000_0011);
                if (parsedHeader.MTI != (byte)MessageTypeIndicatorInbound.SMS_DELIVER)
                    throw new ArgumentException("Invalid SMS-DELIVER data");

                parsedHeader.MMS = (header & 1 << 2) == 0;
                parsedHeader.SRI = (header & 1 << 3) != 0;
                parsedHeader.UDHI = (header & 1 << 6) != 0;
                parsedHeader.RP = (header & 1 << 7) != 0;

                return parsedHeader;
            }
        }

        /// <summary>
        /// Decodes SMS-Deliver bytes in PDU format
        /// </summary>
        /// <param name="bytes">Data</param>
        /// <param name="timestampYearOffset">Year offset</param>
        /// <returns>A decoded SMS-Deliver object</returns>
        /// <exception cref="ArgumentException"></exception>
        public static SmsDeliver Decode(ReadOnlySpan<byte> bytes, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = bytes[offset++];
            PhoneNumber serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = PhoneNumberDecoder.DecodePhoneNumber(bytes[offset..(offset += smsc_length)]);
            }

            // SMS-DELIVER start
            byte headerByte = bytes[offset++];
            SmsDeliverHeader header = SmsDeliverHeader.Parse(headerByte);

            byte tp_oa_nibbles_length = bytes[offset++];
            byte tp_oa_bytes_length = (byte)(tp_oa_nibbles_length % 2 == 0 ? tp_oa_nibbles_length / 2 : (tp_oa_nibbles_length + 1) / 2);
            tp_oa_bytes_length++;
            PhoneNumber oa = null;
            if (tp_oa_bytes_length > 0)
            {
                oa = PhoneNumberDecoder.DecodePhoneNumber(bytes[offset..(offset += tp_oa_bytes_length)]);
            }

            byte tp_pid = bytes[offset++];

            byte tp_dcs_byte = bytes[offset++];
            DataCodingScheme dcs = DataCodingScheme.ParseByte(tp_dcs_byte);

            ReadOnlySpan<byte> tp_scts = bytes[offset..(offset += 7)];
            DateTimeOffset scts = TpduTime.DecodeTimestamp(tp_scts, timestampYearOffset);

            byte tp_udl = bytes[offset++];
            int udlBytes = 0;
            switch (dcs.CharacterSet)
            {
                case CharacterSet.Gsm7:
                    udlBytes = (int)Math.Ceiling(tp_udl * 7 / 8.0);
                    break;
                case CharacterSet.UCS2:
                    udlBytes = tp_udl;
                    break;
                default:
                    throw new ArgumentException($"DCS with value {dcs.CharacterSet} is not supported");
            }

            ReadOnlySpan<byte> tp_ud = bytes[offset..(offset += udlBytes)];
            Udh udh;
            ReadOnlySpan<byte> payload;
            if (header.UDHI)
            {
                byte udhl = tp_ud[0];
                ReadOnlySpan<byte> udh_bytes = tp_ud[1..(udhl + 1)];
                udh = Udh.Parse(udhl, udh_bytes);
                payload = tp_ud[(udhl + 1)..];
            }
            else
            {
                udh = Udh.Empty();
                payload = tp_ud;
            }

            string message;
            switch (dcs.CharacterSet)
            {
                case CharacterSet.Gsm7:
                    int fillBits = 0;
                    if (header.UDHI)
                        fillBits = (1 + udh.Length) % 7 == 0 ? 0 : 7 - (1 + udh.Length) * 8 % 7; // Add 1 to the udh length because the length byte isn't included in the udh length itself. If fillbits == 7 -> use 0 fillbits.
                    message = Gsm7.Decode(payload.ToArray(), fillBits);
                    break;
                case CharacterSet.UCS2:
                    message = UCS2.Decode(payload.ToArray());
                    break;
                default:
                    throw new ArgumentException($"DCS with value {dcs.CharacterSet} is not supported");
            }

            InformationElement concatenatedSms = udh.InformationElements.FirstOrDefault(x => x.IEI == (byte)IEI.ConcatenatedShortMessages);
            if (concatenatedSms != null)
                return new SmsDeliver(serviceCenterNumber, oa, message, scts, concatenatedSms.Data[0], concatenatedSms.Data[1], concatenatedSms.Data[2]);
            else
                return new SmsDeliver(serviceCenterNumber, oa, message, scts);
        }
    }
}
