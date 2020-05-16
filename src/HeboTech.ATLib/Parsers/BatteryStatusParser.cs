using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
using Superpower;
using Superpower.Parsers;
using System;

namespace HeboTech.ATLib.Parsers
{
    public class BatteryStatusParser
    {
        private static class Verbose
        {
            public static TextParser<BatteryStatusResult> Response { get; } =
                (from _ in CommonParsers.Cr
                 from __ in CommonParsers.Lf
                 from ___ in Span.EqualTo("+CBC: ")
                 from bcs in Numerics.IntegerInt32
                 from ____ in Character.EqualTo(',')
                 from bcl in Numerics.IntegerInt32
                 from _____ in Character.EqualTo(',')
                 from voltage in Numerics.IntegerInt32
                 from ______ in CommonParsers.Cr
                 from _______ in CommonParsers.Lf
                 select new BatteryStatusResult((BatteryChargeStatus)bcs, bcl, voltage))
                .AtEnd();
        }

        public static class Numeric
        {
            public static TextParser<BatteryStatusResult> Response { get; } =
                 (from _ in Span.EqualTo("+CBC: ")
                  from bcs in Numerics.IntegerInt32
                  from __ in Character.EqualTo(',')
                  from bcl in Numerics.IntegerInt32
                  from ___ in Character.EqualTo(',')
                  from voltage in Numerics.IntegerInt32
                  from ____ in CommonParsers.Cr
                  from _____ in CommonParsers.Lf
                  select new BatteryStatusResult((BatteryChargeStatus)bcs, bcl, voltage))
                .AtEnd();
        }

        public static bool TryParse(string input, ResponseFormat responseFormat, out ATResult<BatteryStatusResult> result)
        {
            if (input == null)
            {
                result = ATResult.Error<BatteryStatusResult>(Constants.EmptyInput);
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

            result = ATResult.Error<BatteryStatusResult>(parseResult.ErrorMessage);
            return false;
        }
    }
}
