namespace HeboTech.ATLib.Results
{
    public struct ATResult<T>
    {
        public ATResult(T value)
        {
            ErrorMessage = null;
            HasValue = true;
            Value = value;
        }

        public ATResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
            HasValue = false;
            Value = default;
        }

        public string ErrorMessage { get; }
        public bool HasValue { get; }
        public T Value { get; }
    }
}
