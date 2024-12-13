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
        protected PhoneNumber(string countryCode, string nationalNumber, TypeOfNumber ton, NumberingPlanIdentification npi)
        {
            if (nationalNumber == null)
                throw new ArgumentNullException(nameof(nationalNumber), "Number cannot be empty");
            if (nationalNumber.Length < 1)
                throw new ArgumentNullException(nameof(nationalNumber), "Number cannot be empty");
            //if (!nationalNumber.All(char.IsDigit))
            //    throw new ArgumentException("Must be numeric", nameof(nationalNumber));
            if (countryCode != null && countryCode.Length > 0 && !countryCode.All(char.IsDigit))
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
                return new PhoneNumber(countryCode.CodeAsString, nationalNumber, TypeOfNumber.International, NumberingPlanIdentification.ISDN);
            }
            return new PhoneNumber(string.Empty, sanitizedNumber, TypeOfNumber.National, NumberingPlanIdentification.ISDN);
        }

        public static PhoneNumber Create(string number, TypeOfNumber ton, NumberingPlanIdentification npi)
        {
            string sanitizedNumber = Regex.Replace(number, @"[\s-()./]", "", RegexOptions.Compiled);
            var numericNumberMatch = Regex.Match(sanitizedNumber, @"^(?<prefix>\+?)(?<digits>\d+)$", RegexOptions.Compiled);

            switch (ton)
            {
                case TypeOfNumber.Unknown:
                    break;
                case TypeOfNumber.International:
                    if (!numericNumberMatch.Success)
                        throw new ArgumentException("Invalid number. Expected international type.");
                    string numberOnly = numericNumberMatch.Groups["digits"].Value;
                    var orderedCodes = CountryCodes.Items.OrderByDescending(x => x.Code);
                    var countryCode = orderedCodes.FirstOrDefault(x => numberOnly.StartsWith(x.CodeAsString));
                    string nationalNumber = numberOnly[countryCode.CodeAsString.Length..];
                    return new PhoneNumber(countryCode.CodeAsString, nationalNumber, ton, npi);
                case TypeOfNumber.National:
                    if (!numericNumberMatch.Success)
                        throw new ArgumentException("Invalid number. Expected national type.");
                    return new PhoneNumber(string.Empty, sanitizedNumber, ton, npi);
                case TypeOfNumber.NetworkSpecific:
                    break;
                case TypeOfNumber.Subscriber:
                    break;
                case TypeOfNumber.AlphaNumeric:
                    return new PhoneNumber(null, number, ton, npi);
                case TypeOfNumber.Abbreviated:
                    break;
                case TypeOfNumber.ReservedForExtension:
                    break;
                default:
                    throw new NotSupportedException("The number type is not supported");
            }
            return new PhoneNumber(null, number, ton, npi);
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
