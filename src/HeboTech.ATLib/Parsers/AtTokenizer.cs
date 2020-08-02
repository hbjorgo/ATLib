namespace HeboTech.ATLib.Parsers
{
    public class AtTokenizer
    {
        public static string TokenizeStart(string input)
        {
            if (input == null)
                return null;

            int pos = input.IndexOf(':');
            if (pos < 0 || pos == input.Length - 1)
                return null;

            return input.Substring(pos + 1);
        }

        private static string NextToken(string input, out string token)
        {
            token = null;
            if (input == null)
                return null;

            input.Replace(" ", string.Empty);

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
                token = input.Substring(1, pos);
                if (pos == input.Length)
                    return null;
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
    }
}
