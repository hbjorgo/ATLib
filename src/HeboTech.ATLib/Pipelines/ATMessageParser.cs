using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace HeboTech.ATLib.Pipelines
{
    public class ATMessageParser : MessageParserBase<ATResult>
    {
        private const byte BYTE_CR = (byte)'\r';
        private const byte BYTE_NL = (byte)'\n';
        private const byte BYTE_O = (byte)'O';
        private const byte BYTE_K = (byte)'K';

        public ATMessageParser(PipeReader reader) : base(reader)
        {
        }

        protected override bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out ATResult message)
        {
            var reader = new SequenceReader<byte>(buffer);
            if (reader.TryReadTo(out ReadOnlySpan<byte> itemBytes, BYTE_CR, advancePastDelimiter: false)) // we have an item to handle
            {
                // Skip the line + the termination character \r.
                message = new ATResult(Encoding.UTF8.GetString(itemBytes));
                buffer = buffer.Slice(buffer.GetPosition(1, reader.Position));
                return true;
            }

            message = default;
            return false;
        }
    }
}
