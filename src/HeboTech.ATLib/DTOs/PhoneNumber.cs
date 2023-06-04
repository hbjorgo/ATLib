namespace HeboTech.ATLib.DTOs
{
    public class PhoneNumber
    {
        public PhoneNumber(string number)
        {
            Number = number;
        }

        public string Number { get; }

        public override string ToString()
        {
            return Number;
        }
    }
}
