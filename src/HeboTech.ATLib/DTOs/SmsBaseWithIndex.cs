namespace HeboTech.ATLib.DTOs
{
    public class SmsBaseWithIndex
    {
        public SmsBaseWithIndex(SmsBase sms, int index)
        {
            Sms = sms;
            Index = index;
        }

        public SmsBase Sms { get; }
        public int Index { get; }
    }
}