namespace HeboTech.ATLib.Numbering
{
    public class AlphaNumericPhoneNumber : PhoneNumber
    {
        internal AlphaNumericPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.AlphaNumeric, npi)
        {
            ThrowIfEmpty(number);
            Number = number;
        }

        public override string Number { get; }
    }
}
