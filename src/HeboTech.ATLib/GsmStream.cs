using System;
using System.IO;
using System.Text;
using System.Threading;

namespace HeboTech.ATLib
{
    public class GsmStream : IGsmStream, IDisposable
    {
        private Encoding encoding = Encoding.ASCII;
        private readonly Stream stream;
        private readonly byte[] buffer = new byte[1024];
        private const string OK_RESPONSE = "\r\nOK\r\n";
        private const string ERROR_RESPONSE = "\r\nERROR\r\n";

        public GsmStream(Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream must support reading");
            if (!stream.CanWrite)
                throw new ArgumentException("Stream must support writing");
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
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

        private string Readline(int timeout, bool multiline = false)
        {
            byte[] replybuffer = new byte[255];
            int replyidx = 0;

            while (timeout-- > 0)
            {
                if (replyidx >= 254)
                {
                    break;
                }

                int b = 0;
                while ((b = stream.ReadByte()) > -1 && b <= 256)
                {
                    byte c = (byte)b;
                    if (c == '\r') continue;
                    if (c == '\n')
                    {
                        if (replyidx == 0)   // the first \n is ignored
                            continue;

                        if (!multiline)
                        {
                            timeout = 0;         // the second \n is the end of the line
                            break;
                        }
                    }
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

        private (bool success, string reply) GetReply(string send, int timeout)
        {
            FlushInput();
            Write(send);
            string line = Readline(timeout);
            return (true, line);
        }

        public bool SendCheckReply(string send, string expectedReply, int timeout)
        {
            (bool success, string reply) = GetReply(send, timeout);
            if (!success) return false;
            return reply == expectedReply;
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
