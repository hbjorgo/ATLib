using System;

namespace HeboTech.ATLib.Numbering
{
    internal class UnknownPhoneNumber : PhoneNumber
    {
        public UnknownPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.Unknown, npi)
        {
            ThrowIfEmpty(number);
            string sanitizedNumber = GetSanitizedNumber(number);

            Number = sanitizedNumber;
        }

        public override string Number { get; }
    }
}
