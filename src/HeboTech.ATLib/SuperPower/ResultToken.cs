using Superpower.Display;

namespace HeboTech.ATLib.SuperPower
{
    public enum ResultToken
    {
        [Token(Example = "#")]
        CarriageReturn,

        [Token(Example = "\\n")]
        NewLine,

        [Token(Example = "OK")]
        Ok,

        [Token(Example = "ERROR")]
        Error
    }
}
