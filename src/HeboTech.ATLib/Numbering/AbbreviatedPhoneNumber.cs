namespace HeboTech.ATLib.Numbering
{
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
