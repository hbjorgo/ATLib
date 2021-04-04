using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class SerialPortChannel : AtChannel
    {
        private readonly SerialPort serialPort;
        private bool disposedValue;

        public SerialPortChannel(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        protected ValueTask<int> Read(char[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (serialPort.IsOpen && serialPort.BytesToRead > 0)
                return new ValueTask<int>(serialPort.Read(buffer, offset, serialPort.BytesToRead));
            return new ValueTask<int>(0);
        }

        protected override ValueTask<bool> Write(string text, CancellationToken cancellationToken = default)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(text);
                return new ValueTask<bool>(true);
            }
            return new ValueTask<bool>(false);
        }

        protected override ValueTask<bool> Write(char[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(input, offset, count);
                return new ValueTask<bool>(true);
            }
            return new ValueTask<bool>(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    serialPort.Close();
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                disposedValue = true;
            }
        }

        protected override ValueTask<string> ReadSingleMessageAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
