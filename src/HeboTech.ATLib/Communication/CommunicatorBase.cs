using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public abstract class CommunicatorBase<TMessage> : ICommunicator<TMessage>
    {
        private readonly IDuplexPipe duplexPipe;

        public CommunicatorBase(IDuplexPipe duplexPipe)
        {
            this.duplexPipe = duplexPipe;
        }

        public async ValueTask Write(string input, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            await duplexPipe.Output.WriteAsync(bytes, cancellationToken);
        }

        public async ValueTask<TMessage> ReadSingleMessageAsync(byte delimiter, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                ReadResult result = await duplexPipe.Input.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = result.Buffer;

                if (TryReadMessage(ref buffer, out TMessage message, delimiter))
                {
                    duplexPipe.Input.AdvanceTo(buffer.Start, buffer.Start);
                    return message;
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                duplexPipe.Input.AdvanceTo(buffer.Start, buffer.Start);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            return default;
        }

        protected abstract bool TryReadMessage(ref ReadOnlySequence<byte> buffer, out TMessage message, byte delimiter);
    }
}
