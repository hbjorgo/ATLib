using HeboTech.ATLib.Results;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace HeboTech.ATLib.Parsers
{
    public static class BatteryStatusParser
    {
        private static class Verbose
        {
            public static TextParser<BatteryStatusResult> Response { get; } =
                (from _ in CommonParsers.Cr
                 from __ in CommonParsers.Lf
                 from ___ in Span.EqualTo("+CBC: ")
                 from bcs in Character.Digit
                 from ____ in Character.EqualTo(',')
                 from bcl in Character.Digit.AtLeastOnce()
                 from _____ in Character.EqualTo(',')
                 from voltage in Character.Digit.AtLeastOnce()
                 from ______ in CommonParsers.Cr
                 from _______ in CommonParsers.Lf
                 select new BatteryStatusResult((BatteryStatusResult.BatteryChargeStatus)(byte)(bcs + - '0'), int.Parse(bcl), int.Parse(voltage)))
                .AtEnd();
        }

        public static class Numeric
        {
            public static TextParser<BatteryStatusResult> Response { get; } =
                 (from _ in Span.EqualTo("+CBC: ")
                  from bcs in Character.Digit
                  from __ in Character.EqualTo(',')
                  from bcl in Character.Digit.AtLeastOnce()
                  from ___ in Character.EqualTo(',')
                  from voltage in Character.Digit.AtLeastOnce()
                  from ____ in CommonParsers.Cr
                  from _____ in CommonParsers.Lf
                  select new BatteryStatusResult((BatteryStatusResult.BatteryChargeStatus)(bcs + -'0'), int.Parse(bcl), int.Parse(voltage)))
                .AtEnd();
        }

        public static bool TryParseVerbose(string input, out BatteryStatusResult result)
        {
            if (input != null)
            {
                Result<BatteryStatusResult> parseResult = Verbose.Response.TryParse(input);
                if (parseResult.HasValue)
                {
                    result = parseResult.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static bool TryParseNumeric(string input, out BatteryStatusResult result)
        {
            if (input != null)
            {
                Result<BatteryStatusResult> parseResult = Numeric.Response.TryParse(input);
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
