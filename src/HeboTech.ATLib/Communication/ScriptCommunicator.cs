using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public class ScriptCommunicator : ICommunicator
    {
        private readonly Queue<Tuple<string, string>> inputs = new Queue<Tuple<string, string>>();
        private string nextInput;
        private StringBuilder output = new StringBuilder();

        public ScriptCommunicator()
        {
            inputs.Enqueue(new Tuple<string, string>("AT+CPIN?\r", "+CPIN:READY\r"));
            inputs.Enqueue(new Tuple<string, string>(string.Empty, "OK\r"));
        }

        public ValueTask<int> Read(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                if (nextInput != null)
                {
                    int charactersWritten = 0;
                    while (charactersWritten < nextInput.Length && charactersWritten < count)
                    {
                        buffer[offset + charactersWritten] = (byte)nextInput[charactersWritten++];
                    }
                    nextInput = null;
                    return new ValueTask<int>(charactersWritten);
                }
                Thread.Sleep(10);
            }
        }

        public ValueTask Write(string input, CancellationToken cancellationToken = default)
        {
            output.Append(input);
            SetNextInput();
            return new ValueTask();
        }

        public ValueTask Write(byte[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            output.Append(input.Select(b => (char)b).ToArray(), offset, count);
            SetNextInput();
            return new ValueTask();
        }

        private void SetNextInput()
        {
            if (output.ToString() == inputs.Peek().Item1)
            {
                nextInput = inputs.Dequeue().Item2;
                output.Clear();
            }
        }
    }
}
