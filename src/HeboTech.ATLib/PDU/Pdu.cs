using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using System;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    public class Pdu
    {
        public static string Encode(PhoneNumber phoneNumber, string encodedMessage, byte dataCodingScheme)
        {
            StringBuilder sb = new StringBuilder();
            // Length of SMSC information
            sb.Append("00");
            // First octed of the SMS-SUBMIT message
            sb.Append("11");
            // TP-Message-Reference. '00' lets the phone set the message reference number itself
            sb.Append("00");
            // Address length. Length of phone number (number of digits)
            sb.Append((phoneNumber.ToString().Length).ToString("X2"));
            // Type-of-Address (91 - international format, 81 - national format)
            sb.Append(((int)phoneNumber.Format).ToString("X2"));
            // Phone number in semi octets. 12345678 is represented as 21436587
            sb.Append(SwapPhoneNumberDigits(phoneNumber.ToString()));
            // TP-PID Protocol identifier
            sb.Append("00");
            // TP-DCS Data Coding Scheme. '00'-7bit default alphabet. '04'-8bit
            sb.Append(dataCodingScheme.ToString("X2"));
            // TP-Validity-Period. 'AA'-4 days
            sb.Append("AA");
            // TP-User-Data-Length. If TP-DCS field indicates 7-bit data, the length is the number of septets.
            // If TP-DCS indicates 8-bit data or Unicode, the length is the number of octets.
            sb.Append((encodedMessage.Length / 2 * 8 / 7).ToString("X2"));
            sb.Append(encodedMessage);

            return sb.ToString();
        }

        private static string SwapPhoneNumberDigits(string value)
        {
            char[] split = value.ToCharArray();
            for (int i = 0; i < split.Length; i += 2)
            {
                char temp = split[i];
                split[i] = split[i + 1];
                split[i + 1] = temp;
            }
            return new string(split);
        }

        public static PduMessage Decode(string data, int timestampYearOffset = 2000)
        {
            int offset = 0;
            int smscLength = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int smscAddressType = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string serviceCenterNumber = null;
            // National
            if (smscAddressType == (int)PhoneNumberFormat.National)
            {
                for (int i = 0; i < smscLength - 1; i++)
                {
                    string temp = data[offset..(offset += 2)];
                    serviceCenterNumber += temp[1];
                    if (temp[0] != 'F')
                        serviceCenterNumber += temp[0];
                }
            }
            // International
            else if (smscAddressType == (int)PhoneNumberFormat.International)
            {
                serviceCenterNumber += '+';
                for (int i = 0; i < smscLength - 1; i++)
                {
                    string temp = data[offset..(offset += 2)];
                    serviceCenterNumber += temp[1];
                    if (temp[0] != 'F')
                        serviceCenterNumber += temp[0];
                }
            }

            int firstSmsDeliverOctet = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int senderAddressLength = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int senderAddressType = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string senderNumber = null;
            // TODO: What types are there here?
            if (senderAddressType == 0xC8 || senderAddressType == 0x91)
            {
                senderNumber += '+';
                for (int i = 0; i < (int)Math.Ceiling(senderAddressLength / 2f); i++)
                {
                    string temp = data[offset..(offset += 2)];
                    senderNumber += temp[1];
                    if (temp[0] != 'F')
                        senderNumber += temp[0];
                }
            }

            int tpPid = Convert.ToInt32(data[offset..(offset += 2)], 16);
            int tpDcs = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string tpScts = null;
            for (int i = 0; i < 7; i++)
            {
                string temp = data[offset..(offset += 2)];
                tpScts += temp[1];
                tpScts += temp[0];
            }
            DateTimeOffset timeStamp = new DateTimeOffset(
                int.Parse(tpScts[..2]) + timestampYearOffset,
                int.Parse(tpScts[2..4]),
                int.Parse(tpScts[4..6]),
                int.Parse(tpScts[6..8]),
                int.Parse(tpScts[8..10]),
                int.Parse(tpScts[10..12]),
                TimeSpan.FromMinutes(int.Parse(tpScts[12..14]) * 15)); // Offset in quarter of hours
            int tpUdl = Convert.ToInt32(data[offset..(offset += 2)], 16);
            string tpUd = data[offset..];

            string decodedMessage = tpUd;
            switch (tpDcs)
            {
                case Gsm7.DataCodingSchemeCode:
                    decodedMessage = Gsm7.Decode(tpUd);
                    break;
                case UCS2.DataCodingSchemeCode:
                    decodedMessage = UCS2.Decode(tpUd);
                    break;
                default:
                    break;
            }

            return new PduMessage(serviceCenterNumber, senderNumber, decodedMessage, timeStamp);
        }
    }

    public class PduMessage
    {
        public PduMessage(string serviceCenterNumber, string senderNumber, string message, DateTimeOffset timestamp)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
        }

        public string ServiceCenterNumber { get; }
        public string SenderNumber { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
    }
}
