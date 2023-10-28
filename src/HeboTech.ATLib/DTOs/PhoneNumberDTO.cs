namespace HeboTech.ATLib.DTOs
{
    public class PhoneNumberDTO
    {
        public PhoneNumberDTO(string number)
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
