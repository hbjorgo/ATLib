namespace HeboTech.ATLib
{
    public class PhoneNumber
    {
        private readonly string number;

        public PhoneNumber(string number)
        {
            this.number = number;
        }

        public override string ToString()
        {
            return number;
        }
    }
}
