using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using System;
using System.Linq;
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

        public static PduMessage Decode(string data, int timestampYearOffset = 2000)
        {
            int offset = 0;
            int smscLength = Convert.ToInt32(data[offset..(offset += 2)], 16);
            PhoneNumber serviceCenterNumber = null;
            if (smscLength > 0)
            {
                int smscAddressType = Convert.ToInt32(data[offset..(offset += 2)], 16);
                string serviceCenterNumberString = data[offset..(offset += (smscLength - 1) * 2)];
                serviceCenterNumberString = SwapPhoneNumberDigits(serviceCenterNumberString);
                switch (smscAddressType)
                {
                    case (int)PhoneNumberFormat.National:
                        break;
                    case (int)PhoneNumberFormat.International:
                        serviceCenterNumberString = '+' + serviceCenterNumberString;
                        break;
                    default:
                        break;
                }
                serviceCenterNumber = new PhoneNumber(serviceCenterNumberString);
            }

            int tp_mti = Convert.ToInt32(data[offset..(offset + 2)], 16);
            int tpdu_type = tp_mti & 0b0000_0011;
            switch (tpdu_type)
            {
                case (byte)PduType.SMS_DELIVER:
                    return DecodeSmsDeliver(serviceCenterNumber, data[offset..], timestampYearOffset);
                case (byte)PduType.SMS_SUBMIT:
                    return DecodeSmsSubmit(serviceCenterNumber, data[offset..], timestampYearOffset);
                default:
                    break;
            }

            throw new ArgumentException("Invalid data or not supported");
        }

        private static PduMessage DecodeSmsDeliver(PhoneNumber serviceCenterNumber, string text, int timestampYearOffset = 2000)
        {
            char[] data = text.ToCharArray();

            byte temp = Convert.ToByte(text[0..2], 16);

            int tp_mti = temp & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_DELIVER)
                throw new ArgumentException("Invalid SMS-DELIVER data");

            int tp_mms = temp & 0b0000_0100;
            int tp_rp = temp & 0b1000_0000;

            temp = Convert.ToByte(text[2..4], 16);
            int tp_oa_length = temp % 2 == 0 ? temp : temp + 1;
            byte tp_oa_ton = Convert.ToByte(text[4..6], 16);

            int offset = 6 + tp_oa_length;
            char[] oa = data[6..offset];
            byte tp_pid = Convert.ToByte(text[offset..(offset += 2)], 16);
            byte tp_dcs = Convert.ToByte(text[offset..(offset += 2)], 16);
            char[] tp_scts = text[offset..(offset += 14)].ToCharArray();
            byte tp_udl = Convert.ToByte(text[offset..(offset += 2)], 16);
            
            char[] tp_ud = text[offset..(offset += ((tp_udl - 1) * 2))].ToCharArray();
            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    message = Gsm7.Decode(new string(tp_ud));
                    break;
                default:
                    break;
            }
            string oa2 = SwapPhoneNumberDigits(oa);
            DateTimeOffset scts = DecodeTimestamp(tp_scts, timestampYearOffset);
            return new PduMessage(serviceCenterNumber, oa2, message, scts);
        }

        private static PduMessage DecodeSmsSubmit(PhoneNumber serviceCenterNumber, string text, int timestampYearOffset = 2000)
        {
            char[] data = text.ToCharArray();

            byte temp = Convert.ToByte(text[0..2], 16);

            int tp_mti = temp & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_SUBMIT)
                throw new ArgumentException("Invalid SMS-SUBMIT data");

            int tp_rd = temp & 0b0000_0100;
            int tp_vpf = temp & 0b0001_1000;
            int tp_rp = temp & 0b1000_0000;

            byte tp_mr = Convert.ToByte(text[2..4], 16);
            byte tp_oa_length = Convert.ToByte(text[4..6], 16);
            byte tp_toa = Convert.ToByte(text[6..8], 16);

            temp = Convert.ToByte(text[6..8], 16);
            int tp_oa_byteLength = tp_oa_length % 2 == 0 ? tp_oa_length : tp_oa_length + 1;

            int offset = 8;
            char[] oa = data[offset..(offset += tp_oa_length + 1)];
            byte tp_pid = Convert.ToByte(text[offset..(offset += 2)], 16);
            byte tp_dcs = Convert.ToByte(text[offset..(offset += 2)], 16);
            byte tp_vp = 0;
            if (tp_vpf == 0x00)
                tp_vp = Convert.ToByte(text[offset..(offset += 0)], 16);
            else if (tp_vpf == 0x01)
                tp_vp = Convert.ToByte(text[offset..(offset += 14)], 16);
            else if (tp_vpf == 0x10)
                tp_vp = Convert.ToByte(text[offset..(offset += 2)], 16);
            else if (tp_vpf == 0x11)
                tp_vp = Convert.ToByte(text[offset..(offset += 14)], 16);
            byte tp_udl = Convert.ToByte(text[offset..(offset += 2)], 16);
            
            char[] tp_ud = text[offset..(offset += ((tp_udl - 1) * 2))].ToCharArray();
            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    message = Gsm7.Decode(new string(tp_ud));
                    break;
                default:
                    break;
            }
            string oa2 = SwapPhoneNumberDigits(oa);
            return new PduMessage(serviceCenterNumber, oa2, message, DateTimeOffset.MinValue);
        }

        private static string SwapPhoneNumberDigits(string value)
        {
            return SwapPhoneNumberDigits(value.ToCharArray());
        }

        private static string SwapPhoneNumberDigits(char[] data)
        {
            for (int i = 0; i < data.Length; i += 2)
            {
                char temp = data[i];
                data[i] = data[i + 1];
                data[i + 1] = temp;
            }
            if (data[^1] == 'F')
                return new string(data[..^1]);
            return new string(data);
        }

        private static DateTimeOffset DecodeTimestamp(char[] data, int timestampYearOffset = 2000)
        {
            byte offset = (byte)int.Parse(data[12..14].Reverse().ToArray());
            bool positive = (offset & (1 << 7)) == 0;
            byte offsetQuarters = (byte)(offset & 0b0111_1111);

            DateTimeOffset timestamp = new DateTimeOffset(
                int.Parse(data[..2].Reverse().ToArray()) + timestampYearOffset,
                int.Parse(data[2..4].Reverse().ToArray()),
                int.Parse(data[4..6].Reverse().ToArray()),
                int.Parse(data[6..8].Reverse().ToArray()),
                int.Parse(data[8..10].Reverse().ToArray()),
                int.Parse(data[10..12].Reverse().ToArray()),
                TimeSpan.FromMinutes(offsetQuarters * 15)); // Offset in quarter of hours
            return timestamp;
        }
    }
        
    public class PduMessage
    {
        public PduMessage(PhoneNumber serviceCenterNumber, string senderNumber, string message, DateTimeOffset timestamp)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
            Timestamp = timestamp;
        }

        public PhoneNumber ServiceCenterNumber { get; }
        public string SenderNumber { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
    }
}
