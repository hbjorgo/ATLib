﻿using HeboTech.ATLib.Results;
using HeboTech.ATLib.States;
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
