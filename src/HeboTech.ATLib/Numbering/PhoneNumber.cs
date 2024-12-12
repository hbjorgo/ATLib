using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// Phone Number
    /// </summary>
    public class PhoneNumber
    {
        public PhoneNumber(string countryCode, string nationalNumber, TypeOfNumber ton, NumberingPlanIdentification npi)
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
            NumberingPlanIdentification = npi;
        }

        public static PhoneNumber Create(string number)
        {
            string sanitizedNumber = Regex.Replace(number, @"[\s-()./]", "", RegexOptions.Compiled);
            var match = Regex.Match(sanitizedNumber, @"^(?<prefix>\+?)(?<digits>\d+)$", RegexOptions.Compiled);
            if (!match.Success)
                throw new ArgumentException("Invalid phone number");

            if (match.Groups["prefix"].Length > 0)
            {
                string numberOnly = match.Groups["digits"].Value;
                var orderedCodes = CountryCodes.Items.OrderByDescending(x => x.Code);
                var countryCode = orderedCodes.FirstOrDefault(x => numberOnly.StartsWith(x.CodeAsString));
                string nationalNumber = numberOnly[countryCode.CodeAsString.Length..];
                return CreateInternationalNumber(countryCode.CodeAsString, nationalNumber);
            }
            return CreateNationalNumber(sanitizedNumber);
        }

        /// <summary>
        /// Creates a national PhoneNumber. Digits only.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static PhoneNumber CreateNationalNumber(string number) =>
            new PhoneNumber(string.Empty, number, TypeOfNumber.National, NumberingPlanIdentification.ISDN);

        /// <summary>
        /// Creates an international PhoneNumber. Digits only. Exclude leading '+';
        /// </summary>
        /// <param name="countryCode">Country code</param>
        /// <param name="number">Number</param>
        /// <returns></returns>
        public static PhoneNumber CreateInternationalNumber(string countryCode, string number) =>
            new PhoneNumber(countryCode, number, TypeOfNumber.International, NumberingPlanIdentification.ISDN);

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
            new PhoneNumber(null, number, TypeOfNumber.AlphaNumeric, NumberingPlanIdentification.Unknown);

        /// <summary>
        /// Country code
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// National number
        /// </summary>
        public string NationalNumber { get; }

        /// <summary>
        /// Type of number
        /// </summary>
        public TypeOfNumber TypeOfNumber { get; }

        /// <summary>
        /// Numbering plan identification
        /// </summary>
        public NumberingPlanIdentification NumberingPlanIdentification { get; }

        public override string ToString()
        {
            if (CountryCode != string.Empty)
                return $"+{CountryCode}{NationalNumber}";
            return NationalNumber.ToString();
        }
    }
}
