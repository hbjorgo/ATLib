using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using Superpower;
using Superpower.Parsers;
using System;

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

        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<ErrorResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<ErrorResult>(Constants.EmptyInput);
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

            result = ATResult.Error<ErrorResult>(parseResult.ErrorMessage);
            return false;
        }
    }
}
