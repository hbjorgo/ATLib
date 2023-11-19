namespace HeboTech.ATLib.Parsers
{
    public class ModemResponse
    {
        public ModemResponse(bool isSuccess, Error error)
        {
            Success = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Indicates whether the command was successful or not.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// This property is only valid if 'Success' is false.
        /// </summary>
        public Error Error { get; }

        public override string ToString()
        {
            if (Success)
                return "Success";
            else
                return $"Error: {Error}";
        }

        public static ModemResponse IsSuccess() =>
            new ModemResponse(true, default);

        public static ModemResponse IsSuccess(bool isSuccess) =>
            new ModemResponse(isSuccess, default);

        public static ModemResponse HasError() =>
            new ModemResponse(false, default);

        public static ModemResponse HasError(Error error) =>
            new ModemResponse(false, error);

        public static ModemResponse<T> IsResultSuccess<T>(T result) =>
            new ModemResponse<T>(true, default, result);

        public static ModemResponse<T> HasResultError<T>() =>
            new ModemResponse<T>(false, default, default);

        public static ModemResponse<T> HasResultError<T>(Error error) =>
            new ModemResponse<T>(false, error, default);
    }

    public class ModemResponse<T> : ModemResponse
    {
        public ModemResponse(bool success, Error error, T result)
            : base(success, error)
        {
            Result = result;
        }

        /// <summary>
        /// This property is only valid if 'Success' is true.
        /// </summary>
        public T Result { get; }

        public override string ToString()
        {
            if (Success)
                return $"{Result}";
            else
                return $"Error: {Error}";
        }
    }
}
