using HeboTech.ATLib.DTOs;

namespace HeboTech.ATLib.Extensions
{
    internal static class SmsDeliverExtensions
    {
        public static Sms ToSms(this SmsDeliver sms, SmsStatus status)
        {
            return new Sms(status, sms.SenderNumber, sms.Timestamp, sms.Message, sms.MessageReferenceNumber, sms.TotalNumberOfParts, sms.PartNumber);
        }

        public static SmsWithIndex ToSmsWithIndex(this SmsDeliver sms, int index, SmsStatus status)
        {
            return new SmsWithIndex(index, status, sms.SenderNumber, sms.Timestamp, sms.Message, sms.MessageReferenceNumber, sms.TotalNumberOfParts, sms.PartNumber);
        }
    }
}
