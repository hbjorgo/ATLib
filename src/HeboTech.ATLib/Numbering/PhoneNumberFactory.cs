using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Numbering
{
    public static class PhoneNumberFactory
    {
        /// <summary>
        /// Creates a national or international ISDN PhoneNumber.
        /// These are the most common types of numbers used.
        /// <para>E.g. National: 123 45 678</para>
        /// <para>E.g. International: +1 123 45 678</para>
        /// </summary>
        /// <param name="number">The number</param>
        /// <returns>A National or International PhoneNumber</returns>
        /// /// <exception cref="ArgumentException"></exception>
        public static PhoneNumber CreateCommonIsdn(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Number cannot be empty", nameof(number));

            string sanitizedNumber = PhoneNumber.GetSanitizedNumber(number);

            var match = Regex.Match(sanitizedNumber, @"^(?<prefix>\+?)(?<digits>\d+)$", RegexOptions.Compiled);
            if (!match.Success)
                throw new ArgumentException("Invalid phone number");
            string prefix = match.Groups["prefix"].Value;
            string numberOnly = match.Groups["digits"].Value;

            if (prefix == "+")
                return new InternationalPhoneNumber(sanitizedNumber, NumberingPlanIdentification.ISDN);
            else
                return new NationalPhoneNumber(numberOnly, NumberingPlanIdentification.ISDN);
        }

        /// <summary>
        /// Creates a PhoneNumber of the given TypeOfNumber (TON) with the given NumberingPlanIdentification (NPI).
        /// </summary>
        /// <param name="number">The number</param>
        /// <param name="ton">Type Of Number</param>
        /// <param name="npi">Numbering Plan Identification</param>
        /// <returns>A PhoneNumber of the given TON</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static PhoneNumber Create(string number, TypeOfNumber ton, NumberingPlanIdentification npi)
        {
            return ton switch
            {
                TypeOfNumber.Unknown => throw new NotSupportedException(),// new UnknownPhoneNumber(number, npi),
                TypeOfNumber.International => new InternationalPhoneNumber(number, npi),
                TypeOfNumber.National => new NationalPhoneNumber(number, npi),
                TypeOfNumber.NetworkSpecific => throw new NotSupportedException(),// new NetworkSpecificPhoneNumber(number, npi),
                TypeOfNumber.Subscriber => throw new NotSupportedException(),// new SubscriberPhoneNumber(number, npi),
                TypeOfNumber.AlphaNumeric => throw new NotSupportedException(),// new AlphaNumericPhoneNumber(number, npi),
                TypeOfNumber.Abbreviated => throw new NotSupportedException(),// new AbbreviatedPhoneNumber(number, npi),
                TypeOfNumber.ReservedForExtension => throw new NotSupportedException("Number type not supported"),
                _ => throw new NotSupportedException("Number type not supported")
            };
        }
    }
}
