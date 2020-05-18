using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsers
{
    public static class OkParser
    {
        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<OkResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<OkResult>(Constants.EmptyInput);
                return false;
            }

            var parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Regex.Match(input, "0\r\n"),
                ResponseFormat.Verbose => Regex.Match(input, "\r\nOK\r\n"),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.Success)
            {
                result = ATResult.Value(new OkResult());
                return true;
            }

            result = ATResult.Error<OkResult>(Constants.NO_MATCH);
            return false;
        }
    }
}
