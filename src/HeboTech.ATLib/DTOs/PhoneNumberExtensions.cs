namespace HeboTech.ATLib.DTOs
{
    public static class PhoneNumberExtensions
    {
        public static TypeOfNumber GetTypeOfNumber(this PhoneNumber phoneNumber)
        {
#if NETSTANDARD2_0
            if (phoneNumber.Number.StartsWith("+"))
#else
            if (phoneNumber.Number.StartsWith('+'))
#endif
                return TypeOfNumber.International;
            else
                return TypeOfNumber.National;
        }

        public static NumberPlanIdentification GetNumberPlanIdentification(this PhoneNumber phoneNumber)
        {
#if NETSTANDARD2_0
            if (phoneNumber.Number.StartsWith("+"))
#else
            if (phoneNumber.Number.StartsWith('+'))
#endif
                return NumberPlanIdentification.ISDN;
            else
                return NumberPlanIdentification.Unknown;
        }
    }
}
