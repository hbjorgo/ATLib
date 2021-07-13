namespace HeboTech.ATLib.Events
{
    public class ErrorEventArgs
    {
        public ErrorEventArgs(string error)
        {
            Error = error;
        }

        public string Error { get; }

        internal static ErrorEventArgs CreateFromCmeResponse(string response)
        {
            return new ErrorEventArgs(response);
        }

        internal static ErrorEventArgs CreateFromCmsResponse(string response)
        {
            return new ErrorEventArgs(response);
        }
    }
}
