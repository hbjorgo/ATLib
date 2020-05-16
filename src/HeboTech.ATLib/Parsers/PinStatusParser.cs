using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using Superpower;
using Superpower.Parsers;
using System;

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

        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<PinStatusResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<PinStatusResult>(Constants.EmptyInput);
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

            result = ATResult.Error<PinStatusResult>(parseResult.ErrorMessage);
            return false;
        }
    }
}
