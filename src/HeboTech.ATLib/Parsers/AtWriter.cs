using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class AtWriter : IAtWriter, IDisposable
    {
        private Stream stream;
        private bool isDisposed;

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

        public void Close()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    stream.Dispose();
                    stream = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                isDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AtWriter()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
