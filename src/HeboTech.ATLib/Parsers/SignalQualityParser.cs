using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using Superpower;
using Superpower.Parsers;
using System;

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

        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<SignalQualityResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<SignalQualityResult>(Constants.EmptyInput);
                return false;
            }

            var parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Numeric.Response.TryParse(input),
                ResponseFormat.Verbose => Verbose.Response.TryParse(input),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.HasValue)
            {
                result = ATResult.Value(parseResult.Value);
                return true;
            }

            result = ATResult.Error<SignalQualityResult>(parseResult.ErrorMessage);
            return false;
        }
    }
}
