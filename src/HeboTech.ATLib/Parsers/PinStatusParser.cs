using HeboTech.ATLib.Results;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace HeboTech.ATLib.Parsers
{
    public static class PinStatusParser
    {
        private static class Verbose
        {
            public static TextParser<PinStatusResult> Response { get; } =
                (from _ in CommonParsers.Cr
                 from __ in CommonParsers.Lf
                 from ___ in Span.EqualTo("+CPIN: ")
                 from code in Character.Letter.AtLeastOnce()
                 from ____ in CommonParsers.Cr
                 from _____ in CommonParsers.Lf
                 select new PinStatusResult(new string(code)))
                .AtEnd();
        }

        public static class Numeric
        {
            public static TextParser<PinStatusResult> Response { get; } =
                 (from _ in Span.EqualTo("+CPIN: ")
                  from code in Character.Letter.AtLeastOnce()
                  from __ in CommonParsers.Cr
                  from ___ in CommonParsers.Lf
                  select new PinStatusResult(new string(code)))
                .AtEnd();
        }

        public static bool TryParseVerbose(string input, out PinStatusResult result)
        {
            if (input != null)
            {
                Result<PinStatusResult> parseResult = Verbose.Response.TryParse(input);
                if (parseResult.HasValue)
                {
                    result = parseResult.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static bool TryParseNumeric(string input, out PinStatusResult result)
        {
            if (input != null)
            {
                Result<PinStatusResult> parseResult = Numeric.Response.TryParse(input);
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
