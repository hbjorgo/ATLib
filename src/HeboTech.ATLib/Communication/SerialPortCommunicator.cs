using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public class SerialPortCommunicator : ICommunicator
    {
        private readonly SerialPort serialPort;

        public SerialPortCommunicator(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        public ValueTask<int> Read(char[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(serialPort.Read(buffer, offset, count));
        }

        public ValueTask Write(string text, CancellationToken cancellationToken = default)
        {
            serialPort.Write(text);
            return new ValueTask();
        }

        public ValueTask Write(char[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            serialPort.Write(input, offset, count);
            return new ValueTask();
        }
    }
}
