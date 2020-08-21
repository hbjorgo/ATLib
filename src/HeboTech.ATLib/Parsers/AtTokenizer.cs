using System;

namespace HeboTech.ATLib.Parsers
{
    public class AtTokenizer
    {
        public static bool TokenizeStart(string input, out string output)
        {
            if (input == null)
            {
                output = null;
                return false;
            }

            int pos = input.IndexOf(':');
            if (pos < 0 || pos == input.Length - 1)
            {
                output = null;
                return false;
            }

            output = input.Substring(pos + 1);
            return true;
        }

        private static string NextToken(string input, out string token)
        {
            token = null;
            if (input == null)
                return null;

            input = input.Replace(" ", string.Empty);

            if (string.IsNullOrEmpty(input))
                return null;
            else if (input.StartsWith('"'))
            {
                var pos = input.IndexOf('"', 1);
                if (pos == -1)
                    pos = input.Length;
                token = input.Substring(1, pos);
                if (pos == input.Length)
                    return null;
                return input.Substring(pos);
            }
            else
            {
                var pos = input.IndexOf(',');
                if (pos == -1)
                    pos = input.Length;
                token = input.Substring(0, pos);
                if (pos == input.Length)
                    return string.Empty;
                return input.Substring(pos);
            }
        }

        public static string TokenizeNextString(string input, out string token)
        {
            if (input == null)
            {
                token = null;
                return null;
            }

            return NextToken(input, out token);
        }

        public static bool HasMoreTokens(string input)
        {
            return !(input == null || string.IsNullOrWhiteSpace(input));
        }

        public static bool TokenizeNextInt(string input, out int result)
        {
            return TokenizeNextIntBase(input, out result, 10, false);
        }

        private static bool TokenizeNextIntBase(string input, out int result, int @base, bool unsigned)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            NextToken(input, out string token);

            if (token == null)
            {
                return false;
            }
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
