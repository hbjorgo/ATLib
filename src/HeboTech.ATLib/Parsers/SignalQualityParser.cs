using HeboTech.ATLib.Results;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace HeboTech.ATLib.Parsers
{
    public static class SignalQualityParser
    {
        private static class Verbose
        {
            public static TextParser<SignalQualityResult> Response { get; } =
                (from _ in CommonParsers.Cr
                 from __ in CommonParsers.Lf
                 from ___ in Span.EqualTo("+CSQ: ")
                 from rssi in Numerics.IntegerInt32
                 from ____ in Character.EqualTo(',')
                 from ber in Numerics.IntegerInt32
                 from ______ in CommonParsers.Cr
                 from _______ in CommonParsers.Lf
                 select new SignalQualityResult(rssi, ber))
                .AtEnd();
        }

        public static class Numeric
        {
            public static TextParser<SignalQualityResult> Response { get; } =
                  (from _ in Span.EqualTo("+CSQ: ")
                   from rssi in Numerics.IntegerInt32
                   from __ in Character.EqualTo(',')
                   from ber in Numerics.IntegerInt32
                   from ___ in CommonParsers.Cr
                   from ____ in CommonParsers.Lf
                   select new SignalQualityResult(rssi, ber))
                .AtEnd();
        }

        public static bool TryParseVerbose(string input, out SignalQualityResult result)
        {
            if (input != null)
            {
                Result<SignalQualityResult> parseResult = Verbose.Response.TryParse(input);
                if (parseResult.HasValue)
                {
                    result = parseResult.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static bool TryParseNumeric(string input, out SignalQualityResult result)
        {
            if (input != null)
            {
                Result<SignalQualityResult> parseResult = Numeric.Response.TryParse(input);
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
