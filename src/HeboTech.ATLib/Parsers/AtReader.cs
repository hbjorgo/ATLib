using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class AtReader : IAtReader, IDisposable
    {
        private static readonly byte[] eolSequence = new byte[] { (byte)'\r', (byte)'\n' };
        private static readonly byte[] smsPromptSequence = new byte[] { (byte)'>', (byte)' ' };

        private bool isDisposed;
        private PipeReader pipe;
        private Channel<string> channel;
        private Task reader;
        private CancellationTokenSource cancellationTokenSource;

        public AtReader(Stream inputStream)
        {
            cancellationTokenSource = new CancellationTokenSource();
            pipe = PipeReader.Create(inputStream);
            channel = Channel.CreateUnbounded<string>();
        }

        public void Open()
        {
            reader = Task.Factory.StartNew(() => ReadPipeAsync(pipe, cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the current number of items available
        /// </summary>
        /// <returns></returns>
        public int AvailableItems()
        {
            return channel.Reader.Count;
        }

        public ValueTask<string> ReadAsync(CancellationToken cancellationToken = default)
        {
            return channel.Reader.ReadAsync(cancellationToken);
        }

        private async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ReadResult result;
                try
                {
                    result = await reader.ReadAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                ReadOnlySequence<byte> buffer = result.Buffer;

                string line;
                while (TryReadLine(ref buffer, out line))
                {
                    try
                    {
                        await channel.Writer.WriteAsync(line, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out string line)
        {
            SequenceReader<byte> eolReader = new SequenceReader<byte>(buffer);
            SequenceReader<byte> smsReader = new SequenceReader<byte>(buffer);
            bool eolSuccess = eolReader.TryReadTo(out ReadOnlySequence<byte> eolSlice, eolSequence.AsSpan(), advancePastDelimiter: true);
            bool smsSuccess = smsReader.TryReadTo(out ReadOnlySequence<byte> smsSlice, smsPromptSequence.AsSpan(), advancePastDelimiter: true);

            if (eolSuccess && smsSuccess)
            {
                if (eolSlice.Length == smsSlice.Length)
                    throw new Exception("Conflicting line endings");
                if (eolSlice.Length < smsSlice.Length)
                    smsSuccess = false;
                else
                    eolSuccess = false;
            }

            if (eolSuccess)
            {
                string temp = Encoding.ASCII.GetString(eolSlice.ToArray());
                buffer = buffer.Slice(eolReader.Position);
                line = temp;
                return true;
            }
            else if (smsSuccess)
            {
                string temp = Encoding.ASCII.GetString(smsPromptSequence); // Return the SMS prompt sequence instead of an empty string (the sequence is consumed by the reader).
                buffer = buffer.Slice(smsReader.Position);
                line = temp;
                return true;
            }
            line = default;
            return false;
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    cancellationTokenSource.Cancel();
                    reader?.Wait();
                    reader.Dispose();
                    reader = null;
                    channel = null;
                    pipe = null;
                    cancellationTokenSource.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                isDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AtReader()
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
        #endregion
    }
}
