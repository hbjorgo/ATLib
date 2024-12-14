namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// PhoneNumber with TypeOfNumber (TON) 0x00
    /// </summary>
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
