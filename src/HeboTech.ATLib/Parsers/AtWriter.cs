using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class AtWriter : IAtWriter
    {
        private readonly Stream stream;

        public AtWriter(Stream stream)
        {
            this.stream = stream;
        }

        public async Task WriteLineAsync(string command, CancellationToken cancellationToken = default)
        {
            await WriteAsync(command);
            await WriteAsync("\r");
        }

        public async Task WriteSmsPduAndCtrlZAsync(string smsPdu, CancellationToken cancellationToken = default)
        {
            await WriteAsync(smsPdu);
            await WriteAsync("\x1A");
        }

        protected async Task WriteAsync(string text, CancellationToken cancellationToken = default)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }
}
