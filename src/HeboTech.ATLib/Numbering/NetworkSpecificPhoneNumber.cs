﻿namespace HeboTech.ATLib.Numbering
{
    public class NetworkSpecificPhoneNumber : PhoneNumber
    {
        internal NetworkSpecificPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.NetworkSpecific, npi)
        {
            ThrowIfEmpty(number);
            string sanitizedNumber = GetSanitizedNumber(number);

            Number = sanitizedNumber;
        }

        public override string Number { get; }
    }
}
