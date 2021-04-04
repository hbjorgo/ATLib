using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class SerialPortPipeChannel : AtChannel
    {
        private readonly byte[] eolSequence = new byte[] { (byte)'\r', (byte)'\n' };
        private readonly byte[] smsPromptSequence = new byte[] { (byte)'>', (byte)' ' };
        private readonly SerialPort serialPort;
        private readonly PipeReader reader;

        public SerialPortPipeChannel(SerialPort serialPort)
        {
            this.serialPort = serialPort;
            reader = PipeReader.Create(serialPort.BaseStream);
        }

        protected override async ValueTask<string> ReadSingleMessageAsync(CancellationToken cancellationToken = default)
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
                    if (TryReadLine(ref buffer, out string message))
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

            return null;
        }

        private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out string line)
        {
            // Get the index of EOL and SMS prompt
            long eolIndex = FindIndexOf(buffer, eolSequence.AsSpan());
            long smsPromptIndex = FindIndexOf(buffer, smsPromptSequence.AsSpan());

            // Read the first occurence of either EOL or SMS prompt
            if ((eolIndex >= 0 && smsPromptIndex < 0) || (eolIndex >= 0 && eolIndex < smsPromptIndex))
                return ReadLine(ref buffer, out line);
            else if ((smsPromptIndex >= 0 && eolIndex < 0) || (smsPromptIndex >= 0 && smsPromptIndex < eolIndex))
                return ReadSmsPrompt(ref buffer, out line);
            else
            {
                line = default;
                return false;
            }
        }

        /// <summary>
        /// Get the index of the first occurence of the segment
        /// </summary>
        /// <param name="buffer">The buffer to search in</param>
        /// <param name="data">The segment to find</param>
        /// <returns>Returns the index of the segment, or -1 if not found</returns>
        private long FindIndexOf(in ReadOnlySequence<byte> buffer, ReadOnlySpan<byte> data)
        {
            long position = 0;

            foreach (ReadOnlyMemory<byte> segment in buffer)
            {
                ReadOnlySpan<byte> span = segment.Span;
                var index = span.IndexOf(data);
                if (index != -1)
                {
                    return position + index;
                }

                position += span.Length;
            }

            return -1;
        }

        private bool ReadLine(ref ReadOnlySequence<byte> buffer, out string line)
        {
            SequenceReader<byte> sequenceReader = new SequenceReader<byte>(buffer);
            if (sequenceReader.TryReadTo(out ReadOnlySequence<byte> slice, eolSequence.AsSpan(), advancePastDelimiter: true))
            {
                string temp = Encoding.ASCII.GetString(slice.ToArray());
                buffer = buffer.Slice(sequenceReader.Position);
                line = temp;
                return true;
            }
            line = default;
            return false;
        }

        private bool ReadSmsPrompt(ref ReadOnlySequence<byte> buffer, out string line)
        {
            SequenceReader<byte> sequenceReader = new SequenceReader<byte>(buffer);
            if (sequenceReader.TryReadTo(out _, smsPromptSequence.AsSpan(), advancePastDelimiter: true))
            {
                line = Encoding.ASCII.GetString(smsPromptSequence);
                buffer = buffer.Slice(sequenceReader.Position);
                return true;
            }
            line = default;
            return false;
        }

        protected override ValueTask<bool> Write(string text, CancellationToken cancellationToken = default)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(text);
                return new ValueTask<bool>(true);
            }
            return new ValueTask<bool>(false);
        }

        protected override ValueTask<bool> Write(char[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(input, offset, count);
                return new ValueTask<bool>(true);
            }
            return new ValueTask<bool>(false);
        }
    }
}
