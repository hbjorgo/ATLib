using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// Phone Number
    /// </summary>
    public abstract class PhoneNumber
    {
        internal protected PhoneNumber(TypeOfNumber ton, NumberingPlanIdentification npi)
        {
            TypeOfNumber = ton;
            NumberingPlanIdentification = npi;
        }

        /// <summary>
        /// Type of number
        /// </summary>
        public TypeOfNumber TypeOfNumber { get; }

        /// <summary>
        /// Numbering plan identification
        /// </summary>
        public NumberingPlanIdentification NumberingPlanIdentification { get; }

        /// <summary>
        /// Full number (without any prefixes (if any))
        /// </summary>
        public abstract string Number { get; }

        /// <summary>
        /// Remove any characters used for readability
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        internal static string GetSanitizedNumber(string number) =>
            Regex.Replace(number, @"[\s-()./]", "", RegexOptions.Compiled);

        protected static void ThrowIfEmpty(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Number cannot be empty", nameof(number));
        }

        public static implicit operator string(PhoneNumber phoneNumber) =>
            phoneNumber.ToString();

        public override string ToString() =>
            Number;
    }
}
