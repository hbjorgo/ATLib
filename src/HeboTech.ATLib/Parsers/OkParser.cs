using HeboTech.ATLib.Results;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace HeboTech.ATLib.Parsers
{
    public static class OkParser
    {
        private static class Verbose
        {
            public static TextParser<OkResult> Response { get; } =
                (from _ in CommonParsers.Cr
                 from __ in CommonParsers.Lf
                 from ok in Span.EqualTo("OK").Select(c => c.ToStringValue())
                 from ___ in CommonParsers.Cr
                 from ____ in CommonParsers.Lf
                 select new OkResult())
                .AtEnd();
        }

        private static class Numeric
        {
            public static TextParser<OkResult> Response { get; } =
                (from _ in Character.EqualTo('0')
                 from __ in CommonParsers.Cr
                 from ___ in CommonParsers.Lf
                 select new OkResult())
                .AtEnd();
        }

        public static bool TryParseVerbose(string input, out OkResult result)
        {
            if (input != null)
            {
                Result<OkResult> parseResult = Verbose.Response.TryParse(input);
                if (parseResult.HasValue)
                {
                    result = parseResult.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static bool TryParseNumeric(string input, out OkResult result)
        {
            if (input != null)
            {
                Result<OkResult> parseResult = Numeric.Response.TryParse(input);
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
