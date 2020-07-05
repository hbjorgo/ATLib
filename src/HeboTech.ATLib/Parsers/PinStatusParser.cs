using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsers
{
    public static class PinStatusParser
    {
        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<PinStatusResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<PinStatusResult>(Constants.EmptyInput);
                return false;
            }

            var parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Regex.Match(input, @"\+CPIN:\s(?<code>\w+\s?\w+)"),
                ResponseFormat.Verbose => Regex.Match(input, @"\r\n\+CPIN:\s(?<code>\w+\s?\w+)\r\n"),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.Success)
            {
                result = ATResult.Value(new PinStatusResult(parseResult.Groups["code"].Value));
                return true;
            }

            result = ATResult.Error<PinStatusResult>(Constants.NO_MATCH);
            return false;
        }
    }
}
