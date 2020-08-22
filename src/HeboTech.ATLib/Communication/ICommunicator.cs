using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public interface ICommunicator
    {
        ValueTask<int> Read(char[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        ValueTask<bool> Write(string input, CancellationToken cancellationToken = default);
        ValueTask<bool> Write(char[] input, int offset, int count, CancellationToken cancellationToken = default);
    }
}