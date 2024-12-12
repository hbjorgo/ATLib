namespace HeboTech.ATLib.Misc
{
    public class Imsi
    {
        public Imsi(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}
