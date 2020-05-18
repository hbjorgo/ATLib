using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsers
{
    public static class ErrorParser
    {
        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<ErrorResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<ErrorResult>(Constants.EmptyInput);
                return false;
            }

            var parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Regex.Match(input, "\r\n4\r\n"),
                ResponseFormat.Verbose => Regex.Match(input, "\r\nERROR\r\n"),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.Success)
            {
                result = ATResult.Value(new ErrorResult());
                return true;
            }

            result = ATResult.Error<ErrorResult>(Constants.NO_MATCH);
            return false;
        }
    }
}
