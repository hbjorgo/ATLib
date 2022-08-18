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
            //SequenceReader<byte> eolReader = new SequenceReader<byte>(buffer);
            //SequenceReader<byte> smsReader = new SequenceReader<byte>(buffer);
            int eolPosition = Find(buffer, out ReadOnlySequence<byte> eolSlice, eolSequence);
            int smsPosition = Find(buffer, out ReadOnlySequence<byte> smsSlice, smsPromptSequence);

            if (eolPosition != -1 && smsPosition != -1)
            {
                if (eolSlice.Length == smsSlice.Length)
                    throw new Exception("Conflicting line endings");
                if (eolSlice.Length < smsSlice.Length)
                    smsPosition = -1;
                else
                    eolPosition = -1;
            }

            if (eolPosition != -1)
            {
                string temp = Encoding.ASCII.GetString(eolSlice.ToArray());
                buffer = buffer.Slice(eolPosition);
                line = temp;
                return true;
            }
            else if (smsPosition != -1)
            {
                string temp = Encoding.ASCII.GetString(smsPromptSequence); // Return the SMS prompt sequence instead of an empty string (the sequence is consumed by the reader).
                buffer = buffer.Slice(smsPosition);
                line = temp;
                return true;
            }
            line = default;
            return false;
        }

        private static int Find(ReadOnlySequence<byte> sequence, out ReadOnlySequence<byte> slice, byte[] delimiter)
        {
            int i = 0;
            int d = 0;
            foreach (var memory in sequence)
            {
                foreach (var b in memory.Span)
                {
                    if (b == delimiter[d])
                        d++;
                    else
                        d = 0;
                    i++;
                    if (d == delimiter.Length)
                        break;
                }
            }
            slice = sequence.Slice(0, Math.Max(0, i - delimiter.Length));
            
            if (i == sequence.Length && d != delimiter.Length)
                return -1;

            return i; // We want to move past the delimiter
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
