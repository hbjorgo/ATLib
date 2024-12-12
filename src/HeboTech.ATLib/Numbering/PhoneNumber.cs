using System;
using System.Linq;

namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// Phone Number
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="nationalNumber">National Number (National Destination Code and Subscriber Number)</param>
        public PhoneNumber(string nationalNumber)
            : this(string.Empty, nationalNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="countryCode">Country Code</param>
        /// <param name="nationalNumber">National Number (National Destination Code and Subscriber Number)</param>
        /// <exception cref="ArgumentException"></exception>
        public PhoneNumber(string countryCode, string nationalNumber)
        {
            if (!countryCode.All(char.IsDigit))
                throw new ArgumentException("Must be numeric only", nameof(countryCode));
            if (!nationalNumber.All(char.IsDigit))
                throw new ArgumentException("Must be numeric only", nameof(nationalNumber));
            if (countryCode.Length + nationalNumber.Length > 15)
                throw new ArgumentException("Total phone number length cannot exceed 15 characters");

            CountryCode = countryCode;
            NationalNumber = nationalNumber;
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
        /// Get Type Of Number (TON)
        /// </summary>
        /// <returns>Type Of Number (TON)</returns>
        public TypeOfNumber GetTypeOfNumber()
        {
            if (CountryCode != string.Empty)
                return TypeOfNumber.International;
            else
                return TypeOfNumber.National;
        }

        /// <summary>
        /// Get Number Plan Identification (NPI)
        /// </summary>
        /// <returns>Number Plan Identification (NPI)</returns>
        public NumberPlanIdentification GetNumberPlanIdentification()
        {
            if (CountryCode != string.Empty)
                return NumberPlanIdentification.ISDN;
            else
                return NumberPlanIdentification.Unknown;
        }

        public override string ToString()
        {
            if (CountryCode != string.Empty)
                return $"+{CountryCode}{NationalNumber}";
            return NationalNumber.ToString();
        }
    }
}
