using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Parsers
{
    public class BatteryStatusParser
    {
        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<BatteryStatusResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<BatteryStatusResult>(Constants.EmptyInput);
                return false;
            }

            var parseResult = responseFormat switch
            {
                ResponseFormat.Numeric => Regex.Match(input, @"\+CBC: (?<bcs>\d+),(?<bcl>\d+),(?<voltage>\d+)\r\n"),
                ResponseFormat.Verbose => Regex.Match(input, @"\r\n\+CBC: (?<bcs>\d+),(?<bcl>\d+),(?<voltage>\d+)\r\n"),
                _ => throw new NotImplementedException(Constants.PARSER_NOT_IMPLEMENTED),
            };
            if (parseResult.Success)
            {
                result = ATResult.Value(
                    new BatteryStatusResult(
                        (BatteryChargeStatus)int.Parse(parseResult.Groups["bcs"].Value),
                        int.Parse(parseResult.Groups["bcl"].Value),
                        int.Parse(parseResult.Groups["voltage"].Value)));
                return true;
            }

            result = ATResult.Error<BatteryStatusResult>(Constants.NO_MATCH);
            return false;
        }
    }
}
