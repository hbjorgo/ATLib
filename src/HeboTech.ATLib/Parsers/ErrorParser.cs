using HeboTech.ATLib.Results;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace HeboTech.ATLib.Parsers
{
    public static class ErrorParser
    {
        private static class Verbose
        {
            public static TextParser<ErrorResult> Response { get; } =
                (from _ in CommonParsers.Cr
                 from __ in CommonParsers.Lf
                 from error in Span.EqualTo("ERROR").Select(c => c.ToStringValue())
                 from ___ in CommonParsers.Cr
                 from ____ in CommonParsers.Lf
                 select new ErrorResult())
                .AtEnd();
        }

        private static class Numeric
        {
            public static TextParser<ErrorResult> Response { get; } =
                (from _ in CommonParsers.Cr
                 from __ in CommonParsers.Lf
                 from ___ in Character.EqualTo('4')
                 from ____ in CommonParsers.Cr
                 from _____ in CommonParsers.Lf
                 select new ErrorResult())
                .AtEnd();
        }

        public static bool TryParseVerbose(string input, out ErrorResult result)
        {
            if (input != null)
            {
                Result<ErrorResult> parseResult = Verbose.Response.TryParse(input);
                if (parseResult.HasValue)
                {
                    result = parseResult.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static bool TryParseNumeric(string input, out ErrorResult result)
        {
            if (input != null)
            {
                Result<ErrorResult> parseResult = Numeric.Response.TryParse(input);
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
