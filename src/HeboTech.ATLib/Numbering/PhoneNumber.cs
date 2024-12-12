using System;
using System.Linq;

namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// Phone Number
    /// </summary>
    public class PhoneNumber
    {
        public PhoneNumber(string countryCode, string nationalNumber, TypeOfNumber ton, NumberPlanIdentification npi)
        {
            if (nationalNumber == null)
                throw new ArgumentNullException(nameof(nationalNumber), "Number cannot be empty");
            if (nationalNumber.Length < 1)
                throw new ArgumentNullException(nameof(nationalNumber), "Number cannot be empty");
            //if (!nationalNumber.All(char.IsDigit))
            //    throw new ArgumentException("Must be numeric", nameof(nationalNumber));
            if (countryCode != null && !countryCode.All(char.IsDigit))
                throw new ArgumentException("Must be numeric", nameof(countryCode));
            if ((countryCode?.Length ?? 0) + nationalNumber.Length > 15)
                throw new ArgumentException("Total phone number length cannot exceed 15 characters");

            CountryCode = countryCode ?? string.Empty;
            NationalNumber = nationalNumber;
            TypeOfNumber = ton;
            NumberPlanIdentification = npi;
        }

        /// <summary>
        /// Creates a national PhoneNumber. Digits only.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static PhoneNumber CreateNationalNumber(string number) =>
            new PhoneNumber(string.Empty, number, TypeOfNumber.National, NumberPlanIdentification.ISDN);

        /// <summary>
        /// Creates an international PhoneNumber. Digits only. Exclude leading '+';
        /// </summary>
        /// <param name="countryCode">Country code</param>
        /// <param name="number">Number</param>
        /// <returns></returns>
        public static PhoneNumber CreateInternationalNumber(string countryCode, string number) =>
            new PhoneNumber(countryCode, number, TypeOfNumber.International, NumberPlanIdentification.ISDN);

        /// <summary>
        /// Creates a national or international PhoneNumber.
        /// If country code is included, it will be international, else it will be national.
        /// Digits only.
        /// </summary>
        /// <param name="countryCode">Country code</param>
        /// <param name="number">Number</param>
        /// <returns></returns>
        public static PhoneNumber CreateNationalOrInternationalNumber(string countryCode, string number)
        {
            if (countryCode == null || countryCode == string.Empty)
                return CreateNationalNumber(number);
            return CreateInternationalNumber(countryCode, number);
        }

        public static PhoneNumber CreateAlphaNumericNumber(string number) =>
            new PhoneNumber(null, number, TypeOfNumber.AlphaNumeric, NumberPlanIdentification.Unknown);

        /// <summary>
        /// Country code
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// National number
        /// </summary>
        public string NationalNumber { get; }

        public TypeOfNumber TypeOfNumber { get; }
        public NumberPlanIdentification NumberPlanIdentification { get; }

        public override string ToString()
        {
            if (CountryCode != string.Empty)
                return $"+{CountryCode}{NationalNumber}";
            return NationalNumber.ToString();
        }
    }
}
