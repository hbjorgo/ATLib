using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Linq;
using UnitsNet;

namespace HeboTech.ATLib.PDU
{
    internal class SmsDeliverDecoder
    {
        private class SmsDeliverHeader
        {
            private SmsDeliverHeader()
            {
            }

            public SmsDeliverHeader(MTI mti, bool mms, bool lp, bool sri, bool udhi, bool rp)
            {
                MTI = mti;
                MMS = mms;
                LP = lp;
                SRI = sri;
                UDHI = udhi;
                RP = rp;
            }

            public MTI MTI { get; private set; }
            public bool MMS { get; private set; }
            public bool LP { get; private set; }
            public bool SRI { get; private set; }
            public bool UDHI { get; private set; }
            public bool RP { get; private set; }

            public static SmsDeliverHeader Parse(byte header)
            {
                SmsDeliverHeader parsedHeader = new SmsDeliverHeader();

                parsedHeader.MTI = (MTI)(header & 0b0000_0011);
                if (parsedHeader.MTI != (byte)MTI.SMS_DELIVER)
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
            byte udhl = 0;
            ReadOnlySpan<byte> udh;
            ReadOnlySpan<byte> payload;
            if (header.UDHI)
            {
                udhl = tp_ud[0];
                udh = tp_ud[1..(udhl + 1)];
                payload = tp_ud[(udhl + 1)..];
            }
            else
            {
                payload = tp_ud;
            }

            string message = null;
            switch (tp_dcs)
            {
                case CodingScheme.Gsm7:
                    int fillBits = 0;
                    if (header.UDHI)
                        fillBits = 7 - (((1 + udhl) * 8) % 7);

                    var unpacked = Gsm7.Unpack(payload.ToArray(), fillBits);
                    message = Gsm7.DecodeFromBytes(unpacked);
                    break;
                case CodingScheme.UCS2:
                    message = UCS2.Decode(payload.ToArray());
                    break;
                default:
                    throw new ArgumentException($"DCS with value {tp_dcs} is not supported");
            }
            DateTimeOffset scts = DecodeTimestamp(tp_scts, timestampYearOffset);
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

        private static DateTimeOffset DecodeTimestamp(ReadOnlySpan<byte> data, int timestampYearOffset = 2000)
        {
            byte[] swappedData = data.ToArray().Select(x => x.SwapNibbles()).ToArray();

            byte year = swappedData[0].BcdToDecimal();
            byte month = swappedData[1].BcdToDecimal();
            byte day = swappedData[2].BcdToDecimal();
            byte hour = swappedData[3].BcdToDecimal();
            byte minute = swappedData[4].BcdToDecimal();
            byte second = swappedData[5].BcdToDecimal();
            byte offsetQuarters = ((byte)(swappedData[6] & 0b0111_1111)).BcdToDecimal();
            bool isOffsetPositive = (swappedData[6] & 0b1000_0000) == 0;

            DateTimeOffset timestamp = new DateTimeOffset(
                year + timestampYearOffset,
                month,
                day,
                hour,
                minute,
                second,
                TimeSpan.FromMinutes(offsetQuarters * 15)); // Offset in quarter of hours
            return timestamp;
        }
    }
}
