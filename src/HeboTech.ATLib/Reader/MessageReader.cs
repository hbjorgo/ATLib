using HeboTech.MessageReader;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace HeboTech.ATLib.Pipelines
{
    public class MessageReader : MessageReaderBase<string>
    {
        private const byte BYTE_HASH = (byte)'#';
        private const byte BYTE_CR = (byte)'\r';
        private const byte BYTE_NL = (byte)'\n';
        private const byte BYTE_O = (byte)'O';
        private const byte BYTE_K = (byte)'K';

        public MessageReader(PipeReader reader) : base(reader)
        {
        }

        protected override bool TryReadMessage(ref ReadOnlySequence<byte> buffer, byte[] delimiter, out string message)
        {
            var reader = new SequenceReader<byte>(buffer);
            if (reader.TryReadTo(out ReadOnlySequence<byte> itemBytes, delimiter, advancePastDelimiter: true)) // we have an item to handle
            {
                // Skip the line + the termination character \r.
                message = Encoding.UTF8.GetString(itemBytes.ToArray()) + Encoding.UTF8.GetString(delimiter);
                buffer = buffer.Slice(buffer.GetPosition(0, reader.Position));
                return true;
            }

            message = default;
            return false;
        }

        //protected override bool TryReadMessage(ref ReadOnlySequence<byte> buffer, out string message)
        //{
        //    var reader = new SequenceReader<byte>(buffer);
        //    if (reader.TryReadTo(out ReadOnlySpan<byte> itemBytes, BYTE_CR, advancePastDelimiter: true)) // we have an item to handle
        //    {
        //        // Skip the line + the termination character \r.
        //        message = Encoding.UTF8.GetString(itemBytes);
        //        buffer = buffer.Slice(buffer.GetPosition(0, reader.Position));
        //        return true;
        //    }

        //    message = default;
        //    return false;
        //}
    }
}
