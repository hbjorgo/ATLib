﻿using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace HeboTech.ATLib.Communication
{
    public class Communicator : CommunicatorBase<string>
    {
        private const byte BYTE_HASH = (byte)'#';
        private const byte BYTE_CR = (byte)'\r';
        private const byte BYTE_NL = (byte)'\n';
        private const byte BYTE_O = (byte)'O';
        private const byte BYTE_K = (byte)'K';

        public Communicator(IDuplexPipe duplexPipe) : base(duplexPipe)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="message"></param>
        /// <param name="delimiters">Returns the result based on the first match in the array</param>
        /// <returns></returns>
        protected override bool TryReadMessage(ref ReadOnlySequence<byte> buffer, out string message, byte[] delimiters)
        {
            // find the end-of-line marker
            SequencePosition? eol = null;
            byte delimiter = 0;
            foreach (byte del in delimiters)
            {
                eol = buffer.PositionOf(del);
                if (eol != null)
                {
                    delimiter = del;
                    break;
                }
            }
            if (eol == null)
            {
                message = default;
                return false;
            }

            var payload = buffer.Slice(0, eol.Value);
            message = Encoding.UTF8.GetString(payload.ToArray()) + (char)delimiter;
            buffer = buffer.Slice(buffer.GetPosition(1, eol.Value));
            return true;
        }

        //protected override bool TryReadMessage(ref ReadOnlySequence<byte> buffer, byte[] delimiter, out string message)
        //{
        //    var reader = new SequenceReader<byte>(buffer);
        //    if (reader.TryReadTo(out ReadOnlySequence<byte> itemBytes, delimiter, advancePastDelimiter: true)) // we have an item to handle
        //    {
        //        // Skip the line + the termination character \r.
        //        message = Encoding.UTF8.GetString(itemBytes.ToArray()) + Encoding.UTF8.GetString(delimiter);
        //        buffer = buffer.Slice(buffer.GetPosition(0, reader.Position));
        //        return true;
        //    }

        //    message = default;
        //    return false;
        //}

        //protected override bool TryReadMessage(ref ReadOnlySequence<byte> buffer, byte[] delimiter, out string message)
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
