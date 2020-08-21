using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public class Communicator
    {
        private static ReadOnlyMemory<byte> NewLine => new byte[] { (byte)'\r', (byte)'\n' };
        private readonly IDuplexPipe duplexPipe;

        public Communicator(IDuplexPipe duplexPipe)
        {
            this.duplexPipe = duplexPipe;
        }

        public async ValueTask Write(string input, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            await duplexPipe.Output.WriteAsync(bytes, cancellationToken);
        }

        public Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            return ReadLineAsync(NewLine, cancellationToken);
        }

        public async Task<string> ReadLineAsync(ReadOnlyMemory<byte> delimiter, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                ReadResult result = await duplexPipe.Input.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = result.Buffer;

                try
                {
                    // Process all messages from the buffer, modifying the input buffer on each
                    // iteration.
                    while (TryParseLine(ref buffer, delimiter.Span, out ReadOnlySequence<byte> line))
                    {
                        return Encoding.UTF8.GetString(line.ToArray());
                    }

                    // There's no more data to be processed.
                    if (result.IsCompleted)
                    {
                        //if (buffer.Length > 0)
                        //{
                        //    // The message is incomplete and there's no more data to process.
                        //    throw new InvalidDataException("Incomplete message.");
                        //}
                        break;
                    }
                }
                finally
                {
                    // Since all messages in the buffer are being processed, you can use the
                    // remaining buffer's Start and End position to determine consumed and examined.
                    duplexPipe.Input.AdvanceTo(buffer.Start, buffer.Start);
                }
            }

            return default;
        }

        private static bool TryParseLine(ref ReadOnlySequence<byte> buffer, ReadOnlySpan<byte> delimiter, out ReadOnlySequence<byte> line)
        {
            var reader = new SequenceReader<byte>(buffer);

            if (reader.TryReadTo(out line, delimiter))
            {
                buffer = buffer.Slice(reader.Position);
                return true;
            }

            line = default;
            return false;
        }
    }
}
