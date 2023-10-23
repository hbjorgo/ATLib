using System;
using System.Linq;

namespace HeboTech.ATLib.DTOs
{
    public class PhoneNumberV2
    {
        public PhoneNumberV2(string subscriberNumber)
            : this(string.Empty, subscriberNumber)
        {
        }

        public PhoneNumberV2(string countryCode, string subscriberNumber)
        {
            if (!countryCode.All(char.IsDigit))
                throw new ArgumentException("Must be numeric only", nameof(countryCode));
            if (!subscriberNumber.All(char.IsDigit))
                throw new ArgumentException("Must be numeric only", nameof(subscriberNumber));

            CountryCode = countryCode;
            SubscriberNumber = subscriberNumber;
        }

        public string CountryCode { get; }
        public string SubscriberNumber { get; }

        public TypeOfNumber GetTypeOfNumber()
        {
            if (CountryCode != string.Empty)
                return TypeOfNumber.International;
            else
                return TypeOfNumber.National;
        }

        public NumberPlanIdentification GetNumberPlanIdentification()
        {
            if (CountryCode != string.Empty)
                return NumberPlanIdentification.ISDN;
            else
                return NumberPlanIdentification.Unknown;
        }

        public override string ToString()
        {
            if (CountryCode != string.Empty)
                return $"+{CountryCode}{SubscriberNumber}";
            return SubscriberNumber.ToString();
        }
    }
}
