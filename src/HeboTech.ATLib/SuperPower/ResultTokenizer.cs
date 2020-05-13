using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace HeboTech.ATLib.SuperPower
{
    public static class ResultTokenizer
    {
        public static TextParser<TextSpan> OK { get; } = Span.EqualTo("OK");
        public static TextParser<TextSpan> ERROR { get; } = Span.EqualTo("ERROR");

        static Tokenizer<ResultToken> Tokenizer => new TokenizerBuilder<ResultToken>()
        .Match(Character.EqualTo('#'), ResultToken.CarriageReturn, requireDelimiters: false)
        .Match(Character.EqualTo('\n'), ResultToken.NewLine, requireDelimiters: false)
        .Match(OK, ResultToken.Ok, requireDelimiters: false)
        .Match(ERROR, ResultToken.Error, requireDelimiters: false)
        .Build();

        public static Result<TokenList<ResultToken>> TryTokenize(string source)
        {
            return Tokenizer.TryTokenize(source);
        }
    }
}
