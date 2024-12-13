namespace HeboTech.ATLib.Numbering
{
    public class InternationalPhoneNumber : PhoneNumber
    {
        public InternationalPhoneNumber(string countryCode, string nationalNumber, NumberingPlanIdentification npi)
            : base(TypeOfNumber.International, npi)
        {
            ThrowIfNotValid(countryCode + nationalNumber);

            string sanitizedCountryCode = GetSanitizedNumber(countryCode);
            string sanitizedNationalNumber = GetSanitizedNumber(nationalNumber);

            CountryCode = sanitizedCountryCode;
            NationalNumber = sanitizedNationalNumber;

            NumberWithPrefix = $"+{sanitizedCountryCode}{sanitizedNationalNumber}";
            NumberWithoutPrefix = $"{sanitizedCountryCode}{sanitizedNationalNumber}";
        }

        /// <summary>
        /// Country code
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// National number
        /// </summary>
        public string NationalNumber { get; }

        /// <summary>
        /// Full number
        /// </summary>
        public override string NumberWithPrefix { get; }

        public override string NumberWithoutPrefix { get; }
    }
}
