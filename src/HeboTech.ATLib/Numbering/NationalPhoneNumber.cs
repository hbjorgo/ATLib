using System;

namespace HeboTech.ATLib.Numbering
{
    public class NationalPhoneNumber : PhoneNumber
    {
        internal NationalPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.National, npi)
        {
            ThrowIfEmpty(number);
            string sanitizedNationalNumber = GetSanitizedNumber(number);
            ThrowIfNotValid(sanitizedNationalNumber);

            Number = sanitizedNationalNumber;
        }

        protected static void ThrowIfNotValid(string number)
        {
            if (number.Length > 15)
                throw new ArgumentException("Phone number length cannot exceed 15 characters", nameof(number));
        }

        //protected PhoneNumber Parse(string number, NumberingPlanIdentification npi)
        //{
        //    string sanitizedNumber = Regex.Replace(number, @"[\s-()./]", "", RegexOptions.Compiled);
        //    var numericNumberMatch = Regex.Match(sanitizedNumber, @"^(?<prefix>\+?)(?<digits>\d+)$", RegexOptions.Compiled);
        //    if (!numericNumberMatch.Success)
        //        throw new ArgumentException("Invalid number. Expected national type.");
        //    return new NationalPhoneNumber(sanitizedNumber, npi);
        //}

        public override string Number { get; }
    }
}
