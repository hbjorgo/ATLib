namespace HeboTech.ATLib.Results
{
    public class SmsSentResult
    {
        public SmsSentResult(int messageReferenceNumber)
        {
            MessageReferenceNumber = messageReferenceNumber;
        }

        public int MessageReferenceNumber { get; }
    }
}
