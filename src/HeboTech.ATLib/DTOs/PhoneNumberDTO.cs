namespace HeboTech.ATLib.DTOs
{
    /// <summary>
    /// Phone number received from the modem
    /// </summary>
    public class PhoneNumberDTO
    {
        public PhoneNumberDTO(string number)
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
