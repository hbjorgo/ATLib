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
        protected const int MAX_SINGLE_MESSAGE_SIZE_GSM7 = 160;
        protected const int MAX_SINGLE_MESSAGE_SIZE_UCS2 = 70;

        protected const int MAX_MESSAGE_PART_SIZE_GSM7 = 153;
        protected const int MAX_MESSAGE_PART_SIZE_UCS2 = 67;

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
        // Message
        protected string message = string.Empty;
        // Message reference number (for multi-part SMS)
        protected byte messageReferenceNumber;

        protected SmsSubmitBuilder()
        {
            header = (byte)MTI.SMS_SUBMIT;
        }

        public static SmsSubmitBuilder Initialize()
        {
            return new SmsSubmitBuilder();
        }

        protected bool UserDataHeaderIndicatorIsSet => (header & (1 << 6)) != 0x00;

        protected SmsSubmitBuilder EnableUserDataHeaderIndicator()
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
            return (byte)(0b1000_0000 + ((byte)phoneNumber.GetTypeOfNumber() << 4) + (byte)phoneNumber.GetNumberPlanIdentification());
        }

        private static string SwapPhoneNumberDigits(string data)
        {
            if (data.Length % 2 != 0)
                data += 'F';
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
        public SmsSubmitBuilder ValidityPeriod(ValidityPeriod validityPeriod)
        {
            // Set format
            byte mask = 0b0001_1000;
            header = (byte)((header & ~mask) | ((byte)validityPeriod.Format & mask));

            // Set value
            vp.Clear();
            vp.AddRange(validityPeriod.Value);

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
            daLength = (byte)(phoneNumber.CountryCode.Length + phoneNumber.NationalNumber.Length);
            daType = GetAddressType(phoneNumber);
            daNumber = SwapPhoneNumberDigits(phoneNumber.CountryCode + phoneNumber.NationalNumber); // TODO: Old: .TrimStart('+')
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
        public SmsSubmitBuilder DataCodingScheme(CodingScheme dataCodingScheme)
        {
            this.dcs = dataCodingScheme;
            return this;
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SmsSubmitBuilder Message(string message)
        {
            this.message = message;
            return this;
        }

        /// <summary>
        /// Mandatory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SmsSubmitBuilder MessageReferenceNumber(byte messageReferenceNumber)
        {
            this.messageReferenceNumber = messageReferenceNumber;
            return this;
        }

        public IEnumerable<string> Build()
        {
            var partitionedMessage = CreateMessageParts();

            if (partitionedMessage.Parts.Count() > 1)
                EnableUserDataHeaderIndicator();

            foreach (var part in partitionedMessage.Parts)
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
                    case CodingScheme.Gsm7:
                        int fillBits = 0;
                        if (UserDataHeaderIndicatorIsSet)
                            fillBits = 7 - ((part.Header.Length * 8) % 7);

                        var gsm7 = Gsm7.EncodeToBytes(part.Data);
                        var encoded = Gsm7.Pack(gsm7, fillBits);

                        int udlBits = part.Header.Length * 8 + gsm7.Length * 7 + fillBits;
                        int udlSeptets = udlBits / 7;

                        sb.Append((udlSeptets).ToString("X2"));
                        sb.Append(string.Join("", part.Header.Select(x => x.ToString("X2"))));
                        sb.Append(string.Join("", encoded.Select(x => x.ToString("X2"))));
                        break;
                    case CodingScheme.UCS2:
                        var ucs2Bytes = UCS2.EncodeToBytes(part.Data.ToArray());
                        sb.Append((part.Header.Length + ucs2Bytes.Length).ToString("X2"));
                        sb.Append(string.Join("", part.Header.Select(x => x.ToString("X2"))));
                        sb.Append(string.Join("", ucs2Bytes.Select(x => x.ToString("X2"))));
                        break;
                    default:
                        throw new ArgumentException($"Coding scheme {nameof(dcs)} is not supported");
                }

                yield return sb.ToString();
            }
        }

        protected Message CreateMessageParts()
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            int maxMessagePartSize;
            int maxSingleMessageSize;
            switch (dcs)
            {
                case CodingScheme.Gsm7:
                    maxSingleMessageSize = MAX_SINGLE_MESSAGE_SIZE_GSM7;
                    maxMessagePartSize = MAX_MESSAGE_PART_SIZE_GSM7;
                    break;
                case CodingScheme.UCS2:
                    maxSingleMessageSize = MAX_SINGLE_MESSAGE_SIZE_UCS2;
                    maxMessagePartSize = MAX_MESSAGE_PART_SIZE_UCS2;
                    break;
                default:
                    throw new ArgumentException($"Coding scheme {nameof(dcs)} is not supported");
            };

            // The message does not need to be concatenated. Return empty array
            if (message.Length <= maxSingleMessageSize)
                return new Message(0, 1, new MessagePart(Array.Empty<byte>(), message.ToCharArray()));

            int numberOfParts = (message.Length / maxMessagePartSize) + (message.Length % maxMessagePartSize == 0 ? 0 : 1);

            if (numberOfParts > MAX_NUMBER_OF_MESSAGE_PARTS)
                throw new ArgumentException("Message is too large!");

            MessagePart[] parts = new MessagePart[numberOfParts];
            for (int i = 0; i < numberOfParts; i++)
            {
                parts[i] = new MessagePart(
                            new byte[]
                            {
                                // Length of UDH
                                0x05,
                                // IEI (0x00) for concatenated SMS
                                0x00,
                                // Length of header for concatenated SMS (excluding the two first octets)
                                0x03,
                                // CSMS reference number
                                messageReferenceNumber,
                                // Total number of parts
                                (byte)numberOfParts,
                                // This part's sequence number, starting at 1
                                (byte)(i + 1)
                            },
                            // Each part of the total message
                            message.Skip(i * maxMessagePartSize).Take(maxMessagePartSize).ToArray());
            }
            
            return new Message(messageReferenceNumber, (byte)numberOfParts, parts);
        }
    }

    internal class Message
    {
        public Message(byte messageReferenceNumber, byte numberOfParts, params MessagePart[] parts)
        {
            MessageReferenceNumber = messageReferenceNumber;
            NumberOfParts = numberOfParts;
            Parts = parts;
        }

        public byte MessageReferenceNumber { get; }
        public byte NumberOfParts { get; }
        public IEnumerable<MessagePart> Parts { get; }

        public override string ToString()
        {
            return $"Msg. ref. no.: {MessageReferenceNumber}, #Parts: {NumberOfParts}";
        }
    }

    internal class MessagePart
    {
        public MessagePart(byte[] header, char[] data)
        {
            Header = header;
            Data = data;
        }

        public byte[] Header { get; }
        public char[] Data { get; }

        public override string ToString()
        {
            return $"({Data.Length} chars): {new string(Data)}";
        }
    }
}
