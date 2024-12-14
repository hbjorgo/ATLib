namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// PhoneNumber with TypeOfNumber (TON) 0x05
    /// </summary>
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
