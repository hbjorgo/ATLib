namespace HeboTech.ATLib.DTOs
{
    public class SmsReference
    {
        public SmsReference(int messageReference)
        {
            MessageReference = messageReference;
        }

        public int MessageReference { get; }

        public override string ToString()
        {
            return $"Message Reference: {MessageReference}";
        }
    }
}
