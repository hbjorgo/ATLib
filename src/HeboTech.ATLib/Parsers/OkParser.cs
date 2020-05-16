using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using Superpower;
using Superpower.Parsers;
using System;

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

        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<OkResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<OkResult>(Constants.EmptyInput);
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

            result = ATResult.Error<OkResult>(parseResult.ErrorMessage);
            return false;
        }
    }
}
