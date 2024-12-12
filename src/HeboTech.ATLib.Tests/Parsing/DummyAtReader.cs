using HeboTech.ATLib.Parsing;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Tests.Parsing
{
    internal class DummyAtReader : IAtReader
    {
        private readonly Channel<string> channel = Channel.CreateUnbounded<string>();

        public ValueTask<string> ReadAsync(CancellationToken cancellationToken = default)
        {
            return channel.Reader.ReadAsync(cancellationToken);
        }

        public bool QueueLine(string line)
        {
            return channel.Writer.TryWrite(line);
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public int AvailableItems()
        {
            return channel.Reader.Count;
        }
    }
}
