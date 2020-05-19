namespace HeboTech.ATLib.Parsers
{
    public static class Regexes
    {
        public static class Verbose
        {
            public const string OK = "\r\nOK\r\n";
            public const string ERROR = "\r\nERROR\r\n";
        }

        public static class Numeric
        {
            public const string OK = "0\r";
            public const string ERROR = "4\r";
        }
    }
}
