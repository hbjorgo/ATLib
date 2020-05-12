namespace HeboTech.ATLib.Pipelines
{
    public class ATResult
    {
        public ATResult(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
