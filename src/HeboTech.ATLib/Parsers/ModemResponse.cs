namespace HeboTech.ATLib.Parsers
{
    public class ModemResponse
    {
        public ModemResponse(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Indicates whether the command was successful or not.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// This property is only valid if 'Success' is false.
        /// </summary>
        public string ErrorMessage { get; }

        public override string ToString()
        {
            if (IsSuccess)
                return "Success";
            else
                return $"Error: {ErrorMessage}";
        }

        public static ModemResponse Success() =>
            new ModemResponse(true, default);

        public static ModemResponse Success(bool isSuccess) =>
            new ModemResponse(isSuccess, default);

        public static ModemResponse Error(string error = "") =>
            new ModemResponse(false, error);

        public static ModemResponse<T> ResultSuccess<T>(T result) =>
            new ModemResponse<T>(true, default, result);

        public static ModemResponse<T> ResultError<T>(string error = "") =>
            new ModemResponse<T>(false, error, default);
    }

    public class ModemResponse<T> : ModemResponse
    {
        public ModemResponse(bool success, string error, T result)
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
            if (IsSuccess)
                return $"{Result}";
            else
                return $"Error: {ErrorMessage}";
        }
    }
}
