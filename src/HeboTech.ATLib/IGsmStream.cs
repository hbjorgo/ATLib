namespace HeboTech.ATLib
{
    public interface IGsmStream
    {
        bool SendCheckReply(string send, string expectedReply, int timeout);
        string SendGetReply(string send, string terminationReply, int timeout);
    }
}
