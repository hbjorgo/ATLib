using HeboTech.ATLib.DTOs;

namespace HeboTech.ATLib.Extensions
{
    internal static class SmsExtensions
    {
        public static SmsBaseWithIndex ToSmsWithIndex(this SmsBase sms, int index)
        {
            return new SmsBaseWithIndex(sms, index);
        }
    }
}
