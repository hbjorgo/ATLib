using HeboTech.ATLib.Results;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace HeboTech.ATLib.SuperPower.Parsers
{
    public static class ErrorParser
    {
        private static TextParser<string> Error { get; } = Span.EqualTo("ERROR").Select(c => c.ToStringValue());

        private static TextParser<ErrorResult> ErrorResult { get; } =
            //from _ in CommonParsers.Cr
            //from __ in CommonParsers.Lf
            from error in Error
            from ___ in CommonParsers.Cr
            from ____ in CommonParsers.Lf
            select new ErrorResult();

        private static TextParser<ErrorResult> ErrorResponse { get; } = ErrorResult.AtEnd();

        public static bool TryParse(string input, out ErrorResult result)
        {
            if (input != null)
            {
                Result<ErrorResult> parseResult = ErrorResponse.TryParse(input);
                if (parseResult.HasValue)
                {
                    result = parseResult.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }
    }
}
