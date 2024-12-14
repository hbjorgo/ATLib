namespace HeboTech.ATLib.Numbering
{
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
