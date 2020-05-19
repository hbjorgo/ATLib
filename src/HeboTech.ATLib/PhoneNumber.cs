namespace HeboTech.ATLib
{
    public class PhoneNumber
    {
        public PhoneNumber(string number)
        {
            Number = number;
            if (number.StartsWith('+'))
                Format = PhoneNumberFormat.International;
            else
                Format = PhoneNumberFormat.National;
        }

        public PhoneNumberFormat Format { get; }
        public string Number { get; set; }

        public override string ToString()
        {
            return Number;
        }
    }
}
