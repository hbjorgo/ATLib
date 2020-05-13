using Superpower;
using Superpower.Parsers;

namespace HeboTech.ATLib.SuperPower.Parsers
{
    public static class CommonParsers
    {
        public static TextParser<char> Cr { get; } = Character.EqualTo('\r');
        public static TextParser<char> Lf { get; } = Character.EqualTo('\n');
    }
}
