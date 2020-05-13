using HeboTech.ATLib.Results;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace HeboTech.ATLib.SuperPower.Parsers
{
    public static class OkParser
    {
        private static TextParser<string> Ok { get; } = Span.EqualTo("OK").Select(c => c.ToStringValue());

        private static TextParser<OkResult> OkResult { get; } =
            //from _ in CommonParsers.Cr
            //from __ in CommonParsers.Lf
            from ok in Ok
            from ___ in CommonParsers.Cr
            from ____ in CommonParsers.Lf
            select new OkResult();

        private static TextParser<OkResult> OkResponse { get; } = OkResult.AtEnd();

        public static bool TryParse(string input, out OkResult result)
        {
            if (input != null)
            {
                Result<OkResult> parseResult = OkResponse.TryParse(input);
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
