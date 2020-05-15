namespace HeboTech.ATLib.Results
{
    public class PinStatusResult : ATResult
    {
        public PinStatusResult(string status)
        {
            Status = status;
        }

        public string Status { get; }

        public override string ToString()
        {
            return Status;
        }
    }
}
