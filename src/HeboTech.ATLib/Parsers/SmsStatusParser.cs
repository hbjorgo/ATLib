using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsers
{
    public static class SmsStatusParser
    {
        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<SmsSentResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<SmsSentResult>(Constants.EmptyInput);
                return false;
            }

            // Success
            var parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Regex.Match(input, @"\+CMGS: (?<mr>\d+)"),
                ResponseFormat.Verbose => Regex.Match(input, @"\r\n\+CMGS: (?<mr>\d+)"),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.Success)
            {
                result = ATResult.Value(new SmsSentResult(int.Parse(parseResult.Groups["mr"].Value)));
                return true;
            }

            // Error
            parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Regex.Match(input, @"\+CMS ERROR: .*"),
                ResponseFormat.Verbose => Regex.Match(input, @"\r\n\+CMS ERROR: (?<errMsg>.*\r\n"),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.Success)
            {
                result = ATResult.Error<SmsSentResult>(parseResult.Groups["errMsg"].Value);
                return true;
            }

            result = ATResult.Error<SmsSentResult>(Constants.NO_MATCH);
            return false;
        }
    }
}
