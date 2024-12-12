namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// Phone number received from the modem
    /// </summary>
    public class PhoneNumberDto
    {
        public PhoneNumberDto(string number)
        {
            Number = number;
        }

        /// <summary>
        /// The phone number received from the modem
        /// </summary>
        public string Number { get; }

        public override string ToString()
        {
            return Number;
        }
    }
}
