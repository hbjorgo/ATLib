using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public class ConsoleCommunicator : ICommunicator
    {
        public ValueTask<int> Read(char[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            string input = Console.ReadLine() + "\rOK\r";
            int charactersWritten = 0;
            while (charactersWritten < input.Length && charactersWritten < count)
            {
                buffer[offset + charactersWritten] = input[charactersWritten++];
            }
            return new ValueTask<int>(charactersWritten);
        }

        public ValueTask Write(string input, CancellationToken cancellationToken = default)
        {
            Console.WriteLine(input);
            return new ValueTask();
        }

        public ValueTask Write(char[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            Console.WriteLine(input, offset, count);
            return new ValueTask();
        }
    }
}
