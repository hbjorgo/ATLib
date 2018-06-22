namespace HeboTech.ATLib.Tests
{
    public class DataWrittenEvent
    {
        public DataWrittenEvent(string data)
        {
            this.Data = data;
        }

        public string Data { get; }
    }
}