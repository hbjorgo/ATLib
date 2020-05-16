using Superpower;
using Superpower.Parsers;

namespace HeboTech.ATLib.Parsers
{
    public static class CommonParsers
    {
        public static TextParser<char> Cr { get; } = Character.EqualTo('\r');
        public static TextParser<char> Lf { get; } = Character.EqualTo('\n');
        public static TextParser<char> WhiteSpace { get; } = Character.EqualTo(' ');
    }
}
