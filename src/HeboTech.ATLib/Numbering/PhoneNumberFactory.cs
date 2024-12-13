using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Numbering
{
    public static class PhoneNumberFactory
    {
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
                return new InternationalPhoneNumber(countryCode.CodeAsString, nationalNumber, NumberingPlanIdentification.ISDN);
            }
            return new NationalPhoneNumber(sanitizedNumber, NumberingPlanIdentification.ISDN);
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
                    return new InternationalPhoneNumber(countryCode.CodeAsString, nationalNumber, npi);
                case TypeOfNumber.National:
                    if (!numericNumberMatch.Success)
                        throw new ArgumentException("Invalid number. Expected national type.");
                    return new NationalPhoneNumber(sanitizedNumber, npi);
                case TypeOfNumber.NetworkSpecific:
                    return new NetworkSpecificPhoneNumber(sanitizedNumber, npi);
                    break;
                case TypeOfNumber.Subscriber:
                    break;
                case TypeOfNumber.AlphaNumeric:
                    return new AlphaNumericPhoneNumber(number, npi);
                case TypeOfNumber.Abbreviated:
                    break;
                case TypeOfNumber.ReservedForExtension:
                    break;
                default:
                    throw new NotSupportedException("The number type is not supported");
            }
            throw new NotSupportedException("Number type not supported");
        }
    }
}
