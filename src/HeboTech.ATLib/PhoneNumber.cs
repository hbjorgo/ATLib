using System.ComponentModel.DataAnnotations;

namespace HeboTech.ATLib
{
    public class PhoneNumber
    {
        private readonly string number;

        public PhoneNumber(string number)
        {
            (new PhoneAttribute()).Validate(number, nameof(number));
            this.number = number;
        }

        public override string ToString()
        {
            return number;
        }
    }
}
