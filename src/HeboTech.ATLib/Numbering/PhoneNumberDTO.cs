namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// Phone number received from the modem
    /// </summary>
    public class PhoneNumberDto
    {
        public PhoneNumberDto(string number, TypeOfNumber ton, NumberingPlanIdentification npi)
        {
            Number = number;
            Ton = ton;
            Npi = npi;
        }

        /// <summary>
        /// Phone number received from the modem
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// Type of number
        /// </summary>
        public TypeOfNumber Ton { get; }

        /// <summary>
        /// Numbering plan identification
        /// </summary>
        public NumberingPlanIdentification Npi { get; }

        public override string ToString()
        {
            return Number;
        }
    }
}
