namespace HeboTech.ATLib.DTOs
{
    public class PhoneNumber
    {
        public PhoneNumber(string number)
        {
            if (number.StartsWith('+'))
            {
                Ton = TypeOfNumber.International;
                Npi = NumberPlanIdentification.ISDN;
            }
            else
            {
                Ton = TypeOfNumber.National;
                Npi = NumberPlanIdentification.Unknown;
            }
            Number = number.TrimStart('+');
        }

        public PhoneNumber(string number, TypeOfNumber ton, NumberPlanIdentification npi)
        {
            Ton = ton;
            Number = number;
            Npi = npi;
        }

        public TypeOfNumber Ton { get; }
        public NumberPlanIdentification Npi { get; }
        public string Number { get; set; }

        public override string ToString()
        {
            return Number;
        }
    }
}
