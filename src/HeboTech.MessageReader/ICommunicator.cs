namespace HeboTech.MessageReader
{
    public interface ICommunicator<TMessage>
    {
        TMessage GetResponse(byte[] delimiter);
        void Write(string input);
    }
}