using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using System;
using System.Globalization;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    internal class SmsDeliverDecoder
    {
        public static SmsDeliver DecodeSmsDeliver(ReadOnlySpan<char> text, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text[offset..(offset += 2)]);
            PhoneNumberDTO serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = DecodePhoneNumber(text[offset..(offset += smsc_length * 2)]);
            }

            // SMS-DELIVER start
            byte header = HexToByte(text[offset..(offset += 2)]);

            int tp_mti = header & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_DELIVER)
                throw new ArgumentException("Invalid SMS-DELIVER data");

            int tp_mms = header & 0b0000_0100;
            int tp_rp = header & 0b1000_0000;

            byte tp_oa_length = HexToByte(text[offset..(offset += 2)]);
            tp_oa_length = (byte)(tp_oa_length % 2 == 0 ? tp_oa_length : tp_oa_length + 1);
            PhoneNumberDTO oa = null;
            if (tp_oa_length > 0)
            {
                int oa_digits = tp_oa_length + 2; // Add 2 for TON
                oa = DecodePhoneNumber(text[offset..(offset += oa_digits)]);
            }
            byte tp_pid = HexToByte(text[offset..(offset += 2)]);
            byte tp_dcs = HexToByte(text[offset..(offset += 2)]);
            ReadOnlySpan<char> tp_scts = text[offset..(offset += 14)];
            byte tp_udl = HexToByte(text[offset..(offset += 2)]);
            int udlBytes = (int)Math.Ceiling(tp_udl * 7 / 8.0);

            ReadOnlySpan<char> tp_ud = text[offset..(offset += ((udlBytes) * 2))];
            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    // TODO: Fix

                    string tp_ud_asString = new string(tp_ud);
                    byte[] tp_ud_asByteArray = tp_ud_asString.FromHexStringToByteArray();
                    var unpacked = Gsm7.Unpack(tp_ud_asByteArray);
                    message = Encoding.ASCII.GetString(unpacked);
                    break;
                default:
                    break;
            }
            DateTimeOffset scts = DecodeTimestamp(tp_scts, timestampYearOffset);
            return new SmsDeliver(serviceCenterNumber, oa, message, scts);
        }

        private static byte HexToByte(ReadOnlySpan<char> text)
        {
            byte retVal = (byte)int.Parse(text, NumberStyles.HexNumber);
            return retVal;
        }

        private static PhoneNumberDTO DecodePhoneNumber(ReadOnlySpan<char> data)
        {
            if (data.Length < 4)
                return default;
            TypeOfNumber ton = (TypeOfNumber)((HexToByte(data[0..2]) & 0b0111_0000) >> 4);
            string number = string.Empty;
            if (ton == TypeOfNumber.International)
                number = "+";
            number += new String(SwapPhoneNumberDigits(data[2..]));
            return new PhoneNumberDTO(number);
        }

        private static char[] SwapPhoneNumberDigits(ReadOnlySpan<char> data)
        {
            char[] swappedData = new char[data.Length];
            for (int i = 0; i < data.Length; i += 2)
            {
                swappedData[i] = data[i + 1];
                swappedData[i + 1] = data[i];
            }
            if (swappedData[^1] == 'F')
                return swappedData[..^1];
            return swappedData;
        }

        private static DateTimeOffset DecodeTimestamp(ReadOnlySpan<char> data, int timestampYearOffset = 2000)
        {
            char[] swappedData = new char[data.Length];
            for (int i = 0; i < swappedData.Length; i += 2)
            {
                swappedData[i] = data[i + 1];
                swappedData[i + 1] = data[i];
            }
            ReadOnlySpan<char> swappedSpan = swappedData;

            byte offset = DecimalToByte(swappedSpan[12..14]);
            bool positive = (offset & (1 << 7)) == 0;
            byte offsetQuarters = (byte)(offset & 0b0111_1111);

            DateTimeOffset timestamp = new DateTimeOffset(
                DecimalToByte(swappedSpan[..2]) + timestampYearOffset,
                DecimalToByte(swappedSpan[2..4]),
                DecimalToByte(swappedSpan[4..6]),
                DecimalToByte(swappedSpan[6..8]),
                DecimalToByte(swappedSpan[8..10]),
                DecimalToByte(swappedSpan[10..12]),
                TimeSpan.FromMinutes(offsetQuarters * 15)); // Offset in quarter of hours
            return timestamp;

            static byte DecimalToByte(ReadOnlySpan<char> text)
            {
                return (byte)int.Parse(text, NumberStyles.Integer);
            }
        }
    }
}
