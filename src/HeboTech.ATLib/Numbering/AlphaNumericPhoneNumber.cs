namespace HeboTech.ATLib.Numbering
{
    public class AlphaNumericPhoneNumber : PhoneNumber
    {
        public AlphaNumericPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.AlphaNumeric, npi)
        {
            ThrowIfNotValid(number);

            NumberWithPrefix = number;
            NumberWithoutPrefix = number;
        }

        /// <summary>
        /// Full number
        /// </summary>
        public override string NumberWithPrefix { get; }
        public override string NumberWithoutPrefix { get; }
    }
}
