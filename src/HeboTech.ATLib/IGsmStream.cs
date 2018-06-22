using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    public interface IGsmStream
    {
        Task WriteAsync(string text);
        Task<(Status status, string payload)> GetStandardReplyAsync(int timeoutMs);
        Task<Status> GetCustomReplyAsync(string expectedReply, int timeoutMs);
    }
}
