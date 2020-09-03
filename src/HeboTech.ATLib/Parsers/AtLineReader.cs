using Cyotek.Collections.Generic;
using HeboTech.ATLib.Communication;
using System;
using System.Linq;
using System.Threading;

namespace HeboTech.ATLib.Parsers
{
    public class AtLineReader
    {
        private readonly ICommunicator comm;
        private const int maxAtResponse = 8 * 1024;
        private bool closed;
        private readonly char[] readBuffer = new char[maxAtResponse];
        private readonly CircularBuffer<char> ringBuffer;

        public AtLineReader(ICommunicator comm)
        {
            this.comm = comm;
            ringBuffer = new CircularBuffer<char>(maxAtResponse, true);
        }

        public string ReadLine()
        {
            SkipLeadingNewLines();
            int eolCount = FindNextEOL();
            while (eolCount < 0 && !closed)
            {
                int readCount;
                do
                {
                    try
                    {
                        readCount = comm.Read(readBuffer, 0, readBuffer.Length).GetAwaiter().GetResult();
                    }
                    catch (OperationCanceledException)
                    {
                        return null;
                    }
                    Thread.Sleep(10);
                } while (readCount <= 0 && !closed);

                if (readCount > 0)
                {
                    ringBuffer.Put(readBuffer, 0, readCount);

                    SkipLeadingNewLines();
                    eolCount = FindNextEOL();
                }
                else if (readCount <= 0)
                {
                    // Read error encountered or EOF reached
                    // count == 0 => EOF reached
                    // count < 0 => Read error
                    return null;
                }
            }

            string line = new string(ringBuffer.Get(eolCount));
            return line;
        }

        private void SkipLeadingNewLines()
        {
            while (!ringBuffer.IsEmpty && (ringBuffer.Peek() == '\r' || ringBuffer.Peek() == '\n'))
                ringBuffer.Skip(1);
        }

        private int FindNextEOL()
        {
            if (ringBuffer.IsEmpty)
                return -1;

            // SMS prompt character... not \r terminated
            char[] tail = ringBuffer.Peek(3);
            if (tail.Length == 3 && tail[0] == '>' && tail[1] == ' ' && tail[2] == '\0')
                return 2;

            // Find next newline
            int i = 0;
            foreach (char c in ringBuffer)
            {
                if (c != '\0' && c != '\r' && c != '\n')
                    i++;
                else
                    break;
            }
            if (ringBuffer.ElementAt(i) == '\r' || ringBuffer.ElementAt(i) == '\n')
                return i;
            return -1;
        }

        public void Close()
        {
            closed = true;
        }
    }
}
