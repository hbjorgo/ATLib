namespace HeboTech.ATLib.Events
{
    public class SmsReceivedEventArgs
    {
        public SmsReceivedEventArgs(string storage, int index)
        {
            Storage = storage;
            Index = index;
        }

        public string Storage { get; }
        public int Index { get; }
    }
}
