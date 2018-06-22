using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
            if (!stream.CanTimeout)
                throw new ArgumentException("Stream must support timeout");
            if (!stream.CanWrite)
                throw new ArgumentException("Stream must support writing");
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public Task WriteAsync(string text)
        {
            byte[] bytesToWrite = encoding.GetBytes(text);
            return stream.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
        }

        public async Task<(Status status, string payload)> GetStandardReplyAsync(int timeoutMs)
        {
            stream.ReadTimeout = timeoutMs;
            int readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            string reply = encoding.GetString(buffer, 0, readBytes);

            if (string.IsNullOrWhiteSpace(reply))
                return (Status.ERROR, string.Empty);

            int statusPosition = reply.IndexOf(OK_RESPONSE);
            if (statusPosition < 0)
            {
                statusPosition = reply.IndexOf(ERROR_RESPONSE);
                if (statusPosition < 0)
                    return (Status.ERROR, string.Empty);
                else
                    return (Status.ERROR, reply.Substring(0, statusPosition));
            }
            else
                return (Status.OK, reply.Substring(0, statusPosition));
        }

        public async Task<Status> GetCustomReplyAsync(string expectedReply, int timeoutMs)
        {
            stream.ReadTimeout = timeoutMs;
            int readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            string reply = encoding.GetString(buffer, 0, readBytes);

            if (string.IsNullOrWhiteSpace(reply))
                return Status.ERROR;

            int statusPosition = reply.IndexOf(expectedReply);
            if (statusPosition < 0)
            {
                return Status.ERROR;
            }
            else
                return Status.OK;
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
