using System;

namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// PhoneNumber with TypeOfNumber (TON) 0x01
    /// </summary>
    public class InternationalPhoneNumber : PhoneNumber
    {
        internal InternationalPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.International, npi)
        {
            ThrowIfEmpty(number);
            string sanitizedNumber = GetSanitizedNumber(number);
            ThrowIfNotValid(sanitizedNumber);
            
            Number = sanitizedNumber[1..];
        }

        protected static void ThrowIfNotValid(string number)
        {
            if (!number.StartsWith('+'))
                throw new ArgumentException("Invalid prefix ('+')", nameof(number));
            if (number[1..].Length > 15)
                throw new ArgumentException("Phone number length cannot exceed 15 characters", nameof(number));
        }

        public override string Number { get; }

        public override string ToString() =>
            $"+{base.ToString()}";

        //protected CountryCode GetCountryCode(string number)
        //{
        //    string sanitizedNumber = GetSanitizedNumber(number);
        //    var match = Regex.Match(sanitizedNumber, @"^(?<prefix>\+?)(?<digits>\d+)$", RegexOptions.Compiled);
        //    if (!match.Success)
        //        throw new ArgumentException("Invalid phone number");

        //    if (match.Groups["prefix"].Value == "+")
        //    {
        //        string numberOnly = match.Groups["digits"].Value;
        //        var orderedCodes = CountryCodes.Items.OrderByDescending(x => x.Code);
        //        var countryCode = orderedCodes.FirstOrDefault(x => numberOnly.StartsWith(x.CodeAsString));
        //        return countryCode;
        //    }

        //    throw new ArgumentException("Invalid phone number");
        //}
    }
}
