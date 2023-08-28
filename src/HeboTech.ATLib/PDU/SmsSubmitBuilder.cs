using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.PDU
{
    internal class SmsSubmitBuilder
    {
        protected const int MAX_SINGLE_MESSAGE_SIZE = 134;
        protected const int MAX_MESSAGE_PART_SIZE = 134;
        protected const int MAX_NUMBER_OF_MESSAGE_PARTS = 255;

        // First octet of the message
        protected byte header = 0x00;
        // TP-Message-Reference. '00' lets the phone set the message reference number itself
        protected byte mr;
        // Address length.Length of phone number (number of digits)
        protected byte daLength;
        // Type-of-Address
        protected byte daType;
        // Phone number in semi octets. 12345678 is represented as 21436587
        protected string daNumber = string.Empty;
        // TP-PID Protocol identifier
        protected byte pi;
        // TP-DCS Data Coding Scheme. '00'-7bit default alphabet. '04'-8bit
        protected CodingScheme dcs;
        // TP-Validity-Period. 'AA'-4 days
        protected List<byte> vp = new List<byte>();

        protected SmsSubmitBuilder()
        {
            header = (byte)PduType.SMS_SUBMIT;
        }

        public static SmsSubmitBuilder Initialize()
        {
            return new SmsSubmitBuilder();
        }

        protected bool UserDataHeaderIndicatorIsSet => (header & (1 << 6)) != 0x00;

        public SmsSubmitBuilder EnableUserDataHeaderIndicator()
        {
            header |= 0b0100_0000;
            return this;
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <returns></returns>
        public SmsSubmitBuilder EnableReplyPath()
        {
            header |= 0b1000_0000;
            return this;
        }

        private static byte GetAddressType(PhoneNumber phoneNumber)
        {
            return (byte)(0b1000_0000 + (byte)phoneNumber.GetTypeOfNumber() + (byte)phoneNumber.GetNumberPlanIdentification());
        }

        private static string SwapPhoneNumberDigits(ReadOnlySpan<char> data)
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

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected SmsSubmitBuilder MessageReference(byte value)
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

        public string Build(byte[] data)
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

            switch (dcs)
            {
                case CodingScheme.Ansi:
                    //var encoded = Ansi.EncodeToBytes(data);
                    //sb.Append((data.Length * 8).ToString("X2"));
                    //sb.Append(string.Join("", encoded.Select(x => x.ToHexString())));
                    break;
                case CodingScheme.Gsm7:

                    //var udhEncoded = Gsm7.EncodeToBytes(data[..6]);
                    //var udEncoded = Gsm7.EncodeToBytes(data[6..]);
                    //int udlBits = (udhEncoded.Length + udEncoded.Length) * 8;
                    //int udlSeptets = udlBits / 7;
                    //sb.Append((udlSeptets).ToString("X2"));
                    //sb.Append(string.Join("", udhEncoded.Select(x => x.ToHexString())));
                    //sb.Append(string.Join("", udEncoded.Select(x => x.ToHexString())));



                    var encoded = Gsm7.EncodeToBytes(data, UserDataHeaderIndicatorIsSet ? 1 : 0);
                    int udlBits = (encoded.Length) * 8;
                    int udlSeptets = udlBits / 7;
                    sb.Append((udlSeptets).ToString("X2"));
                    sb.Append(string.Join("", encoded.Select(x => x.ToHexString())));
                    break;
                case CodingScheme.UCS2:
                    //var encoded = UCS2.EncodeToBytes(data);
                    //sb.Append((data.Length * 8).ToString("X2"));
                    //sb.Append(string.Join("", encoded.Select(x => x.ToHexString())));
                    break;
                default:
                    break;
            }

            return sb.ToString();
        }

        public static byte[][] CreateMessageParts(IEnumerable<byte> data, byte messageReferenceNumber)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // The message does not need to be concatenated. Return empty array
            if (data.Count() <= MAX_SINGLE_MESSAGE_SIZE)
                return new byte[0][];

            int numberOfParts = (data.Count() / MAX_MESSAGE_PART_SIZE) + (data.Count() % MAX_MESSAGE_PART_SIZE == 0 ? 0 : 1);

            byte[][] parts = new byte[numberOfParts][];
            for (int i = 0; i < numberOfParts; i++)
            {
                // UDH (6 bytes) + length of part
                byte[] part = new byte[6 + MAX_MESSAGE_PART_SIZE];

                // Length of UDH
                part[0] = 0x05;
                // IEI (0x00) for concatenated SMS
                part[1] = 0x00;
                // Length of header for concatenated SMS (excluding the two first octets)
                part[2] = 0x03;
                // CSMS reference number.
                part[3] = messageReferenceNumber;
                // Total number of parts
                part[4] = (byte)numberOfParts;
                // This part's sequence number, starting at 1
                part[5] = (byte)(i + 1);

                // Copy message part onto the end
                byte[] temp = data.Skip(i * MAX_MESSAGE_PART_SIZE).Take(MAX_MESSAGE_PART_SIZE).ToArray();
                Array.Copy(temp, 0, part, 6, temp.Length);

                parts[i] = part;
            }

            return parts;
        }
    }

    internal class MessagePart
    {
        public MessagePart(byte[] header, byte[] data)
        {
            Header = header;
            Data = data;
        }

        public byte[] Header { get; }
        public byte[] Data { get; }
    }

    //internal class SingleSmsSubmitBuilder : SmsSubmitBuilder
    //{
    //    public SingleSmsSubmitBuilder(PhoneNumber phoneNumber, byte[] message, CodingScheme codingScheme, byte validityPeriodFormat, byte validityPeriod)
    //    {
    //        DestinationAddress(phoneNumber);
    //        DataCodingScheme(codingScheme);
    //        ValidityPeriodFormat(validityPeriodFormat);
    //        ValidityPeriod(validityPeriod);
    //        UserData(message);
    //    }

    //    public void UserData(IEnumerable<byte> message)
    //    {
    //        if (message.Count() > MAX_MESSAGE_PART_SIZE * MAX_NUMBER_OF_MESSAGE_PARTS)
    //            throw new ArgumentException($"Too long", nameof(message));

    //        if (message.Count() <= MAX_SINGLE_MESSAGE_SIZE)
    //        {
    //            ud.Clear();
    //            ud.AddRange(message);
    //        }
    //    }

    //    public string Build()
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        sb.Append(header.ToString("X2"));
    //        sb.Append(mr.ToString("X2"));
    //        sb.Append(daLength.ToString("X2"));
    //        sb.Append(daType.ToString("X2"));
    //        sb.Append(daNumber);
    //        sb.Append(pi.ToString("X2"));
    //        sb.Append(((byte)dcs).ToString("X2"));
    //        if (vp.Count > 0)
    //            sb.Append(String.Join("", vp.Select(x => x.ToString("X2"))));

    //        switch (dcs)
    //        {
    //            case CodingScheme.Ansi:
    //                break;
    //            case CodingScheme.Gsm7:
    //                int udlBits = (ud.Count) * 8;
    //                int udlSeptets = udlBits / 7;
    //                sb.Append((udlSeptets).ToString("X2"));
    //                break;
    //            case CodingScheme.UCS2:
    //                sb.Append((ud.Count * 8).ToString("X2"));
    //                break;
    //            default:
    //                break;
    //        }
    //        sb.Append(string.Join("", ud.Select(x => x.ToHexString())));

    //        return sb.ToString();
    //    }
    //}

    //internal class ConcatenatedSmsSubmitBuilder : SmsSubmitBuilder
    //{
    //    public ConcatenatedSmsSubmitBuilder(PhoneNumber phoneNumber, byte[] message, byte messageReferenceNumber, CodingScheme codingScheme, byte validityPeriodFormat, byte validityPeriod)
    //    {
    //        DestinationAddress(phoneNumber);
    //        DataCodingScheme(codingScheme);
    //        ValidityPeriodFormat(validityPeriodFormat);
    //        ValidityPeriod(validityPeriod);
    //        UserData(message, messageReferenceNumber);
    //    }

    //    public void UserData(IEnumerable<byte> message, byte messageReferenceNumber)
    //    {
    //        if (message.Count() > MAX_MESSAGE_PART_SIZE * MAX_NUMBER_OF_MESSAGE_PARTS)
    //            throw new ArgumentException($"Too long", nameof(message));

    //        if (message.Count() <= MAX_SINGLE_MESSAGE_SIZE)
    //        {
    //            ud.Clear();
    //            ud.AddRange(message);
    //        }
    //        else
    //        {
    //            var parts = CreateMessageParts(message, messageReferenceNumber);
    //        }
    //    }

    //    public IEnumerable<string> Build()
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        sb.Append(header.ToString("X2"));
    //        sb.Append(mr.ToString("X2"));
    //        sb.Append(daLength.ToString("X2"));
    //        sb.Append(daType.ToString("X2"));
    //        sb.Append(daNumber);
    //        sb.Append(pi.ToString("X2"));
    //        sb.Append(((byte)dcs).ToString("X2"));
    //        if (vp.Count > 0)
    //            sb.Append(String.Join("", vp.Select(x => x.ToString("X2"))));

    //        int fillBits = 0;

    //        switch (dcs)
    //        {
    //            case CodingScheme.Ansi:
    //                break;
    //            case CodingScheme.Gsm7:
    //                int udhlBits = (udhLength + 1) * 8;
    //                if (udhlBits % 7 > 0)
    //                    fillBits = 7 - (udhlBits % 7);
    //                int udlBits = ((udhLength + 1) * 8) + fillBits + ((ud.Count) * 8);
    //                int udlSeptets = udlBits / 7;
    //                sb.Append((udlSeptets).ToString("X2"));
    //                break;
    //            case CodingScheme.UCS2:
    //                sb.Append((ud.Count * 8).ToString("X2"));
    //                break;
    //            default:
    //                break;
    //        }

    //        sb.Append(udhLength.ToHexString());
    //        sb.AppendJoin("", triplets);

    //        sb.Append(string.Join("", ud.Select(x => x.ToHexString())));

    //        return sb.ToString();
    //    }
    //}
}
