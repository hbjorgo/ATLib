namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// PhoneNumber with TypeOfNumber (TON) 0x03
    /// </summary>
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
