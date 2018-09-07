using System;
using System.IO;
using System.Text;
using System.Threading;

namespace HeboTech.ATLib
{
    public class GsmStream : IGsmStream, IDisposable
    {
        private readonly Encoding encoding;
        private readonly Stream stream;
        private readonly int replyBufferSize;
        private readonly byte[] replybuffer;

        public GsmStream(Stream stream, Encoding encoding, int replyBufferSize = 255)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream must support reading");
            if (!stream.CanWrite)
                throw new ArgumentException("Stream must support writing");
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            this.replyBufferSize = replyBufferSize;
            replybuffer = new byte[this.replyBufferSize];
        }

        private void Write(string text)
        {
            byte[] bytesToWrite = encoding.GetBytes(text);
            stream.Write(bytesToWrite, 0, bytesToWrite.Length);
        }

        private void FlushInput()
        {
            while (stream.ReadByte() != -1)
                stream.ReadByte();
        }

        private string Readline(string expected, int timeout)
        {
            int expectedIndex = 0;
            int replyidx = 0;

            while (timeout-- > 0 && expectedIndex != expected.Length)
            {
                if (replyidx >= replyBufferSize - 1)
                {
                    break;
                }

                int b = 0;
                while ((b = stream.ReadByte()) > -1 && b <= replyBufferSize && expectedIndex != expected.Length)
                {
                    byte c = (byte)b;
                    if (c == expected[expectedIndex])
                        expectedIndex++;
                    else
                        expectedIndex = 0;

                    replybuffer[replyidx] = c;
                    replyidx++;
                }

                if (timeout == 0)
                {
                    break;
                }
                Thread.Sleep(1);
            }
            return encoding.GetString(replybuffer, 0, replyidx);
        }

        public bool SendCheckReply(string send, string expectedReply, int timeout)
        {
            FlushInput();
            Write(send);
            string reply = Readline(expectedReply, timeout);
            return reply == expectedReply;
        }

        public string SendGetReply(string send, string terminationReply, int timeout)
        {
            FlushInput();
            Write(send);
            string reply = Readline(terminationReply, timeout);
            return reply;
        }

        #region IDisposable
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    stream.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
