using HeboTech.ATLib.CodingSchemes;

namespace HeboTech.ATLib.DTOs
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
            : this(
                  phoneNumber,
                  message,
                  codingScheme,
                  ValidityPeriod.NotPresent())
        {
        }

        /// <summary>
        /// Creates a data object for submitting an SMS in PDU format.
        /// </summary>
        /// <param name="phoneNumber">The receiver phone number</param>
        /// <param name="message">The message to send</param>
        /// <param name="codingScheme">The coding scheme to use</param>
        /// <param name="validityPeriod">The validity period to use</param>
        public SmsSubmitRequest(
            PhoneNumber phoneNumber,
            string message,
            CharacterSet codingScheme,
            ValidityPeriod validityPeriod)
        {
            PhoneNumber = phoneNumber;
            Message = message;
            CodingScheme = codingScheme;
            ValidityPeriod = validityPeriod;
        }

        public PhoneNumber PhoneNumber { get; }
        public string Message { get; }
        public CharacterSet CodingScheme { get; }
        public byte MessageReferenceNumber { get; set; }
        public ValidityPeriod ValidityPeriod { get; set; }
        public bool EnableStatusReportRequest { get; set; }
    }
}
