using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public class FileDumper : ICommunicator
    {
        private readonly ICommunicator communicator;
        private readonly bool dumpInput;
        private readonly bool dumpOutput;
        private readonly StreamWriter writer;

        public FileDumper(ICommunicator communicator, string path, bool dumpInput = true, bool dumpOutput = false)
        {
            this.communicator = communicator;
            this.dumpInput = dumpInput;
            this.dumpOutput = dumpOutput;
            writer = new StreamWriter(path);
        }

        public async ValueTask<int> Read(char[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            var retVal = await communicator.Read(buffer, offset, count, cancellationToken);
            var input = new string(buffer, offset, retVal);

            if (dumpInput)
            {
                writer.WriteLine("IN:");
                writer.WriteLine(input);
            }

            return retVal;
        }

        public ValueTask<bool> Write(string input, CancellationToken cancellationToken = default)
        {
            var retVal = communicator.Write(input, cancellationToken);

            if (dumpOutput)
            {
                writer.WriteLine("OUT:");
                writer.WriteLine(input);
            }

            return retVal;
        }

        public async ValueTask<bool> Write(char[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            var retVal = await communicator.Write(input, offset, count, cancellationToken);
            var output = new string(input, offset, count);

            if (dumpOutput)
            {
                writer.WriteLine("OUT:");
                writer.WriteLine(output);
            }

            return retVal;
        }

        public void Close()
        {
            writer.Close();
        }
    }
}
