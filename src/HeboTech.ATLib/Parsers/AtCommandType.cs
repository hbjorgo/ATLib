namespace HeboTech.ATLib.Parsers
{
    internal enum AtCommandType
    {
        NO_RESULT, // No intermediate response expected
        NUMERIC, // A single intermediate response starting with a 0-9
        SINGELLINE, // A single intermediate response starting with a prefix
        MULTILINE, // Multiple line intermediate response starting with a prefix
        MULTILINE_NO_PREFIX // Multiple line intermediate response without a prefix
    }
}
