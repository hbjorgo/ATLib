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
            if (serialPort.IsOpen)
                return new ValueTask<int>(serialPort.Read(buffer, offset, count));
            return new ValueTask<int>(0);
        }

        public ValueTask<bool> Write(string text, CancellationToken cancellationToken = default)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(text);
                return new ValueTask<bool>(true);
            }
            return new ValueTask<bool>(false);
        }

        public ValueTask<bool> Write(char[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(input, offset, count);
                return new ValueTask<bool>(true);
            }
            return new ValueTask<bool>(false);
        }
    }
}
