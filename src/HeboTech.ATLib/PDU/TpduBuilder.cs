using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    internal class TpduBuilderBase
    {
        // First octet of the message
        protected byte header = 0x00;

        protected bool UserDataHeaderIndicatorIsSet => (header & (1 << 6)) != 0x00;

        protected TpduBuilderBase(PduType messageType)
        {
            header = (byte)messageType;
        }

        protected TpduBuilderBase UserDataHeaderIndicator(bool value)
        {
            header |= (byte)(Convert.ToByte(value) << 6);
            return this;
        }

        protected TpduBuilderBase EnableReplyPath()
        {
            header |= 0b1000_0000;
            return this;
        }

        protected static byte GetAddressType(PhoneNumber phoneNumber)
        {
            return (byte)(0b1000_0000 + (byte)phoneNumber.GetTypeOfNumber() + (byte)phoneNumber.GetNumberPlanIdentification());
        }

        protected static string SwapPhoneNumberDigits(ReadOnlySpan<char> data)
        {
            char[] swappedData = new char[data.Length];
            for (int i = 0; i < data.Length; i += 2)
            {
                swappedData[i] = data[i + 1];
                swappedData[i + 1] = data[i];
            }
            if (swappedData[^1] == 'F')
                return new string(swappedData[..^1]);
            return new string(swappedData);
        }
    }

    internal class SmsSubmitBuilder : TpduBuilderBase
    {
        // TP-Message-Reference. '00' lets the phone set the message reference number itself
        private byte mr;
        // Address length.Length of phone number (number of digits)
        private byte daLength;
        // Type-of-Address
        private byte daType;
        // Phone number in semi octets. 12345678 is represented as 21436587
        private string daNumber = string.Empty;
        // TP-PID Protocol identifier
        private byte pi;
        // TP-DCS Data Coding Scheme. '00'-7bit default alphabet. '04'-8bit
        private CodingScheme dcs;
        // TP-Validity-Period. 'AA'-4 days
        private List<byte> vp = new List<byte>();
        // User Data Length
        private byte udl;
        // IEIs
        private List<string> triplets = new List<string>();
        // User Data
        private string ud = string.Empty;

        protected SmsSubmitBuilder()
            : base(PduType.SMS_SUBMIT)
        {
        }

        public static SmsSubmitBuilder Initialize()
        {
            return new SmsSubmitBuilder();
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <returns></returns>
        public SmsSubmitBuilder RejectDuplicates()
        {
            header |= 0b0000_0100;
            return this;
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SmsSubmitBuilder ValidityPeriodFormat(byte value)
        {
            byte mask = 0b0001_1000;
            header = (byte)((header & ~mask) | (value & mask));
            return this;
        }

        public SmsSubmitBuilder EnableStatusReportRequest()
        {
            header |= 0b0010_0000;
            return this;
        }

        public new SmsSubmitBuilder UserDataHeaderIndicator(bool value)
        {
            return (SmsSubmitBuilder)base.UserDataHeaderIndicator(value);
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <returns></returns>
        public new SmsSubmitBuilder EnableReplyPath()
        {
            return (SmsSubmitBuilder)base.EnableReplyPath();
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SmsSubmitBuilder MessageReference(byte value)
        {
            mr = value;
            return this;
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public SmsSubmitBuilder DestinationAddress(PhoneNumber phoneNumber)
        {
            if (phoneNumber == null)
                throw new ArgumentNullException(nameof(phoneNumber));
            daLength = (byte)phoneNumber.ToString().TrimStart('+').Length;
            daType = GetAddressType(phoneNumber);
            daNumber = SwapPhoneNumberDigits(phoneNumber.ToString().TrimStart('+'));
            return this;
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SmsSubmitBuilder ProtocolIdentifier(byte value)
        {
            pi = value;
            return this;
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SmsSubmitBuilder DataCodingScheme(CodingScheme value)
        {
            dcs = value;
            return this;
        }

        public SmsSubmitBuilder ValidityPeriod(byte value)
        {
            return ValidityPeriod(new byte[] { value });
        }

        public SmsSubmitBuilder ValidityPeriod(IEnumerable<byte> value)
        {
            if (!(value.Count() != 0 || value.Count() != 1 || value.Count() != 7))
                throw new ArgumentOutOfRangeException($"{nameof(value)} must either be 0, 1 or 7 bytes");
            vp.Clear();
            vp.AddRange(value);
            return this;
        }

        public SmsSubmitBuilder AddTriplet(int tag, params int[] values)
        {
            string triplet = $"{tag.ToString("X2")}{values.Length.ToString("X2")}{string.Join("", values.Select(x => x.ToString("X2")))}";
            triplets.Add(triplet);
            udhLength += 2 + values.Length; // IEI + IE Length + Values
            return this;
        }

        public SmsSubmitBuilder UserData(string value)
        {
            if (value.Length > 255)
                throw new ArgumentOutOfRangeException($"{nameof(value)} must be less than or equal to 255 bytes");
            ud = value;
            return this;
        }

        public string Build()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(header.ToString("X2"));
            sb.Append(mr.ToString("X2"));
            sb.Append(daLength.ToString("X2"));
            sb.Append(daType.ToString("X2"));
            sb.Append(daNumber);
            sb.Append(pi.ToString("X2"));
            sb.Append(((byte)dcs).ToString("X2"));
            if (vp.Count > 0)
                sb.Append(String.Join("", vp.Select(x => x.ToString("X2"))));

            if (UserDataHeaderIndicatorIsSet)
            {

            }
            else
            {
                switch (dcs)
                {
                    case CodingScheme.Ansi:
                        break;
                    case CodingScheme.Gsm7:
                        int messageBitLength = ud.Length / 2 * 7;
                        int messageLength = messageBitLength % 8 == 0 ? messageBitLength / 8 : (messageBitLength / 8) + 1;
                        sb.Append((messageLength).ToString("X2"));
                        break;
                    case CodingScheme.UCS2:
                        sb.Append((ud.Length / 2 * 8 / 7).ToString("X2"));
                        break;
                    default:
                        break;
                }

                sb.Append(ud);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return Build();
        }
    }
}
