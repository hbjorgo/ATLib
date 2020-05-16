namespace HeboTech.ATLib.Results
{
    public static class ATResult
    {
        public static ATResult<T> Value<T>(T value)
        {
            return new ATResult<T>(value);
        }

        public static ATResult<T> Error<T>(string error)
        {
            return new ATResult<T>(error);
        }
    }
}
