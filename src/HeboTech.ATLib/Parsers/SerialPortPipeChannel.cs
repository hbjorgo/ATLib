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
            SequenceReader<byte> sequenceReader = new SequenceReader<byte>(buffer);
            if (sequenceReader.TryReadTo(out ReadOnlySequence<byte> temp, eolSequence.AsSpan(), advancePastDelimiter: true))
            {
                line = Encoding.ASCII.GetString(temp.ToArray());
                buffer = buffer.Slice(sequenceReader.Position);
                return true;
            }
            else if (sequenceReader.TryReadTo(out _, smsPromptSequence.AsSpan(), advancePastDelimiter: true))
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
