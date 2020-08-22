namespace HeboTech.ATLib.Parsers
{
    public static class ImprovedTokenizer
    {
        public static bool TokenizeStart(string input, out string remainder)
        {
            var split = input.Split(':', 2);
            if (split.Length == 2)
            {
                remainder = split[1];
                return true;
            }

            remainder = null;
            return false;
        }

        public static bool TokenizeNextString(string input, out string remainder, out string token)
        {
            if (input == null)
            {
                remainder = null;
                token = null;
                return false;
            }

            remainder = NextToken(input, out token);
            return token != null;
        }

        private static string NextToken(string input, out string token)
        {
            if (input == null)
            {
                token = null;
                return null;
            }

            input = input.TrimStart();

            if (input.StartsWith('"'))
            {
                var split = input.Split('\"', 2);
                if (split.Length == 2)
                {
                    token = split[0];
                    return split[1];
                }
                else
                {
                    token = split[0];
                    return null;
                }
            }
            else
            {
                var split = input.Split(',', 2);
                if (split.Length == 2)
                {
                    token = split[0];
                    return split[1];
                }
                else
                {
                    token = split[0];
                    return null;
                }
            }
        }

        public static bool TokenizeNextInt(string input, out string remainder, out int result)
        {
            return TokenizeNextIntBase(input, out remainder, out result, 10, false);
        }

        private static bool TokenizeNextIntBase(string input, out string remainder, out int result, int @base, bool unsigned)
        {
            remainder = null;
            result = 0;

            if (input == null)
                return false;

            remainder = NextToken(input, out string token);

            if (token == null)
                return false;
            else
            {
                if (unsigned)
                {
                    if (ulong.TryParse(token, out ulong l))
                    {
                        result = (int)l;
                        return true;
                    }
                }
                else
                {
                    if (long.TryParse(token, out long l))
                    {
                        result = (int)l;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
