namespace HeboTech.ATLib.Numbering
{
    /// <summary>
    /// PhoneNumber with TypeOfNumber (TON) 0x04
    /// </summary>
    public class SubscriberPhoneNumber : PhoneNumber
    {
        internal SubscriberPhoneNumber(string number, NumberingPlanIdentification npi)
            : base(TypeOfNumber.Subscriber, npi)
        {
            ThrowIfEmpty(number);
            string sanitizedNumber = GetSanitizedNumber(number);

            Number = sanitizedNumber;
        }

        public override string Number { get; }
    }
}
