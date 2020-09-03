using HeboTech.ATLib.Communication;
using System;
using System.Threading;

namespace HeboTech.ATLib.Parsers
{
    public class AtLineReaderOld
    {
        private readonly ICommunicator comm;
        private const int maxAtResponse = 8 * 1024;
        private readonly char[] buffer = new char[maxAtResponse];
        private int headIndex = 0;
        private int tailIndex = 0;
        private bool closed;

        public AtLineReaderOld(ICommunicator comm)
        {
            this.comm = comm;
        }

        public string ReadLine()
        {
            int eolIndex = -1;

            if (0 == maxAtResponse - headIndex)
            {
                // Ditch buffer and start over again
                headIndex = 0;
                tailIndex = 0;
            }

            if (headIndex != tailIndex)
            {
                // Skip over leading newlines
                while ((buffer[tailIndex] == '\r' || buffer[tailIndex] == '\n') && tailIndex < headIndex)
                    tailIndex++;

                eolIndex = FindNextEOL(buffer, tailIndex, headIndex);
            }

            while (eolIndex < 0 && !closed)
            {
                int count;
                do
                {
                    try
                    {
                        count = comm.Read(buffer, headIndex, buffer.Length - headIndex).GetAwaiter().GetResult();
                    }
                    catch (OperationCanceledException oce)
                    {
                        return null;
                    }
                    Thread.Sleep(10);
                } while (count <= 0 && !closed);

                if (count > 0)
                {
                    headIndex += count;

                    // Skip over leading newlines
                    while (buffer[tailIndex] == '\r' || buffer[tailIndex] == '\n')
                        tailIndex++;

                    eolIndex = FindNextEOL(buffer, tailIndex, headIndex);
                }
                else if (count <= 0)
                {
                    // Read error encountered or EOF reached
                    // count == 0 => EOF reached
                    // count < 0 => Read error
                    return null;
                }
            }

            string line = new string(buffer, tailIndex, eolIndex - tailIndex);

            // Move remaining data to start of buffer
            Array.Copy(buffer, eolIndex, buffer, 0, headIndex - eolIndex);
            tailIndex = 0;
            headIndex -= eolIndex;

            return line;
        }

        private int FindNextEOL(char[] input, int startIndex, int endIndex)
        {
            if (startIndex == endIndex)
                return -1;

            if (startIndex + 2 < endIndex && input[startIndex] == '>' && input[startIndex + 1] == ' ' && input[startIndex + 2] == '\0')
            {
                // SMS prompt character... not \r terminated
                return startIndex + 2;
            }

            // Find next newline
            int i = startIndex;
            while (startIndex < endIndex && input[i] != '\0' && input[i] != '\r' && input[i] != '\n')
            {
                i++;
            }

            if (input[i] == '\r' || input[i] == '\n')
            {
                return i;
            }
            else
            {
                return -1;
            }
        }

        public void Close()
        {
            closed = true;
        }
    }
}
