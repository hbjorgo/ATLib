using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.StreamReader
{
    public static class Parser
    {
        public static async ValueTask<TMessage> ReadSingleMessageAsync<TMessage>(PipeReader reader, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = result.Buffer;

                // In the event that no message is parsed successfully, mark consumed
                // as nothing and examined as the entire buffer.
                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                try
                {
                    if (TryParseMessage(ref buffer, out TMessage message))
                    {
                        // A single message was successfully parsed so mark the start as the
                        // parsed buffer as consumed. TryParseMessage trims the buffer to
                        // point to the data after the message was parsed.
                        consumed = buffer.Start;

                        // Examined is marked the same as consumed here, so the next call
                        // to ReadSingleMessageAsync will process the next message if there's
                        // one.
                        examined = consumed;

                        return message;
                    }

                    // There's no more data to be processed.
                    if (result.IsCompleted)
                    {
                        if (buffer.Length > 0)
                        {
                            // The message is incomplete and there's no more data to process.
                            throw new InvalidDataException("Incomplete message.");
                        }

                        break;
                    }
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            return default;
        }

        private static bool TryParseMessage<TMessage>(ref ReadOnlySequence<byte> buffer, out TMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
