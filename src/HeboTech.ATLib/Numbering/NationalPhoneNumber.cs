namespace HeboTech.ATLib.Numbering
{
    public class NationalPhoneNumber : PhoneNumber
    {
        public NationalPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.National, npi)
        {
            ThrowIfNotValid(number);

            string sanitizedNationalNumber = GetSanitizedNumber(number);

            NumberWithPrefix = sanitizedNationalNumber;
            NumberWithoutPrefix = sanitizedNationalNumber;
        }

        /// <summary>
        /// Full number
        /// </summary>
        public override string NumberWithPrefix { get; }
        public override string NumberWithoutPrefix { get; }
    }
}
