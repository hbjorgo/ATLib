using HeboTech.ATLib.DTOs;

namespace HeboTech.ATLib.Extensions
{
    internal static class SmsExtensions
    {
        public static SmsWithIndex ToSmsWithIndex(this Sms sms, int index)
        {
            return new SmsWithIndex(sms, index);
        }
    }
}
