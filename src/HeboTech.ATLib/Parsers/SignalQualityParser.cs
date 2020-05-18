using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsers
{
    public static class SignalQualityParser
    {
        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<SignalQualityResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<SignalQualityResult>(Constants.EmptyInput);
                return false;
            }

            var parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Regex.Match(input, @"\+CSQ:\s(?<rssi>\d+),(?<ber>\d+)\r\n"),
                ResponseFormat.Verbose => Regex.Match(input, @"\r\n\+CSQ:\s(?<rssi>\d+),(?<ber>\d+)\r\n"),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.Success)
            {
                result = ATResult.Value(new SignalQualityResult(int.Parse(parseResult.Groups["rssi"].Value), int.Parse(parseResult.Groups["ber"].Value)));
                return true;
            }

            result = ATResult.Error<SignalQualityResult>(Constants.NO_MATCH);
            return false;
        }
    }
}
