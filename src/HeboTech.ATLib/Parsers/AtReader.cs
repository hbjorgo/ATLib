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
    public class AtReader : IAtReader
    {
        private static readonly byte[] eolSequence = new byte[] { (byte)'\r', (byte)'\n' };
        private static readonly byte[] smsPromptSequence = new byte[] { (byte)'>', (byte)' ' };

        private readonly PipeReader pipe;
        private Channel<string> channel;
        private Task reader;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AtReader(Stream inputStream)
        {
            cancellationTokenSource = new CancellationTokenSource();
            pipe = PipeReader.Create(inputStream);
            channel = Channel.CreateUnbounded<string>();
        }

        public void Start()
        {
            reader = Task.Factory.StartNew(() => ReadPipeAsync(pipe, cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            Task.WhenAll(reader);
        }

        public ValueTask<string> ReadAsync(CancellationToken cancellationToken = default)
        {
            return channel.Reader.ReadAsync(cancellationToken);
        }

        private async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ReadResult result = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = result.Buffer;

                string line;
                while (TryReadLine(ref buffer, out line))
                {
                    await channel.Writer.WriteAsync(line, cancellationToken);
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
    }
}
