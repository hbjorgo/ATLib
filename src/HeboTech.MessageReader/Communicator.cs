using System.IO;
using System.Text;

namespace HeboTech.MessageReader
{
    public class Communicator<TMessage> : ICommunicator<TMessage>
    {
        private readonly Stream writeStream;
        private readonly MessageReaderBase<TMessage> reader;

        public Communicator(Stream writeStream, MessageReaderBase<TMessage> reader)
        {
            this.writeStream = writeStream;
            this.reader = reader;
        }

        public void Write(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            writeStream.Write(bytes, 0, bytes.Length);
        }

        public TMessage GetResponse(byte[] delimiter)
        {
            return reader.ReadSingleMessageAsync(delimiter).GetAwaiter().GetResult();
        }
    }
}
