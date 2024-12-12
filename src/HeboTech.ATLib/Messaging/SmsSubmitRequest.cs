using HeboTech.ATLib.Numbering;

namespace HeboTech.ATLib.Messaging
{
    /// <summary>
    /// Data object for submitting an SMS in PDU format.
    /// </summary>
    public class SmsSubmitRequest
    {
        /// <summary>
        /// Creates a data object for submitting an SMS in PDU format.
        /// Chooses GSM 7 bit encoding if the message content is compatible, otherwise, UCS2 is used.
        /// No ValidityPeriod is set.
        /// </summary>
        /// <param name="phoneNumber">The receiver phone number</param>
        /// <param name="message">The message to send</param>
        public SmsSubmitRequest(
            PhoneNumber phoneNumber,
            string message)
            : this(
                  phoneNumber,
                  message,
                  Gsm7.IsGsm7Compatible(message.ToCharArray()) ? CharacterSet.Gsm7 : CharacterSet.UCS2)
        {
        }

        /// <summary>
        /// Creates a data object for submitting an SMS in PDU format.
        /// </summary>
        /// <param name="phoneNumber">The receiver phone number</param>
        /// <param name="message">The message to send</param>
        /// <param name="codingScheme">The coding scheme to use</param>
        public SmsSubmitRequest(
            PhoneNumber phoneNumber,
            string message,
            CharacterSet codingScheme)
        {
            PhoneNumber = phoneNumber;
            Message = message;
            CodingScheme = codingScheme;
        }

        public PhoneNumber PhoneNumber { get; }
        public string Message { get; }
        public CharacterSet CodingScheme { get; }
        public byte MessageReferenceNumber { get; set; }
        public ValidityPeriod ValidityPeriod { get; set; }
        public bool EnableStatusReportRequest { get; set; }
    }
}
