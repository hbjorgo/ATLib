using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public interface ICommunicator
    {
        Task<string> ReadLineAsync(CancellationToken cancellationToken = default);
        Task<string> ReadLineAsync(ReadOnlyMemory<byte> delimiter, CancellationToken cancellationToken = default);
        ValueTask Write(string input, CancellationToken cancellationToken = default);
    }
}