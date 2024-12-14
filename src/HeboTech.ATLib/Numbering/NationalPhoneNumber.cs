using System;

namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// PhoneNumber with TypeOfNumber (TON) 0x02
    /// </summary>
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

        public override string Number { get; }
    }
}
