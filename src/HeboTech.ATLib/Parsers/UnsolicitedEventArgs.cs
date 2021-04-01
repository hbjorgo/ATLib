namespace HeboTech.ATLib.Parsers
{
    public class UnsolicitedEventArgs
    {
        public UnsolicitedEventArgs(string line1, string line2)
        {
            Line1 = line1;
            Line2 = line2;
        }

        public string Line1 { get; }
        public string Line2 { get; }
    }
}
