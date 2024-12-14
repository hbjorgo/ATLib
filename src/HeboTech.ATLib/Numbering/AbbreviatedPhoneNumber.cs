namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// PhoneNumber with TypeOfNumber (TON) 0x06
    /// </summary>
    public class AbbreviatedPhoneNumber : PhoneNumber
    {
        internal protected AbbreviatedPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.Abbreviated, npi)
        {
            ThrowIfEmpty(number);
            string sanitizedNumber = GetSanitizedNumber(number);

            Number = sanitizedNumber;
        }

        public override string Number { get; }
    }
}
