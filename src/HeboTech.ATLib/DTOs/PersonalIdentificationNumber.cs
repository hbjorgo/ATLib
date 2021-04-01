using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.DTOs
{
    public class PersonalIdentificationNumber
    {
        public PersonalIdentificationNumber(string pin)
        {
            if (!Regex.IsMatch(pin, @"^\d{4}$"))
                throw new ArgumentException("Invalid PIN");
            Pin = pin;
        }

        public string Pin { get; }

        public override string ToString()
        {
            return Pin;
        }
    }
}
