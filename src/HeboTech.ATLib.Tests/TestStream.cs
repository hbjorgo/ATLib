using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HeboTech.ATLib.Tests
{
    public class TestStream : Stream
    {
        public TestStream(Encoding encoding)
        {
            this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public event EventHandler<DataWrittenEvent> DataWritten;
        private Stack<byte> replyData = new Stack<byte>();
        private readonly Encoding encoding;

        public override bool CanRead => true;

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override int ReadByte()
        {
            byte retVal = 0;
            bool stackValue = replyData.TryPop(out retVal);
            return stackValue ? retVal : -1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            DataWritten?.Invoke(this, new DataWrittenEvent(encoding.GetString(buffer)));
        }

        public void SetReply(string data)
        {
            var bytes = encoding.GetBytes(data);
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                replyData.Push(bytes[i]);
            }
        }
    }
}
