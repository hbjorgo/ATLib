namespace HeboTech.ATLib.DTOs
{
    public class PhoneNumber
    {
        public PhoneNumber(string number)
        {
            Number = number;
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
        }

        public PhoneNumber(string number, TypeOfNumber ton, NumberPlanIdentification npi)
        {
            Ton = ton;
            Number = number;
        }

        public TypeOfNumber Ton { get; }
        public NumberPlanIdentification Npi { get; }
        public string Number { get; set; }
        public byte AddressType => (byte)(0b1000_0000 + (byte)Ton + (byte)Npi);

        public override string ToString()
        {
            return Number;
        }
    }
}
