using System;
using System.Data;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// Phone Number
    /// </summary>
    public abstract class PhoneNumber
    {
        protected PhoneNumber(TypeOfNumber ton, NumberingPlanIdentification npi)
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
        /// Full number with prefix (if any)
        /// </summary>
        public abstract string NumberWithPrefix { get; }

        /// <summary>
        /// Full number without prefix
        /// </summary>
        public abstract string NumberWithoutPrefix { get; }

        protected static void ThrowIfNotValid(string number)
        {
            if (number == null)
                throw new ArgumentNullException(nameof(number), "Number cannot be empty");
            if (string.IsNullOrEmpty(number))
                throw new ArgumentNullException(nameof(number), "Number cannot be empty");
            if (number.Length > 15)
                throw new ArgumentException("Total phone number length cannot exceed 15 characters");
        }

        protected static string GetSanitizedNumber(string number) =>
            Regex.Replace(number, @"[\s-()./]", "", RegexOptions.Compiled);

        public static implicit operator string(PhoneNumber phoneNumber) =>
            phoneNumber.ToString();

        public override string ToString() =>
            NumberWithPrefix;
    }
}
