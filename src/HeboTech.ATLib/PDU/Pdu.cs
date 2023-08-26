using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.PDU
{
#if NETSTANDARD2_0
    public class Pdu
    {
        public static string EncodeSmsSubmit(PhoneNumber phoneNumber, string encodedMessage, byte dataCodingScheme, bool includeEmptySmscLength = true)
        {
            StringBuilder sb = new StringBuilder();
            // Length of SMSC information
            if (includeEmptySmscLength)
                sb.Append("00");
            // First octed of the SMS-SUBMIT message
            sb.Append("11");
            // TP-Message-Reference. '00' lets the phone set the message reference number itself
            sb.Append("00");
            // Address length. Length of phone number (number of digits)
            sb.Append((phoneNumber.ToString().TrimStart('+').Length).ToString("X2"));
            // Type-of-Address
            sb.Append(GetAddressType(phoneNumber).ToString("X2"));
            // Phone number in semi octets. 12345678 is represented as 21436587
            sb.Append(SwapPhoneNumberDigits(phoneNumber.ToString().TrimStart('+')));
            // TP-PID Protocol identifier
            sb.Append("00");
            // TP-DCS Data Coding Scheme. '00'-7bit default alphabet. '04'-8bit
            sb.Append((dataCodingScheme).ToString("X2"));
            // TP-Validity-Period. 'AA'-4 days
            sb.Append("AA");
            // TP-User-Data-Length. If TP-DCS field indicates 7-bit data, the length is the number of septets.
            // If TP-DCS indicates 8-bit data or Unicode, the length is the number of octets.
            if (dataCodingScheme == 0)
            {
                int messageBitLength = encodedMessage.Length / 2 * 7;
                int messageLength = messageBitLength % 8 == 0 ? messageBitLength / 8 : (messageBitLength / 8) + 1;
                sb.Append((messageLength).ToString("X2"));
            }
            else
                sb.Append((encodedMessage.Length / 2 * 8 / 7).ToString("X2"));
            sb.Append(encodedMessage);

            return sb.ToString();
        }

        public static SmsDeliver DecodeSmsDeliver(ReadOnlySpan<char> text, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            PhoneNumber serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = DecodePhoneNumber(text.SliceOnIndex(offset, (offset += smsc_length * 2)));
            }

            // SMS-DELIVER start
            byte header = HexToByte(text.SliceOnIndex(offset, (offset += 2)));

            int tp_mti = header & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_DELIVER)
                throw new ArgumentException("Invalid SMS-DELIVER data");

            int tp_mms = header & 0b0000_0100;
            int tp_rp = header & 0b1000_0000;

            byte tp_oa_length = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            tp_oa_length = (byte)(tp_oa_length % 2 == 0 ? tp_oa_length : tp_oa_length + 1);
            PhoneNumber oa = null;
            if (tp_oa_length > 0)
            {
                int oa_digits = tp_oa_length + 2; // Add 2 for TON
                oa = DecodePhoneNumber(text.SliceOnIndex(offset, (offset += oa_digits)));
            }
            byte tp_pid = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            byte tp_dcs = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            ReadOnlySpan<char> tp_scts = text.SliceOnIndex(offset, (offset += 14));
            byte tp_udl = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            int udlBytes = (int)Math.Ceiling(tp_udl * 7 / 8.0);

            ReadOnlySpan<char> tp_ud = text.SliceOnIndex(offset, (offset += ((udlBytes) * 2)));
            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    message = Gsm7.Decode(tp_ud.ToString());
                    break;
                default:
                    break;
            }
            DateTimeOffset scts = DecodeTimestamp(tp_scts, timestampYearOffset);
            return new SmsDeliver(serviceCenterNumber, oa, message, scts);
        }

        public static SmsSubmit DecodeSmsSubmit(ReadOnlySpan<char> text, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            PhoneNumber serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = DecodePhoneNumber(text.SliceOnIndex(offset, (offset += smsc_length * 2)));
            }

            // SMS-DELIVER start
            byte header = HexToByte(text.SliceOnIndex(offset, (offset += 2)));

            int tp_mti = header & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_SUBMIT)
                throw new ArgumentException("Invalid SMS-SUBMIT data");

            int tp_rd = header & 0b0000_0100;
            int tp_vpf = header & 0b0001_1000;
            int tp_rp = header & 0b1000_0000;

            byte tp_mr = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            byte tp_oa_length = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            tp_oa_length = (byte)(tp_oa_length % 2 == 0 ? tp_oa_length : tp_oa_length + 1);
            PhoneNumber oa = null;
            if (tp_oa_length > 0)
            {
                int oa_digits = tp_oa_length + 2; // Add 2 for TON
                oa = DecodePhoneNumber(text.SliceOnIndex(offset, (offset += oa_digits)));
            }
            byte tp_pid = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            byte tp_dcs = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            byte tp_vp = 0;
            if (tp_vpf == 0x00)
                tp_vp = HexToByte(text.SliceOnIndex(offset, (offset += 0)));
            else if (tp_vpf == 0x01)
                tp_vp = HexToByte(text.SliceOnIndex(offset, (offset += 14)));
            else if (tp_vpf == 0x10)
                tp_vp = HexToByte(text.SliceOnIndex(offset, (offset += 2)));
            else if (tp_vpf == 0x11)
                tp_vp = HexToByte(text.SliceOnIndex(offset, (offset += 14)));
            byte tp_udl = HexToByte(text.SliceOnIndex(offset, (offset += 2)));

            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    int length = (tp_udl * 7 / 8) + 1;
                    ReadOnlySpan<char> tp_ud = text.SliceOnIndex(offset, (offset += ((length) * 2)));
                    message = Gsm7.Decode(tp_ud.ToString());
                    break;
                default:
                    break;
            }
            return new SmsSubmit(serviceCenterNumber, oa, message);
        }

        private static byte HexToByte(ReadOnlySpan<char> text)
        {
            byte retVal = (byte)int.Parse(text.ToString(), NumberStyles.HexNumber);
            return retVal;
        }

        private static char[] SwapPhoneNumberDigits(string data)
        {
            char[] swappedData = new char[data.Length];
            for (int i = 0; i < data.Length; i += 2)
            {
                swappedData[i] = data[i + 1];
                swappedData[i + 1] = data[i];
            }
            if (swappedData[swappedData.Length - 1] == 'F')
            {
                char[] subArray = new char[swappedData.Length - 1];
                Array.Copy(swappedData, subArray, subArray.Length);
                return subArray;
            }
            return swappedData;
        }

        private static byte GetAddressType(PhoneNumber phoneNumber)
        {
            return (byte)(0b1000_0000 + (byte)phoneNumber.GetTypeOfNumber() + (byte)phoneNumber.GetNumberPlanIdentification());
        }

        private static PhoneNumber DecodePhoneNumber(ReadOnlySpan<char> data)
        {
            if (data.Length < 4)
                return default;
            TypeOfNumber ton = (TypeOfNumber)((HexToByte(data.Slice(0, 2)) & 0b0111_0000) >> 4);
            string number = string.Empty;
            if (ton == TypeOfNumber.International)
                number = "+";
            number += new String(SwapPhoneNumberDigits(data.Slice(2).ToString()));
            return new PhoneNumber(number);
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

            byte offset = DecimalToByte(swappedSpan.SliceOnIndex(12, 14));
            bool positive = (offset & (1 << 7)) == 0;
            byte offsetQuarters = (byte)(offset & 0b0111_1111);

            DateTimeOffset timestamp = new DateTimeOffset(
                DecimalToByte(swappedSpan.SliceOnIndex(0, 2)) + timestampYearOffset,
                DecimalToByte(swappedSpan.SliceOnIndex(2, 4)),
                DecimalToByte(swappedSpan.SliceOnIndex(4, 6)),
                DecimalToByte(swappedSpan.SliceOnIndex(6, 8)),
                DecimalToByte(swappedSpan.SliceOnIndex(8, 10)),
                DecimalToByte(swappedSpan.SliceOnIndex(10, 12)),
                TimeSpan.FromMinutes(offsetQuarters * 15)); // Offset in quarter of hours
            return timestamp;
        }

        private static byte DecimalToByte(ReadOnlySpan<char> text)
        {
            return (byte)int.Parse(text.ToString(), NumberStyles.Integer);
        }
    }

#elif NETSTANDARD2_1_OR_GREATER
    public class Pdu
    {
        public static string EncodeSmsSubmit(
            PhoneNumber phoneNumber,
            string encodedMessage,
            CodingScheme dataCodingScheme,
            bool includeEmptySmscLength = true)
        {
            if (encodedMessage.Length > 160)
                throw new ArgumentException("Maximum length exceeded (160)", nameof(encodedMessage));

            //return EncodeMultipartSmsSubmit(phoneNumber, encodedMessage, dataCodingScheme, includeEmptySmscLength).First();
            return string.Empty;
        }

        public static IEnumerable<string> EncodeMultipartSmsSubmit(
            PhoneNumber phoneNumber,
            string message,
            CodingScheme dataCodingScheme,
            bool includeEmptySmscLength = true)
        {
            //if (encodedMessage.Length > 160 * 255)
            //    throw new ArgumentOutOfRangeException(nameof(encodedMessage), "Maximum length exceeded (160 * 255)");

            byte[] encodedMessage = Gsm7.EncodeToBytes(message);

            // Single message
            // 140 octets equals 160 septets. Because of string representation, double it (two chars per octet)
            if (message.Length <= 2 * 140)
            {
                StringBuilder sb = new StringBuilder();

                // Length of SMSC information
                if (includeEmptySmscLength)
                    sb.Append("00");

                // Build TPDU
                sb.Append(SmsSubmitBuilder
                    .Initialize()
                    .ValidityPeriodFormat(0x10)
                    .DestinationAddress(phoneNumber)
                    .ValidityPeriod(0xAA)
                    .DataCodingScheme(dataCodingScheme)
                    .UserData(encodedMessage)
                    .Build());

                yield return sb.ToString();
            }
            // Concatenated messages
            else
            {
                //var messageParts = encodedMessage.SplitByLength(2 * 134); // 140 - 6 = 134. 6 octets for UDH
                int numberOfMessageParts = (int)Math.Ceiling(encodedMessage.Length / 134d);
                byte messageReferenceNumber = (byte)new Random(DateTime.UtcNow.Millisecond).Next(255);

                for (var i = 0; i < numberOfMessageParts; i++)
                {
                    ConcatenatedShortMessages csms = new ConcatenatedShortMessages(
                        messageReferenceNumber,
                        (byte)numberOfMessageParts,
                        (byte)(i + 1));

                    StringBuilder sb = new StringBuilder();

                    // Length of SMSC information
                    if (includeEmptySmscLength)
                        sb.Append("00");

                    // Build TPDU
                    sb.Append(SmsSubmitBuilder
                        .Initialize()
                        .EnableUserDataHeaderIndicator()
                        .ValidityPeriodFormat(0x10)
                        .DestinationAddress(phoneNumber)
                        .ValidityPeriod(0xAA)
                        .DataCodingScheme(dataCodingScheme)
                        .AddUdhInformationElement(csms)
                        .UserData(encodedMessage.Skip(i * numberOfMessageParts * 134).Take(i * numberOfMessageParts * 134).ToArray()) //messageParts.ElementAt(i))
                        .Build());

                    Console.WriteLine(sb.ToString());

                    yield return sb.ToString();
                }
            }
        }

        public static SmsDeliver DecodeSmsDeliver(ReadOnlySpan<char> text, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text[offset..(offset += 2)]);
            PhoneNumber serviceCenterNumber = null;
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
            PhoneNumber oa = null;
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
                    message = Gsm7.Decode(new string(tp_ud));
                    break;
                default:
                    break;
            }
            DateTimeOffset scts = DecodeTimestamp(tp_scts, timestampYearOffset);
            return new SmsDeliver(serviceCenterNumber, oa, message, scts);
        }

        public static SmsSubmit DecodeSmsSubmit(ReadOnlySpan<char> text, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text[offset..(offset += 2)]);
            PhoneNumber serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = DecodePhoneNumber(text[offset..(offset += smsc_length * 2)]);
            }

            // SMS-DELIVER start
            byte header = HexToByte(text[offset..(offset += 2)]);

            int tp_mti = header & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_SUBMIT)
                throw new ArgumentException("Invalid SMS-SUBMIT data");

            int tp_rd = header & 0b0000_0100;
            int tp_vpf = header & 0b0001_1000;
            int tp_rp = header & 0b1000_0000;

            byte tp_mr = HexToByte(text[offset..(offset += 2)]);
            byte tp_oa_length = HexToByte(text[offset..(offset += 2)]);
            tp_oa_length = (byte)(tp_oa_length % 2 == 0 ? tp_oa_length : tp_oa_length + 1);
            PhoneNumber oa = null;
            if (tp_oa_length > 0)
            {
                int oa_digits = tp_oa_length + 2; // Add 2 for TON
                oa = DecodePhoneNumber(text[offset..(offset += oa_digits)]);
            }
            byte tp_pid = HexToByte(text[offset..(offset += 2)]);
            byte tp_dcs = HexToByte(text[offset..(offset += 2)]);
            byte tp_vp = 0;
            if (tp_vpf == 0x00)
                tp_vp = HexToByte(text[offset..(offset += 0)]);
            else if (tp_vpf == 0x01)
                tp_vp = HexToByte(text[offset..(offset += 14)]);
            else if (tp_vpf == 0x10)
                tp_vp = HexToByte(text[offset..(offset += 2)]);
            else if (tp_vpf == 0x11)
                tp_vp = HexToByte(text[offset..(offset += 14)]);
            byte tp_udl = HexToByte(text[offset..(offset += 2)]);

            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    int length = (tp_udl * 7 / 8) + 1;
                    ReadOnlySpan<char> tp_ud = text[offset..(offset += ((length) * 2))];
                    message = Gsm7.Decode(new string(tp_ud));
                    break;
                default:
                    break;
            }
            return new SmsSubmit(serviceCenterNumber, oa, message);
        }

        private static byte HexToByte(ReadOnlySpan<char> text)
        {
            byte retVal = (byte)int.Parse(text, NumberStyles.HexNumber);
            return retVal;
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

        private static byte GetAddressType(PhoneNumber phoneNumber)
        {
            return (byte)(0b1000_0000 + (byte)phoneNumber.GetTypeOfNumber() + (byte)phoneNumber.GetNumberPlanIdentification());
        }

        private static PhoneNumber DecodePhoneNumber(ReadOnlySpan<char> data)
        {
            if (data.Length < 4)
                return default;
            TypeOfNumber ton = (TypeOfNumber)((HexToByte(data[0..2]) & 0b0111_0000) >> 4);
            string number = string.Empty;
            if (ton == TypeOfNumber.International)
                number = "+";
            number += new String(SwapPhoneNumberDigits(data[2..]));
            return new PhoneNumber(number);
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
#endif
}
