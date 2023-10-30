using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using System;
using System.Linq;

namespace HeboTech.ATLib.PDU
{
    internal class SmsDeliverDecoder
    {
        private class SmsDeliverHeader
        {
            private SmsDeliverHeader()
            {
            }

            public SmsDeliverHeader(MessageTypeIndicator mti, bool mms, bool lp, bool sri, bool udhi, bool rp)
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

            public static SmsDeliverHeader Parse(byte header)
            {
                SmsDeliverHeader parsedHeader = new SmsDeliverHeader();

                parsedHeader.MTI = (MessageTypeIndicator)(header & 0b0000_0011);
                if (parsedHeader.MTI != (byte)MessageTypeIndicator.SMS_DELIVER)
                    throw new ArgumentException("Invalid SMS-DELIVER data");

                parsedHeader.MMS = (header & 0b0000_0100) != 0;
                parsedHeader.SRI = (header & 0b0000_1000) != 0;
                parsedHeader.UDHI = (header & 0b0100_0000) != 0;
                parsedHeader.RP = (header & 0b1000_0000) != 0;

                return parsedHeader;
            }
        }

        public static SmsDeliver DecodeSmsDeliver(ReadOnlySpan<byte> bytes, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = bytes[offset++];
            PhoneNumberDTO serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = DecodePhoneNumber(bytes[offset..(offset += smsc_length)]);
            }

            // SMS-DELIVER start
            byte headerByte = bytes[offset++];
            SmsDeliverHeader header = SmsDeliverHeader.Parse(headerByte);

            byte tp_oa_nibbles_length = bytes[offset++];
            byte tp_oa_bytes_length = (byte)(tp_oa_nibbles_length % 2 == 0 ? tp_oa_nibbles_length / 2 : (tp_oa_nibbles_length / 2) + 1);
            tp_oa_bytes_length++;
            PhoneNumberDTO oa = null;
            if (tp_oa_bytes_length > 0)
            {
                oa = DecodePhoneNumber(bytes[offset..(offset += tp_oa_bytes_length)]);
            }

            byte tp_pid = bytes[offset++];

            byte tp_dcs_byte = bytes[offset++];
            if (!Enum.IsDefined(typeof(CodingScheme), tp_dcs_byte))
                throw new ArgumentException($"DCS with value {tp_dcs_byte} is not supported");
            CodingScheme tp_dcs = (CodingScheme)tp_dcs_byte;

            ReadOnlySpan<byte> tp_scts = bytes[offset..(offset += 7)];
            DateTimeOffset scts = TpduTime.DecodeTimestamp(tp_scts, timestampYearOffset);

            byte tp_udl = bytes[offset++];
            int udlBytes = 0;
            switch (tp_dcs)
            {
                case CodingScheme.Gsm7:
                    udlBytes = (int)Math.Ceiling(tp_udl * 7 / 8.0);
                    break;
                case CodingScheme.UCS2:
                    udlBytes = tp_udl;
                    break;
                default:
                    throw new ArgumentException($"DCS with value {tp_dcs} is not supported");
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
            switch (tp_dcs)
            {
                case CodingScheme.Gsm7:
                    int fillBits = 0;
                    if (header.UDHI)
                        fillBits = 7 - (((1 + udh.Length) * 8) % 7);

                    var unpacked = Gsm7.Unpack(payload.ToArray(), fillBits);
                    message = Gsm7.DecodeFromBytes(unpacked);
                    break;
                case CodingScheme.UCS2:
                    message = UCS2.Decode(payload.ToArray());
                    break;
                default:
                    throw new ArgumentException($"DCS with value {tp_dcs} is not supported");
            }

            return new SmsDeliver(serviceCenterNumber, oa, message, scts);
        }

        private static PhoneNumberDTO DecodePhoneNumber(ReadOnlySpan<byte> data)
        {
            byte ext_ton_npi = data[0];
            TypeOfNumber ton = (TypeOfNumber)((ext_ton_npi & 0b0111_0000) >> 4);

            string number = string.Empty;
            if (ton == TypeOfNumber.International)
                number = "+";
            number += string.Join("", data[1..].ToArray().Select(x => x.SwapNibbles().ToString("X2")));
            if (number[^1] == 'F')
                number = number[..^1];
            return new PhoneNumberDTO(number);
        }
    }
}
