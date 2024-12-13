namespace HeboTech.ATLib.Numbering
{
    public class NetworkSpecificPhoneNumber : PhoneNumber
    {
        public NetworkSpecificPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.NetworkSpecific, npi)
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
