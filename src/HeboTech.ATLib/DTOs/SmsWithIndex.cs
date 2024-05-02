namespace HeboTech.ATLib.DTOs
{
    public class SmsWithIndex
    {
        public SmsWithIndex(Sms sms, int index)
        {
            Sms = sms;
            Index = index;
        }

        public Sms Sms { get; }
        public int Index { get; }
    }
}